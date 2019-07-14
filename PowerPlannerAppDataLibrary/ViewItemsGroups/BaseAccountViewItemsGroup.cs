using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    public class BaseAccountViewItemsGroup : BindableBase
    {
        private static List<WeakReference<BaseAccountViewItemsGroup>> _viewItemsGroups = new List<WeakReference<BaseAccountViewItemsGroup>>();

        public MyAsyncReadWriteLockWithTracking DataChangeLock { get; private set; } = new MyAsyncReadWriteLockWithTracking();

        public PortableDispatcher Dispatcher { get; private set; } = PortableDispatcher.GetCurrentDispatcher();

        static BaseAccountViewItemsGroup()
        {
            AccountDataStore.DataChangedEvent += AccountDataStore_DataChangedEvent;
        }

        private static void AccountDataStore_DataChangedEvent(object sender, DataChangedEvent e)
        {
            lock (_viewItemsGroups)
            {
                for (int i = 0; i < _viewItemsGroups.Count; i++)
                {
                    BaseAccountViewItemsGroup model;

                    if (_viewItemsGroups[i].TryGetTarget(out model))
                    {
                        model.OnInitialDataChangedEvent(e);
                    }

                    else
                    {
                        _viewItemsGroups.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public Guid LocalAccountId { get; private set; }
        protected bool trackChanges { get; private set; }

        /// <summary>
        /// Must be initialized on UI thread, since it grabs the Dispatcher
        /// </summary>
        public BaseAccountViewItemsGroup(Guid localAccountId, bool trackChanges = true)
        {
            LocalAccountId = localAccountId;
            this.trackChanges = trackChanges;

            // If we use the DataChangedEvent, this object will never be garbage collected since it's always referenced by the static event!
            // Thus we can use a list of weak references

            if (trackChanges)
            {
                lock (_viewItemsGroups)
                {
                    _viewItemsGroups.Add(new WeakReference<BaseAccountViewItemsGroup>(this));
                }
            }

            Debug.WriteLine("ViewItemsGroup created: " + this.GetType());
        }

        ~BaseAccountViewItemsGroup()
        {
            Debug.WriteLine("ViewItemsGroup disposed: " + this.GetType());
        }

        private void OnInitialDataChangedEvent(DataChangedEvent e)
        {
            try
            {
                if (e.LocalAccountId == LocalAccountId)
                {
                    Dispatcher.Run(async delegate
                    {
                        try
                        {
                            // There's a good reason why we lock here. You initially might think "Well, it's on the UI thread, no reason
                            // to lock when something's on the UI thread, right?"
                            // However, some things like Live Tiles will load the view items group on a background thread, and will
                            // enumerate over the items within the view items group from their background thread.
                            // Therefore, a new change could come in on the UI thread while the background thread is enumerating over
                            // the items in the view group. That would therefore cause an exception in the enumeration.
                            // So far we haven't seen any cases of this lock causing deadlocks, and it's 100% isolated from the main data write
                            // lock, so there shouldn't be any concerns about this causing delays
                            using (await DataChangeLock.LockForWriteAsync(callerName: this.GetType() + ".OnInitialDataChanged"))
                            {
                                OnDataChangedEvent(e);
                            }
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Triggered when there's a data changed event for this account. Already dispatched to UI
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataChangedEvent(DataChangedEvent e)
        {
            // nothing
        }

        private AccountDataStore _dataStore;
        /// <summary>
        /// This can NOT be called while in a data lock, since initializing data store uses a lock
        /// </summary>
        /// <returns></returns>
        protected async Task<AccountDataStore> GetDataStore()
        {
            if (_dataStore == null)
                _dataStore = await AccountDataStore.Get(LocalAccountId);

            return _dataStore;
        }
    }
}
