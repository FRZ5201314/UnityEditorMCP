using System.Collections.Generic;
using UnityEditor;

namespace Unity2019Mcp.Commands
{
    public static class AssetCommands
    {
        public static object Refresh(Dictionary<string, object> parameters)
        {
            AssetDatabase.Refresh();
            return new { refreshed = true };
        }
    }
}
