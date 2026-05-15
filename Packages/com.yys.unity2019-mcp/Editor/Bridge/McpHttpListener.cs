using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Unity2019Mcp.Models;
using Unity2019Mcp.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity2019Mcp.Bridge
{
    public class McpHttpListener
    {
        private class ScriptAttachWaitStatus
        {
            public bool isUpdating;
            public bool isCompiling;
            public bool typeFound;
            public int candidateCount;
            public string[] candidates;
        }

        private class CapturedLog
        {
            public string timeUtc;
            public string type;
            public string condition;
            public string stackTrace;
        }

        private readonly string _prefix;
        private readonly int _timeoutMs;
        private HttpListener _listener;
        private Thread _thread;
        private bool _running;
        private static readonly object CapturedLogsLock = new object();
        private static readonly System.Collections.Generic.List<CapturedLog> CapturedLogs = new System.Collections.Generic.List<CapturedLog>();
        private static bool _logCaptureInitialized;

        public McpHttpListener(string host, int port, int timeoutMs)
        {
            _prefix = "http://" + host + ":" + port + "/";
            _timeoutMs = timeoutMs;
            EnsureLogCaptureInitialized();
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _listener.Start();
            _running = true;
            _thread = new Thread(ListenLoop);
            _thread.IsBackground = true;
            _thread.Start();
            Debug.Log("Unity2019MCP bridge listening on " + _prefix);
        }

        public void Stop()
        {
            _running = false;
            if (_listener != null)
            {
                _listener.Close();
                _listener = null;
            }
        }

        private void ListenLoop()
        {
            while (_running && _listener != null)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => Handle(context));
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Debug.LogError("Unity2019MCP listener error: " + ex);
                    }
                }
            }
        }

        private void Handle(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/health")
                {
                    var health = MainThreadDispatcher.Invoke(
                        () => new
                        {
                            ok = true,
                            service = "Unity2019MCP",
                            unityVersion = Application.unityVersion
                        },
                        _timeoutMs);
                    WriteJson(context, 200, health);
                    return;
                }

                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/command")
                {
                    var body = ReadBody(context.Request);
                    var request = JsonUtil.FromJson<McpCommandRequest>(body);
                    var waitError = WaitForCompilationIfNeeded(request);
                    if (waitError != null)
                    {
                        WriteJson(context, 200, waitError);
                        return;
                    }

                    var response = (McpCommandResponse)MainThreadDispatcher.Invoke(
                        () => McpCommandDispatcher.Execute(request),
                        _timeoutMs);
                    WriteJson(context, 200, response);
                    return;
                }

                WriteJson(context, 404, new { ok = false, error = "Not found" });
            }
            catch (Exception ex)
            {
                WriteJson(context, 500, McpCommandResponse.Fail(null, "OPERATION_FAILED", ex.Message, ex.ToString()));
            }
        }

        private static string ReadBody(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        private McpCommandResponse WaitForCompilationIfNeeded(McpCommandRequest request)
        {
            if (request == null || request.command != "script.attach")
            {
                return null;
            }

            var timeoutMs = ParamUtil.Get(request.@params, "compileTimeoutMs", _timeoutMs);
            var typeName = ParamUtil.Get<string>(request.@params, "typeName", null);
            var start = DateTime.UtcNow;
            var lastStatus = new ScriptAttachWaitStatus();
            while (true)
            {
                lastStatus = (ScriptAttachWaitStatus)MainThreadDispatcher.Invoke(
                    () => GetScriptAttachWaitStatus(typeName),
                    1000);
                if (lastStatus.typeFound && !lastStatus.isCompiling)
                {
                    return null;
                }

                if (!lastStatus.isUpdating && !lastStatus.isCompiling && lastStatus.candidateCount > 1)
                {
                    return McpCommandResponse.Fail(
                        request.id,
                        "TYPE_AMBIGUOUS",
                        "Script component type is ambiguous: " + typeName,
                        BuildScriptAttachFailureDetails(typeName, timeoutMs, start, lastStatus, "type_ambiguous"));
                }

                if ((DateTime.UtcNow - start).TotalMilliseconds >= timeoutMs)
                {
                    var reason = lastStatus.isUpdating || lastStatus.isCompiling ? "unity_compiling" : "type_not_available";
                    return McpCommandResponse.Fail(
                        request.id,
                        lastStatus.isUpdating || lastStatus.isCompiling ? "UNITY_COMPILING" : "SCRIPT_COMPILE_FAILED",
                        lastStatus.isUpdating || lastStatus.isCompiling
                            ? "Unity is still importing or compiling after " + timeoutMs + "ms."
                            : "Script component type is not available after " + timeoutMs + "ms: " + typeName,
                        BuildScriptAttachFailureDetails(typeName, timeoutMs, start, lastStatus, reason));
                }

                Thread.Sleep(200);
            }
        }

        private static ScriptAttachWaitStatus GetScriptAttachWaitStatus(string typeName)
        {
            EditorApplication.QueuePlayerLoopUpdate();

            var candidates = new System.Collections.Generic.List<string>();
            var type = TypeResolver.ResolveComponentType(typeName, out candidates);
            return new ScriptAttachWaitStatus
            {
                isUpdating = EditorApplication.isUpdating,
                isCompiling = EditorApplication.isCompiling,
                typeFound = type != null,
                candidateCount = candidates.Count,
                candidates = candidates.ToArray()
            };
        }

        private static object BuildScriptAttachFailureDetails(
            string typeName,
            int timeoutMs,
            DateTime start,
            ScriptAttachWaitStatus status,
            string reason)
        {
            return new
            {
                reason = reason,
                typeName = typeName,
                timeoutMs = timeoutMs,
                elapsedMs = (int)(DateTime.UtcNow - start).TotalMilliseconds,
                isUpdating = status.isUpdating,
                isCompiling = status.isCompiling,
                typeFound = status.typeFound,
                candidateCount = status.candidateCount,
                candidates = status.candidates,
                recentErrors = GetRecentCapturedErrors(start.AddSeconds(-2)),
                hint = "Check Unity Console compile errors and ensure the class name, namespace, file name, and MonoBehaviour inheritance match the requested typeName."
            };
        }

        private static void EnsureLogCaptureInitialized()
        {
            if (_logCaptureInitialized)
            {
                return;
            }

            _logCaptureInitialized = true;
            Application.logMessageReceived += CaptureLogMessage;
        }

        private static void CaptureLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
            {
                return;
            }

            lock (CapturedLogsLock)
            {
                CapturedLogs.Add(new CapturedLog
                {
                    timeUtc = DateTime.UtcNow.ToString("o"),
                    type = type.ToString(),
                    condition = condition,
                    stackTrace = stackTrace
                });

                while (CapturedLogs.Count > 50)
                {
                    CapturedLogs.RemoveAt(0);
                }
            }
        }

        private static CapturedLog[] GetRecentCapturedErrors(DateTime sinceUtc)
        {
            var recent = new System.Collections.Generic.List<CapturedLog>();
            lock (CapturedLogsLock)
            {
                foreach (var log in CapturedLogs)
                {
                    DateTime logTime;
                    if (!DateTime.TryParse(log.timeUtc, out logTime) || logTime < sinceUtc)
                    {
                        continue;
                    }

                    recent.Add(log);
                }
            }

            return recent.ToArray();
        }

        private static void WriteJson(HttpListenerContext context, int statusCode, object payload)
        {
            var json = JsonUtil.ToJson(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }
    }
}
