using System;
using System.IO;
using UnityEngine;

namespace Unity2019Mcp.Utils
{
    public static class BridgeLogger
    {
        public static readonly string LogPath = Path.Combine("Library", "Unity2019Mcp", "bridge.log");
        private static readonly object LockObject = new object();

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Warning(string message)
        {
            Write("WARN", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (LockObject)
                {
                    var directory = Path.GetDirectoryName(LogPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(LogPath, DateTime.UtcNow.ToString("o") + " [" + level + "] " + message + "\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Unity2019MCP failed to write log: " + ex.Message);
            }
        }
    }
}
