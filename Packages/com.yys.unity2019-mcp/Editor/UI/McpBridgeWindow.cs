using System;
using System.IO;
using Unity2019Mcp.Bridge;
using Unity2019Mcp.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity2019Mcp.UI
{
    public class McpBridgeWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Unity 2019 MCP";
        private const double RefreshIntervalSeconds = 0.5;
        private const int MaxLogLines = 500;

        private Vector2 _logScroll;
        private long _lastSeenSequence;
        private double _lastRefreshTime;
        private string _logFilter = string.Empty;
        private bool _autoScroll = true;
        private bool _showInfo = true;
        private bool _showWarn = true;
        private bool _showError = true;
        private GUIStyle _logLineStyle;

        private readonly System.Collections.Generic.List<BridgeLogger.Entry> _entries = new System.Collections.Generic.List<BridgeLogger.Entry>();

        [MenuItem(MenuPath)]
        public static void ShowWindow()
        {
            var window = GetWindow<McpBridgeWindow>(false, "Unity 2019 MCP", true);
            window.minSize = new Vector2(420, 360);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            LoadInitialEntries();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            var now = EditorApplication.timeSinceStartup;
            if (now - _lastRefreshTime < RefreshIntervalSeconds)
            {
                return;
            }
            _lastRefreshTime = now;

            var current = BridgeLogger.CurrentSequence;
            if (current != _lastSeenSequence)
            {
                AppendNewEntries();
                Repaint();
            }
        }

        private void LoadInitialEntries()
        {
            _entries.Clear();
            _lastSeenSequence = 0;
            AppendNewEntries();
        }

        private void AppendNewEntries()
        {
            var newEntries = BridgeLogger.GetEntriesSince(_lastSeenSequence);
            if (newEntries.Length == 0)
            {
                _lastSeenSequence = BridgeLogger.CurrentSequence;
                return;
            }

            foreach (var entry in newEntries)
            {
                _entries.Add(entry);
                _lastSeenSequence = entry.sequence;
            }
            while (_entries.Count > MaxLogLines)
            {
                _entries.RemoveAt(0);
            }
        }

        private void OnGUI()
        {
            DrawConnectionSection();
            EditorGUILayout.Space();
            DrawPermissionsSection();
            EditorGUILayout.Space();
            DrawLogSection();
            EditorGUILayout.Space();
            DrawFooter();
        }

        private void DrawConnectionSection()
        {
            EditorGUILayout.LabelField("连接状态", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Bridge 运行中", McpBridgeServer.IsRunning);
                EditorGUILayout.TextField("监听地址", McpBridgeServer.CurrentPrefix ?? "(未启动)");
            }

            EditorGUILayout.LabelField("Unity 版本", Application.unityVersion);
            EditorGUILayout.LabelField("工程名称", Application.productName);
            EditorGUILayout.LabelField("工程路径", GetProjectPath());

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(McpBridgeServer.IsRunning))
                {
                    if (GUILayout.Button("启动 Bridge"))
                    {
                        McpBridgeServer.Start();
                    }
                }
                using (new EditorGUI.DisabledScope(!McpBridgeServer.IsRunning))
                {
                    if (GUILayout.Button("停止 Bridge"))
                    {
                        McpBridgeServer.Stop();
                    }
                    if (GUILayout.Button("复制地址"))
                    {
                        var prefix = McpBridgeServer.CurrentPrefix;
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            EditorGUIUtility.systemCopyBuffer = prefix.TrimEnd('/');
                        }
                    }
                }
            }
        }

        private void DrawPermissionsSection()
        {
            EditorGUILayout.LabelField("Bridge Permissions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("这些开关只限制 MCP Bridge 命令入口，不限制 Codex、Shell、Unity UI 或文件系统层面的其他操作。", MessageType.None);

            var sceneDelete = EditorGUILayout.ToggleLeft("Allow Scene Object Delete (gameObject.delete / component.remove)", BridgeSettings.AllowSceneDelete);
            if (sceneDelete != BridgeSettings.AllowSceneDelete)
            {
                BridgeSettings.AllowSceneDelete = sceneDelete;
                BridgeLogger.Info("allowSceneDelete set to " + sceneDelete);
            }

            var scriptWrite = EditorGUILayout.ToggleLeft("Allow Script Write (script.create)", BridgeSettings.AllowScriptWrite);
            if (scriptWrite != BridgeSettings.AllowScriptWrite)
            {
                BridgeSettings.AllowScriptWrite = scriptWrite;
                BridgeLogger.Info("allowScriptWrite set to " + scriptWrite);
            }

            var assetDelete = EditorGUILayout.ToggleLeft("Allow Asset Delete (asset.delete)", BridgeSettings.AllowAssetDelete);
            if (assetDelete != BridgeSettings.AllowAssetDelete)
            {
                BridgeSettings.AllowAssetDelete = assetDelete;
                BridgeLogger.Info("allowAssetDelete set to " + assetDelete);
            }
        }

        private void DrawLogSection()
        {
            EnsureLogLineStyle();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Bridge 日志", EditorStyles.boldLabel, GUILayout.Width(80));
                _showInfo = GUILayout.Toggle(_showInfo, "INFO", EditorStyles.miniButtonLeft);
                _showWarn = GUILayout.Toggle(_showWarn, "WARN", EditorStyles.miniButtonMid);
                _showError = GUILayout.Toggle(_showError, "ERROR", EditorStyles.miniButtonRight);
                GUILayout.FlexibleSpace();
                _autoScroll = GUILayout.Toggle(_autoScroll, "自动滚动", EditorStyles.toolbarButton, GUILayout.Width(70));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("过滤", GUILayout.Width(40));
                _logFilter = EditorGUILayout.TextField(_logFilter ?? string.Empty);
                if (GUILayout.Button("清空", GUILayout.Width(50)))
                {
                    _entries.Clear();
                    BridgeLogger.ClearBuffer();
                    _lastSeenSequence = BridgeLogger.CurrentSequence;
                }
                if (GUILayout.Button("打开日志文件", GUILayout.Width(100)))
                {
                    OpenLogFile();
                }
            }

            var rect = EditorGUILayout.GetControlRect(false, Mathf.Max(160f, position.height - 320f));
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            const float padding = 4f;
            const float scrollbarWidth = 16f;
            var view = new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);
            var contentWidth = view.width - scrollbarWidth;

            var filter = string.IsNullOrEmpty(_logFilter) ? null : _logFilter;
            var visible = new System.Collections.Generic.List<VisibleEntry>(_entries.Count);
            var totalHeight = 0f;
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (!IsLevelVisible(entry.level))
                {
                    continue;
                }
                if (filter != null && entry.message.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var label = "[" + entry.timeUtc.ToLocalTime().ToString("HH:mm:ss") + "] " + entry.level + " " + entry.message;
                var content = new GUIContent(label);
                var height = _logLineStyle.CalcHeight(content, contentWidth);
                visible.Add(new VisibleEntry { content = content, level = entry.level, height = height, top = totalHeight });
                totalHeight += height;
            }

            if (totalHeight < 1f)
            {
                totalHeight = EditorGUIUtility.singleLineHeight;
            }

            var scrollContent = new Rect(0, 0, contentWidth, totalHeight);

            if (_autoScroll)
            {
                _logScroll.y = Mathf.Max(0, totalHeight - view.height);
            }

            _logScroll = GUI.BeginScrollView(view, _logScroll, scrollContent);
            for (var i = 0; i < visible.Count; i++)
            {
                var item = visible[i];
                if (item.top + item.height < _logScroll.y || item.top > _logScroll.y + view.height)
                {
                    continue;
                }
                var lineRect = new Rect(0, item.top, contentWidth, item.height);
                var oldColor = GUI.color;
                GUI.color = ColorForLevel(item.level);
                GUI.Label(lineRect, item.content, _logLineStyle);
                GUI.color = oldColor;
            }
            GUI.EndScrollView();
        }

        private struct VisibleEntry
        {
            public GUIContent content;
            public string level;
            public float height;
            public float top;
        }

        private void EnsureLogLineStyle()
        {
            if (_logLineStyle != null)
            {
                return;
            }
            _logLineStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = false,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(2, 2, 1, 1),
            };
        }

        private void DrawFooter()
        {
            EditorGUILayout.LabelField("端口范围", McpBridgeServer.Host + ":" + McpBridgeServer.PreferredPort + "-" + McpBridgeServer.MaxPort);
            EditorGUILayout.SelectableLabel("日志文件: " + GetAbsoluteLogPath(), EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        private bool IsLevelVisible(string level)
        {
            switch (level)
            {
                case "INFO": return _showInfo;
                case "WARN": return _showWarn;
                case "ERROR": return _showError;
                default: return true;
            }
        }

        private static Color ColorForLevel(string level)
        {
            switch (level)
            {
                case "WARN": return new Color(1f, 0.85f, 0.4f);
                case "ERROR": return new Color(1f, 0.5f, 0.45f);
                default: return EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f) : Color.black;
            }
        }

        private static string GetProjectPath()
        {
            var dataPath = Application.dataPath;
            if (dataPath.EndsWith("/Assets"))
            {
                return dataPath.Substring(0, dataPath.Length - "/Assets".Length);
            }
            return dataPath;
        }

        private static string GetAbsoluteLogPath()
        {
            try
            {
                return Path.GetFullPath(BridgeLogger.LogPath);
            }
            catch
            {
                return BridgeLogger.LogPath;
            }
        }

        private static void OpenLogFile()
        {
            var path = GetAbsoluteLogPath();
            if (!File.Exists(path))
            {
                BridgeLogger.Info("Log file not yet created: " + path);
                EditorUtility.DisplayDialog("Unity 2019 MCP", "日志文件还未生成: " + path, "确定");
                return;
            }
            EditorUtility.RevealInFinder(path);
        }
    }
}
