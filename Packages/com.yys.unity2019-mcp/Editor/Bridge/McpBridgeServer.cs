using Unity2019Mcp.Utils;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity2019Mcp.Bridge
{
    [InitializeOnLoad]
    public static class McpBridgeServer
    {
        private const string Host = "127.0.0.1";
        private const int Port = 8765;
        private const int TimeoutMs = 30000;
        private static McpHttpListener _listener;
        private static bool _retryQueued;

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

        [MenuItem("Tools/Unity 2019 MCP/Start Bridge")]
        public static void Start()
        {
            if (_listener != null)
            {
                return;
            }

            try
            {
                _listener = new McpHttpListener(Host, Port, TimeoutMs);
                _listener.Start();
            }
            catch (System.Exception ex)
            {
                _listener = null;
                Debug.LogError("Failed to start Unity2019MCP bridge: " + ex.Message);
                QueueStartRetry();
            }
        }

        [MenuItem("Tools/Unity 2019 MCP/Stop Bridge")]
        public static void Stop()
        {
            if (_listener == null)
            {
                return;
            }

            _listener.Stop();
            _listener = null;
            Debug.Log("Unity2019MCP bridge stopped.");
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
