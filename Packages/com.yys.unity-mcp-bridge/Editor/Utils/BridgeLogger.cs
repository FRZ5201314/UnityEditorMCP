using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityMcp.Utils
{
    public static class BridgeLogger
    {
        public static readonly string LogPath = Path.Combine("Library", "UnityMcp", "bridge.log");
        private static readonly object LockObject = new object();

        public struct Entry
        {
            public long sequence;
            public DateTime timeUtc;
            public string level;
            public string message;
        }

        private const int BufferCapacity = 500;
        private static readonly object BufferLock = new object();
        private static readonly LinkedList<Entry> Buffer = new LinkedList<Entry>();
        private static long _sequence;

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

        public static long CurrentSequence
        {
            get
            {
                lock (BufferLock)
                {
                    return _sequence;
                }
            }
        }

        public static Entry[] GetEntriesSince(long lastSequence, int maxCount = BufferCapacity)
        {
            lock (BufferLock)
            {
                if (Buffer.Count == 0)
                {
                    return new Entry[0];
                }

                var result = new List<Entry>(Math.Min(Buffer.Count, maxCount));
                foreach (var entry in Buffer)
                {
                    if (entry.sequence <= lastSequence)
                    {
                        continue;
                    }
                    result.Add(entry);
                    if (result.Count >= maxCount)
                    {
                        break;
                    }
                }
                return result.ToArray();
            }
        }

        public static void ClearBuffer()
        {
            lock (BufferLock)
            {
                Buffer.Clear();
            }
        }

        private static void Write(string level, string message)
        {
            var now = DateTime.UtcNow;
            Entry entry;
            lock (BufferLock)
            {
                _sequence++;
                entry = new Entry
                {
                    sequence = _sequence,
                    timeUtc = now,
                    level = level,
                    message = message ?? string.Empty
                };
                Buffer.AddLast(entry);
                while (Buffer.Count > BufferCapacity)
                {
                    Buffer.RemoveFirst();
                }
            }

            try
            {
                lock (LockObject)
                {
                    var directory = Path.GetDirectoryName(LogPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(LogPath, entry.timeUtc.ToString("o") + " [" + level + "] " + entry.message + "\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Unity MCP failed to write log: " + ex.Message);
            }
        }
    }
}
