using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Unity2019Mcp.Models;
using Unity2019Mcp.Utils;
using UnityEngine;

namespace Unity2019Mcp.Bridge
{
    public class McpHttpListener
    {
        private readonly string _prefix;
        private readonly int _timeoutMs;
        private HttpListener _listener;
        private Thread _thread;
        private bool _running;

        public McpHttpListener(string host, int port, int timeoutMs)
        {
            _prefix = "http://" + host + ":" + port + "/";
            _timeoutMs = timeoutMs;
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
