using UnityMcp.Utils;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityMcp.Bridge
{
    [InitializeOnLoad]
    public static class McpBridgeServer
    {
        public const string Host = "127.0.0.1";
        public const int PreferredPort = 8765;
        public const int MaxPort = 8775;
        private const int TimeoutMs = 30000;
        private static McpHttpListener _listener;
        private static bool _retryQueued;

        public static bool IsRunning
        {
            get { return _listener != null; }
        }

        public static string CurrentPrefix
        {
            get { return _listener != null ? _listener.Prefix : null; }
        }

        static McpBridgeServer()
        {
            MainThreadDispatcher.Initialize();
            Start();
            EditorApplication.delayCall += Start;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += Stop;
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            MainThreadDispatcher.Initialize();
            Start();
            EditorApplication.delayCall += Start;
        }

        public static void Start()
        {
            if (_listener != null)
            {
                return;
            }

            for (var port = PreferredPort; port <= MaxPort; port++)
            {
                try
                {
                    _listener = new McpHttpListener(Host, port, TimeoutMs);
                    _listener.Start();
                    BridgeLogger.Info("Bridge started on " + _listener.Prefix);
                    return;
                }
                catch (System.Exception ex)
                {
                    _listener = null;
                    BridgeLogger.Warning("Failed to start bridge on port " + port + ": " + ex.Message);
                    if (port == MaxPort)
                    {
                        Debug.LogError("Failed to start Unity MCP bridge: " + ex.Message);
                        QueueStartRetry();
                    }
                }
            }
        }

        public static void Stop()
        {
            if (_listener == null)
            {
                return;
            }

            _listener.Stop();
            _listener = null;
            BridgeLogger.Info("Bridge stopped.");
            Debug.Log("Unity MCP bridge stopped.");
        }

        private static void QueueStartRetry()
        {
            if (_retryQueued)
            {
                return;
            }

            _retryQueued = true;
            EditorApplication.delayCall += () =>
            {
                _retryQueued = false;
                Start();
            };
        }
    }
}
