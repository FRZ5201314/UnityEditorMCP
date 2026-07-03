using System.Collections.Generic;
using UnityMcp.Utils;

namespace UnityMcp.Commands
{
    public static class BridgeCommands
    {
        public static object GetConfig(Dictionary<string, object> parameters)
        {
            return BridgeSettings.ToDto();
        }

        public static object GetLogPath(Dictionary<string, object> parameters)
        {
            return new { logPath = BridgeLogger.LogPath };
        }
    }
}
