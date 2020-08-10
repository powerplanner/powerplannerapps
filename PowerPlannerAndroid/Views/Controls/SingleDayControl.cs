using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using ToolsPortable;
using PowerPlannerAndroid.Adapters;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Themes;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewLists;
using AndroidX.RecyclerView.Widget;
using System.Threading.Tasks;

namespace PowerPlannerAndroid.Views.Controls
{
    public class SingleDayControl : InflatedViewWithBinding
    {
        public event EventHandler<ViewItemTaskOrEvent> ItemClick;
        public event EventHandler<ViewItemHoliday> HolidayItemClick;
        public event EventHandler<ViewItemSchedule> ScheduleItemClick;
        public event EventHandler ScheduleClick;

        private FlatTasksOrEventsAdapter _adapter;
        private DayScheduleSnapshotView _snapshot;
        private DateTime? _date;
        private SemesterItemsViewGroup _viewItemsGroup;
        private RecyclerView _recyclerView;
        private FrameLayout _differentSemesterOverlayContainer;

        public SingleDayControl(ViewGroup root) : base(Resource.Layout.SingleDay, root)
        {
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.RecyclerViewItems);

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            _recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            _adapter = new FlatTasksOrEventsAdapter();
            _adapter.ItemClick += Adapter_ItemClick;

            _adapter.CreateViewHolderForFooter = CreateFooterViewHolder;
            _adapter.Footer = "footer"; // Don't need an object, but need this so footer counts towards items
            _recyclerView.SetAdapter(_adapter);

            // ViewTreeObserver.ScrollChanged ends up holding a strong reference so need to use Touch
            _recyclerView.Touch += _recyclerView_Touch;

            _differentSemesterOverlayContainer = FindViewById<FrameLayout>(Resource.Id.FrameLayoutDifferentSemesterOverlayContainer);
        }

        private GenericRecyclerViewHolder CreateFooterViewHolder(ViewGroup parent, object footer)
        {
            if (_snapshot != null)
            {
                _snapshot.Deinitialize();
                _snapshot.HolidayItemClick -= _snapshot_HolidayItemClick;
                _snapshot.ScheduleItemClick -= _snapshot_ScheduleItemClick;
                _snapshot.ScheduleClick -= _snapshot_ScheduleClick;
            }

            _snapshot = new DayScheduleSnapshotView(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    LinearLayout.LayoutParams.WrapContent)
                {
                    TopMargin = ThemeHelper.AsPx(Context, 10),
                    BottomMargin = ThemeHelper.AsPx(Context, 80)
                }
            };
            _snapshot.HolidayItemClick += _snapshot_HolidayItemClick;
            _snapshot.ScheduleItemClick += _snapshot_ScheduleItemClick;
            _snapshot.ScheduleClick += _snapshot_ScheduleClick;

            InitializeSnapshot();

            return new GenericRecyclerViewHolder(_snapshot);
        }

        private void _recyclerView_Touch(object sender, TouchEventArgs e)
        {
            try
            {
                if (_snapshot != null)
                {
                    e.Handled = _snapshot.CloseExpandedEvents();
                }
            }
            catch { }
        }

        private void _snapshot_HolidayItemClick(object sender, ViewItemHoliday e)
        {
            HolidayItemClick?.Invoke(this, e);
        }

        private void _snapshot_ScheduleClick(object sender, EventArgs e)
        {
            ScheduleClick?.Invoke(this, e);
        }

        private void _snapshot_ScheduleItemClick(object sender, ViewItemSchedule e)
        {
            ScheduleItemClick?.Invoke(this, e);
        }

        private void Adapter_ItemClick(object sender, ViewItemTaskOrEvent e)
        {
            ItemClick?.Invoke(this, e);
        }

        private int? _indexToScrollTo;
        private void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                        if (_indexToScrollTo == null)
                        {
                            _indexToScrollTo = e.NewStartingIndex;

                            _ = Task.Run(delegate
                            {
                                PortableDispatcher.GetCurrentDispatcher().Run(delegate
                                {
                                    try
                                    {
                                        _recyclerView.ScrollToPosition(_indexToScrollTo.Value);
                                    }
                                    catch { }

                                    _indexToScrollTo = null;
                                });
                            });
                        }
                        break;
                }
            }
            catch { }
        }

        private NotifyCollectionChangedEventHandler _currItemsSourceCollectionChangedHandler;
        public void Initialize(DateTime date, TasksOrEventsOnDay tasksOrEvents, SemesterItemsViewGroup viewGroup)
        {
            _viewItemsGroup = viewGroup;
            _date = date;

            // Set the header text
            FindViewById<TextView>(Resource.Id.TextViewHeaderText).Text = GetHeaderText(date);

            if (_currItemsSourceCollectionChangedHandler != null && _adapter.ItemsSource is INotifyCollectionChanged)
            {
                (_adapter.ItemsSource as INotifyCollectionChanged).CollectionChanged -= _currItemsSourceCollectionChangedHandler;
            }

            _adapter.ItemsSource = tasksOrEvents;
            _currItemsSourceCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(ItemsSource_CollectionChanged).Handler;
            tasksOrEvents.CollectionChanged += _currItemsSourceCollectionChangedHandler;

            InitializeDifferentSemesterOverlay();
            InitializeSnapshot();
        }

        public void Deinitialize()
        {
            if (_currItemsSourceCollectionChangedHandler != null && _adapter.ItemsSource is INotifyCollectionChanged)
            {
                (_adapter.ItemsSource as INotifyCollectionChanged).CollectionChanged -= _currItemsSourceCollectionChangedHandler;
            }

            _adapter.ItemsSource = null;
            _snapshot?.Deinitialize();
        }

        private void InitializeSnapshot()
        {
            if (_viewItemsGroup != null && _date != null)
            {
                try
                {
                    _snapshot?.Initialize(_viewItemsGroup, _date.Value);
                }

                // Let ObjectDisposed bubble up so recyclers know to dispose the view
                catch (ObjectDisposedException) { throw; }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private void InitializeDifferentSemesterOverlay()
        {
            _differentSemesterOverlayContainer.RemoveAllViews();

            if (!_viewItemsGroup.Semester.IsDateDuringThisSemester(_date.GetValueOrDefault()))
            {
                _differentSemesterOverlayContainer.AddView(new DifferentSemesterOverlayControl(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(
                        ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.MatchParent)
                });
            }
        }

        private string GetHeaderText(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return PowerPlannerResources.GetRelativeDateToday().ToUpper();

            else if (date.Date == DateTime.Today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow().ToUpper();

            else if (date.Date == DateTime.Today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday().ToUpper();

            return PowerPlannerAppDataLibrary.Helpers.DateHelpers.ToMediumDateString(date).ToUpper();
        }
    }
}