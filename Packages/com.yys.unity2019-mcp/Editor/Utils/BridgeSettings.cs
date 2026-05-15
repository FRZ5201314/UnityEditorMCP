using UnityEditor;

namespace Unity2019Mcp.Utils
{
    public static class BridgeSettings
    {
        private const string AllowDeleteKey = "Unity2019Mcp.AllowDelete";
        private const string AllowScriptWriteKey = "Unity2019Mcp.AllowScriptWrite";
        private const string AllowAssetDeleteKey = "Unity2019Mcp.AllowAssetDelete";

        public static bool AllowSceneDelete
        {
            get { return EditorPrefs.GetBool(AllowDeleteKey, true); }
            set { EditorPrefs.SetBool(AllowDeleteKey, value); }
        }

        public static bool AllowDelete
        {
            get { return AllowSceneDelete; }
            set { AllowSceneDelete = value; }
        }

        public static bool AllowScriptWrite
        {
            get { return EditorPrefs.GetBool(AllowScriptWriteKey, true); }
            set { EditorPrefs.SetBool(AllowScriptWriteKey, value); }
        }

        public static bool AllowAssetDelete
        {
            get { return EditorPrefs.GetBool(AllowAssetDeleteKey, true); }
            set { EditorPrefs.SetBool(AllowAssetDeleteKey, value); }
        }

        public static object ToDto()
        {
            return new
            {
                allowSceneDelete = AllowSceneDelete,
                allowDelete = AllowSceneDelete,
                allowScriptWrite = AllowScriptWrite,
                allowAssetDelete = AllowAssetDelete,
                logPath = BridgeLogger.LogPath
            };
        }
    }
}
