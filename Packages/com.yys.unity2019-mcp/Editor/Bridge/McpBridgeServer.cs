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
        private const int PreferredPort = 8765;
        private const int MaxPort = 8775;
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
                        Debug.LogError("Failed to start Unity2019MCP bridge: " + ex.Message);
                        QueueStartRetry();
                    }
                }
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
            BridgeLogger.Info("Bridge stopped.");
            Debug.Log("Unity2019MCP bridge stopped.");
        }

        private const string PermissionsMenuRoot = "Tools/Unity 2019 MCP/Bridge Permissions/";
        private const string AllowSceneDeleteMenu = PermissionsMenuRoot + "Allow Scene Object Delete";
        private const string AllowScriptWriteMenu = PermissionsMenuRoot + "Allow Script Write";
        private const string AllowAssetDeleteMenu = PermissionsMenuRoot + "Allow Asset Delete";

        [MenuItem(AllowSceneDeleteMenu)]
        public static void ToggleAllowSceneDelete()
        {
            BridgeSettings.AllowSceneDelete = !BridgeSettings.AllowSceneDelete;
            BridgeLogger.Info("allowSceneDelete set to " + BridgeSettings.AllowSceneDelete);
        }

        [MenuItem(AllowSceneDeleteMenu, true)]
        public static bool ValidateAllowSceneDelete()
        {
            Menu.SetChecked(AllowSceneDeleteMenu, BridgeSettings.AllowSceneDelete);
            return true;
        }

        [MenuItem(AllowScriptWriteMenu)]
        public static void ToggleAllowScriptWrite()
        {
            BridgeSettings.AllowScriptWrite = !BridgeSettings.AllowScriptWrite;
            BridgeLogger.Info("allowScriptWrite set to " + BridgeSettings.AllowScriptWrite);
        }

        [MenuItem(AllowScriptWriteMenu, true)]
        public static bool ValidateAllowScriptWrite()
        {
            Menu.SetChecked(AllowScriptWriteMenu, BridgeSettings.AllowScriptWrite);
            return true;
        }

        [MenuItem(AllowAssetDeleteMenu)]
        public static void ToggleAllowAssetDelete()
        {
            BridgeSettings.AllowAssetDelete = !BridgeSettings.AllowAssetDelete;
            BridgeLogger.Info("allowAssetDelete set to " + BridgeSettings.AllowAssetDelete);
        }

        [MenuItem(AllowAssetDeleteMenu, true)]
        public static bool ValidateAllowAssetDelete()
        {
            Menu.SetChecked(AllowAssetDeleteMenu, BridgeSettings.AllowAssetDelete);
            return true;
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
