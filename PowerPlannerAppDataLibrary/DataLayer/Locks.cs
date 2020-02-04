using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsPortable.Locks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public static class Locks
    {
        /// <summary>
        /// Lock for the data items on an account. Theoretically there should be one per each account, but it's easier to just have an overall one
        /// </summary>
        private static readonly MyAsyncReadWriteLockWithTracking _dataLock = new MyAsyncReadWriteLockWithTracking();

        public static readonly MyAsyncLock _accountsLock = new MyAsyncLock();

        private const int MILLISECOND_TIMEOUT = 20000;


        public static Task<IDisposable> LockDataForReadAsync([CallerMemberName]string callerName = null, string customMessage = null, [CallerFilePath]string callerFilePath = null)
        {
            return _dataLock.LockForReadAsync(callerName, customMessage, callerFilePath);
        }

        public static Task<IDisposable> LockDataForReadAsync(Func<string> customMessage, [CallerMemberName]string callerName = null, [CallerFilePath]string callerFilePath = null)
        {
            return _dataLock.LockForReadAsync(customMessage, callerName, callerFilePath);
        }

        public static Task<IDisposable> LockDataForWriteAsync([CallerMemberName]string callerName = null, string customMessage = null, [CallerFilePath]string callerFilePath = null)
        {
            return _dataLock.LockForWriteAsync(callerName, customMessage, callerFilePath);
        }

        public static Task<IDisposable> LockDataForWriteAsync(Func<string> customMessage, [CallerMemberName]string callerName = null, [CallerFilePath]string callerFilePath = null)
        {
            return _dataLock.LockForWriteAsync(customMessage, callerName, callerFilePath);
        }

        public static async Task<IDisposable> LockAccounts()
        {
            IDisposable answer = await _accountsLock.LockAsync(MILLISECOND_TIMEOUT);
            return answer;
        }

        
    }

    public class MyAsyncReadWriteLockWithTracking
    {
        private const int MILLISECOND_TIMEOUT = 20000;
        private MyAsyncReadWriteLock _lock = new MyAsyncReadWriteLock();
        private List<LockingTracker> ActiveReadLocks = new List<LockingTracker>();
        private List<LockingTracker> ActiveWriteLocks = new List<LockingTracker>();

        public Task<IDisposable> LockForReadAsync([CallerMemberName]string callerName = null, string customMessage = null, [CallerFilePath]string callerFilePath = null)
        {
            return LockForReadAsync(delegate { return customMessage; }, callerName, callerFilePath);
        }

        public async Task<IDisposable> LockForReadAsync(Func<string> customMessage, [CallerMemberName]string callerName = null, [CallerFilePath]string callerFilePath = null)
        {
            try
            {
                // iOS doesn't seem to allow simultaneous SQL read operations at a time, so instead will redirect everything to write locks.
                // Otherwise that's where I'm getting the crashing from when reminders are trying to be set while other data is also being loaded.
                if (SyncLayer.SyncExtensions.GetPlatform() == "iOS")
                {
                    return await LockForWriteAsync(customMessage, callerName, callerFilePath);
                }

                IDisposable answer = new LockingTracker(
                    this,
                    await _lock.LockReadAsync(MILLISECOND_TIMEOUT),
                    false,
                    customMessage,
                    callerName,
                    callerFilePath);
                return answer;
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException("Timeout for read lock reached. " + GetLocksThatAreHeld() + "Caller name: " + callerName + ". File: " + callerFilePath, ex);
            }
        }

        private string GetLocksThatAreHeld()
        {
            try
            {
                string answer = "";

                lock (this)
                {
                    if (ActiveWriteLocks.Count > 0)
                    {
                        answer = "WriteLocks held: " + string.Join(", ", ActiveWriteLocks.Select(i => i.CallerName)) + ". ";
                    }

                    if (ActiveReadLocks.Count > 0)
                    {
                        answer += "ReadLocks held: " + string.Join(", ", ActiveReadLocks.Select(i => i.CallerName)) + ". ";
                    }
                }

                if (answer.Length == 0)
                {
                    return "No locks were held.";
                }

                return answer;
            }
            catch { return "Exception getting locks that were held"; }
        }

        public Task<IDisposable> LockForWriteAsync([CallerMemberName]string callerName = null, string customMessage = null, [CallerFilePath]string callerFilePath = null)
        {
            return LockForWriteAsync(delegate { return customMessage; }, callerName, callerFilePath);
        }

        public async Task<IDisposable> LockForWriteAsync(Func<string> customMessage, [CallerMemberName]string callerName = null, [CallerFilePath]string callerFilePath = null)
        {
            try
            {
                IDisposable answer = new LockingTracker(
                    this,
                    await _lock.LockWriteAsync(MILLISECOND_TIMEOUT),
                    true,
                    customMessage,
                    callerName,
                    callerFilePath);
                return answer;
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException("Timeout for write lock reached. " + GetLocksThatAreHeld() + "Caller name: " + callerName + ". File: " + callerFilePath, ex);
            }
        }

        private class LockingTracker : IDisposable
        {
            private MyAsyncReadWriteLockWithTracking _parent;
            private IDisposable _lock;
            private DateTime _start;
            private Func<string> _customMessage;
            private string _typeOfLock;
            public string CallerName { get; private set; }
            private string _callerFilePath;
            private bool _isWriteLock;

            public LockingTracker(MyAsyncReadWriteLockWithTracking parent, IDisposable actualLock, bool isWriteLock, Func<string> message, string callerName, string callerFilePath)
            {
                _parent = parent;
                _lock = actualLock;
                _start = DateTime.UtcNow;
                _customMessage = message;
                _isWriteLock = isWriteLock;
                _typeOfLock = isWriteLock ? "Write" : "Read";
                CallerName = callerName;
                _callerFilePath = callerFilePath;

                try
                {
                    lock (_parent)
                    {
                        if (_isWriteLock)
                        {
                            _parent.ActiveWriteLocks.Add(this);
                        }
                        else
                        {
                            _parent.ActiveReadLocks.Add(this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            public void Dispose()
            {
                TimeSpan duration = (DateTime.UtcNow - _start);

                _lock.Dispose();

                try
                {
                    lock (_parent)
                    {
                        if (_isWriteLock)
                        {
                            _parent.ActiveWriteLocks.Remove(this);
                        }
                        else
                        {
                            _parent.ActiveReadLocks.Remove(this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                Debug.WriteLine($"{_typeOfLock}Lock took {duration.TotalSeconds.ToString("0.0")} seconds for {CallerName}");
                if (duration.TotalSeconds > 8 && TelemetryExtension.Current != null)
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>()
                    {
                        { "CallerName", CallerName },
                        { "CallerFilePath", _callerFilePath },
                        { "Duration", duration.TotalSeconds.ToString("0.0") }
                    };

                    try
                    {
                        var customMsg = _customMessage?.Invoke();
                        if (customMsg != null)
                        {
                            properties["CustomMessage"] = customMsg;
                        }
                    }
                    catch { }

                    TelemetryExtension.Current.TrackEvent($"{_typeOfLock}LockTookTooLong", properties);
                }
            }
        }
    }
}
