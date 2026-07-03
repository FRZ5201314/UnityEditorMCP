using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;

namespace UnityMcp.Utils
{
    public static class MainThreadDispatcher
    {
        private class WorkItem
        {
            public Func<object> Func;
            public ManualResetEvent Done;
            public object Result;
            public Exception Exception;
        }

        private static readonly Queue<WorkItem> Queue = new Queue<WorkItem>();
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            EditorApplication.update += Update;
        }

        public static object Invoke(Func<object> func, int timeoutMs)
        {
            Initialize();

            var item = new WorkItem { Func = func, Done = new ManualResetEvent(false) };
            lock (Queue)
            {
                Queue.Enqueue(item);
            }

            if (!item.Done.WaitOne(timeoutMs))
            {
                throw new TimeoutException("Timed out waiting for Unity main thread.");
            }

            if (item.Exception != null)
            {
                throw item.Exception;
            }

            return item.Result;
        }

        private static void Update()
        {
            while (true)
            {
                WorkItem item = null;
                lock (Queue)
                {
                    if (Queue.Count > 0)
                    {
                        item = Queue.Dequeue();
                    }
                }

                if (item == null)
                {
                    break;
                }

                try
                {
                    item.Result = item.Func();
                }
                catch (Exception ex)
                {
                    item.Exception = ex;
                }
                finally
                {
                    item.Done.Set();
                }
            }
        }
    }
}
