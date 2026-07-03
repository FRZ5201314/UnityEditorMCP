using UnityEditor;

namespace UnityMcp.Utils
{
    public static class BridgeSettings
    {
        private const string AllowDeleteKey = "UnityMcp.AllowDelete";
        private const string AllowScriptWriteKey = "UnityMcp.AllowScriptWrite";
        private const string AllowAssetDeleteKey = "UnityMcp.AllowAssetDelete";

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
