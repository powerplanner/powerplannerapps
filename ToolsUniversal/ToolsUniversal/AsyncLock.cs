using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsUniversal
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                        m_releaser :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            m_releaser.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }


    /// <summary>
    /// Should only be used on one level of calls. Otherwise locking could get messed up with await calls.
    /// </summary>
    //public class AsyncLock : IDisposable
    //{
    //    private static class MyMonitor
    //    {
    //        private static Dictionary<object, Queue<int>> _objects = new Dictionary<object, Queue<int>>();
    //        private static object _lock = new object();

    //        public static void Enter(object obj)
    //        {
    //            Queue<int> queue = null;

    //            lock (_lock)
    //            {
    //                if (_objects.TryGetValue(obj, out queue))
    //                {
    //                    queue.Enqueue(Thread.CurrentThread.ManagedThreadId);
    //                }

    //                else
    //                {
    //                    queue = new Queue<int>();
    //                    queue.Enqueue(Thread.CurrentThread.ManagedThreadId);
    //                    _objects[obj] = queue;
    //                }
    //            }

    //            //make it wait
    //            while (queue.Peek() != Thread.CurrentThread.ManagedThreadId)
    //                ;
    //        }

    //        public static void Exit(object obj)
    //        {
    //            lock (_lock)
    //            {
    //                Queue<int> queue = null;

    //                if (_objects.TryGetValue(obj, out queue))
    //                {
    //                    if (queue.Count <= 1)
    //                        _objects.Remove(obj);

    //                    else
    //                        queue.Dequeue();
    //                }
    //            }
    //        }
    //    }

    //    public static AsyncLock Lock(object obj)
    //    {
    //        return new AsyncLock(obj);
    //    }

    //    private object _obj;
    //    private bool _lockWasTaken;

    //    public AsyncLock(object obj)
    //    {
    //        _obj = obj;

    //        try
    //        {
    //            MyMonitor.Enter(obj);
    //            _lockWasTaken = true;
    //            //_lockWasTaken = Monitor.TryEnter(obj, int.MaxValue);
    //            //Monitor.Enter(obj);
    //            //Monitor.Enter(obj, ref _lockWasTaken);
    //        }

    //        catch { }
    //    }

    //    public void Dispose()
    //    {
    //        if (_lockWasTaken)
    //            MyMonitor.Exit(_obj);
    //            //Monitor.Exit(_obj);
    //    }
    //}
}
