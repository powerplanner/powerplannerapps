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
using Android.Graphics.Drawables;
using Android.Graphics;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.App;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using System.ComponentModel;
using BareMvvm.Core.App;

namespace PowerPlannerAndroid.Views
{
    public class DayScheduleSnapshotView : LinearLayout
    {
        public event EventHandler<ViewItemHoliday> HolidayItemClick;
        public event EventHandler<ViewItemSchedule> ScheduleItemClick;
        public event EventHandler ScheduleClick;

        private const double TIME_INDICATOR_SIZE = 60;
        private const double GAP_SIZE = 2;

        private static int TimeIndicatorSize;
        private static int GapSize;
        private static int HeightOfHour;

        public DateTime Date { get; private set; }

        public MyObservableList<ViewItemClass> Classes
        {
            get { return ViewModel.Classes; }
        }

        public SemesterItemsViewGroup ViewModel { get; private set; }

        public DayScheduleSnapshotView(Context context) : base(context)
        {
            this.Background = new ColorDrawable(Color.Argb(255, 240, 240, 240));
            this.Orientation = Orientation.Vertical;

            TimeIndicatorSize = ThemeHelper.AsPx(context, TIME_INDICATOR_SIZE);
            GapSize = ThemeHelper.AsPx(context, GAP_SIZE);
            HeightOfHour = TimeIndicatorSize + GapSize;

            base.Click += DayScheduleSnapshotView_Click;
        }

        private void DayScheduleSnapshotView_Click(object sender, EventArgs e)
        {
            if (CloseExpandedEvents())
            {
                return;
            }

            ScheduleClick?.Invoke(this, new EventArgs());
        }

        public bool CloseExpandedEvents()
        {
            bool didClose = false;
            foreach (var vis in GetEventVisuals())
            {
                didClose = vis.HideFull() || didClose;
            }

            return didClose;
        }

        private IEnumerable<MyBaseEventVisual> GetEventVisuals()
        {
            if (_schedulesContent == null)
            {
                return new MyBaseEventVisual[0];
            }

            return _schedulesContent.GetAllChildren().Concat(_schedulesContent.GetAllChildren().OfType<ViewGroup>().SelectMany(i => i.GetAllChildren())).OfType<MyBaseEventVisual>();
        }

        private DayScheduleItemsArranger _arrangedItems;
        private EventHandler _arrangedItemsOnItemsChangedHandler;
        private int _request = 0;

        /// <summary>
        /// You CAN call this multiple times, it'll successfully clear previous
        /// </summary>
        /// <param name="classes"></param>
        /// <param name="date"></param>
        public async void Initialize(SemesterItemsViewGroup viewModel, DateTime date)
        {
            ViewModel = viewModel;
            Date = date;

            try
            {
                _request++;
                var currRequest = _request;
                await viewModel.LoadingTask;
                if (currRequest != _request)
                {
                    // Another initialize happened while loading, so stop here on this old request
                    // (No concern about int overflow since it wraps by default)
                    return;
                }

                if (_arrangedItemsOnItemsChangedHandler == null)
                {
                    _arrangedItemsOnItemsChangedHandler = new WeakEventHandler<EventArgs>(_arrangedItems_OnItemsChanged).Handler;
                }
                else if (_arrangedItems != null)
                {
                    _arrangedItems.OnItemsChanged -= _arrangedItemsOnItemsChangedHandler;
                }

                _arrangedItems = DayScheduleItemsArranger.Create(PowerPlannerApp.Current.GetCurrentAccount(), ViewModel, PowerPlannerApp.Current.GetMainScreenViewModel().ScheduleViewItemsGroup, Date, TIME_INDICATOR_SIZE + GAP_SIZE, MyCollapsedEventItem.SPACING_WITH_NO_ADDITIONAL, MyCollapsedEventItem.SPACING_WITH_ADDITIONAL, MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM, includeHomeworkAndHolidays: true);
                _arrangedItems.OnItemsChanged += _arrangedItemsOnItemsChangedHandler;

                render();

                if (_holidaysOnDay != null && _holidaysChangedHandler != null)
                {
                    _holidaysOnDay.CollectionChanged -= _holidaysChangedHandler;
                }
                if (_holidaysChangedHandler == null)
                {
                    _holidaysChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(HolidaysOnDay_CollectionChanged).Handler;
                }
                _holidaysOnDay = HolidaysOnDay.Create(viewModel.Items, date);
                _holidaysOnDay.CollectionChanged += _holidaysChangedHandler;
                _holidaysItemsWrapper.ItemsSource = _holidaysOnDay;
                UpdateHolidayOpacity();
            }

            // We want to allow ObjectDisposed to bubble up, so recyclers can know to dispose the view
            catch (ObjectDisposedException) { throw; }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void Deinitialize()
        {
            // Make sure to cancel the async initialize
            _request++;

            if (_arrangedItems != null && _arrangedItemsOnItemsChangedHandler != null)
            {
                _arrangedItems.OnItemsChanged -= _arrangedItemsOnItemsChangedHandler;
            }
            if (_holidaysOnDay != null && _holidaysChangedHandler != null)
            {
                _holidaysOnDay.CollectionChanged -= _holidaysChangedHandler;
            }

            _arrangedItems = null;
            _holidaysOnDay = null;

            if (_holidaysItemsWrapper != null)
            {
                _holidaysItemsWrapper.ItemsSource = null;
            }
        }

        private bool _objectDisposed;
        private void _arrangedItems_OnItemsChanged(object sender, EventArgs e)
        {
            try
            { 
                render();
            }
            catch (ObjectDisposedException ex)
            {
                // I confirmed that this object DOES dispose, it just takes a long time for GC to kick in.
                // And apparently the Java object will dispose before it. But if I call GC.Collect, it disposes immediately.
                // So instead, I'll just unwire the events and then at some point in time GC will collect.
                if (!_objectDisposed)
                {
#if DEBUG
                    WeakEventHandler.InvokeObjectDisposedAction();
#endif
                    _objectDisposed = true;
                    Deinitialize();
                }
                else
                {
                    TelemetryExtension.Current?.TrackException(new Exception("DayScheduleSnapshotView ObjectDisposedException hit twice or more", ex));
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void HolidaysOnDay_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateHolidayOpacity();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdateHolidayOpacity()
        {
            if (_holidaysOnDay.Count > 0)
            {
                _schedulesContent.Alpha = 0.5f;
            }
            else
            {
                _schedulesContent.Alpha = 1f;
            }
        }

        private NotifyCollectionChangedEventHandler _holidaysChangedHandler;
        private HolidaysOnDay _holidaysOnDay;
        private RelativeLayout _schedulesContent;
        private LinearLayout _holidaysLayout;
        private ItemsControlWrapper _holidaysItemsWrapper;

        private void render()
        {
            if (_holidaysLayout == null)
            {
                _holidaysLayout = new LinearLayout(Context)
                {
                    Orientation = Orientation.Vertical
                };

                _holidaysItemsWrapper = new ItemsControlWrapper(_holidaysLayout)
                {
                    ItemTemplate = new CustomDataTemplate<ViewItemHoliday>(CreateHolidayView)
                };

                base.AddView(_holidaysLayout);
            }

            if (_schedulesContent == null)
            {
                _schedulesContent = new RelativeLayout(Context);
                base.AddView(_schedulesContent);
            }
            else
            {
                _schedulesContent.RemoveAllViews();
            }

            if (Classes == null || Date == DateTime.MinValue || !ViewModel.Semester.IsDateDuringThisSemester(Date))
            {
                base.Visibility = ViewStates.Gone;
                return;
            }

            if (!_arrangedItems.IsValid())
            {
                base.Visibility = ViewStates.Gone;
                return;
            }

            base.Visibility = ViewStates.Visible;

            //put in the vertical gap divider
            View verticalGap = new View(Context)
            {
                Background = new ColorDrawable(Color.White),
                LayoutParameters = new RelativeLayout.LayoutParams(
                    GapSize,
                    RelativeLayout.LayoutParams.MatchParent)
                {
                    LeftMargin = TimeIndicatorSize + GapSize
                }
            };
            _schedulesContent.AddView(verticalGap);

            int row = 0;
            for (TimeSpan time = _arrangedItems.StartTime; time <= _arrangedItems.EndTime; time = time.Add(TimeSpan.FromHours(1)), row++)
            {
                int startHeight = row * HeightOfHour;

                _schedulesContent.AddView(new TextView(Context)
                {
                    Text = new DateTime().Add(time).ToString("h ").TrimEnd(),
                    TextSize = 26,
                    LayoutParameters = new RelativeLayout.LayoutParams(
                        TimeIndicatorSize,
                        TimeIndicatorSize)
                    {
                        TopMargin = startHeight
                    },
                    Gravity = GravityFlags.Center
                });

                //if not last row, add the divider
                if (time + TimeSpan.FromHours(1) <= _arrangedItems.EndTime)
                {
                    _schedulesContent.AddView(new View(Context)
                    {
                        Background = new ColorDrawable(Color.White),
                        LayoutParameters = new RelativeLayout.LayoutParams(
                            RelativeLayout.LayoutParams.MatchParent,
                            GapSize)
                        {
                            TopMargin = startHeight + TimeIndicatorSize
                        }
                    });
                }
            }

            // Render the schedules
            foreach (var s in _arrangedItems.ScheduleItems)
            {
                MyScheduleItem visual = new MyScheduleItem(Context, s.Item);
                visual.Click += TimeItem_Click;

                AddVisualItem(visual, s);
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in _arrangedItems.EventItems.Reverse())
            {
                View visual;
                if (e.IsCollapsedMode)
                {
                    visual = new MyCollapsedEventItem(Context)
                    {
                        Item = e
                    };
                }
                else
                {
                    visual = new MyFullEventItem(Context)
                    {
                        Item = e
                    };
                }

                AddVisualItem(visual, e);
            }
        }

        private void AddVisualItem(View visual, DayScheduleItemsArranger.BaseScheduleItem item)
        {
            View root;

            if (item.NumOfColumns > 1)
            {
                var grid = new LinearLayout(Context)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent),
                    Orientation = Orientation.Horizontal
                };

                for (int i = 0; i < item.NumOfColumns; i++)
                {
                    View colView;

                    if (i == item.Column)
                    {
                        colView = visual;
                    }
                    else
                    {
                        colView = new View(Context);
                    }

                    colView.LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent)
                    {
                        Weight = 1
                    };
                    grid.AddView(colView);
                }

                root = grid;
            }
            else
            {
                root = visual;
            }

            root.LayoutParameters = new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
            {
                LeftMargin = ThemeHelper.AsPx(Context, item.LeftOffset) + TimeIndicatorSize + GapSize,
                TopMargin = ThemeHelper.AsPx(Context, item.TopOffset)
            };

            if (item is DayScheduleItemsArranger.ScheduleItem)
            {
                root.LayoutParameters.Height = ThemeHelper.AsPx(Context, item.Height);
            }

            _schedulesContent.AddView(root);
        }

        private View CreateHolidayView(ViewGroup parent, ViewItemHoliday holiday)
        {
            var textView = new TextView(Context)
            {
                Text = holiday.Name,
                LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent)
                {
                    BottomMargin = ThemeHelper.AsPx(Context, 1)
                }
            };
            var px = ThemeHelper.AsPx(Context, 18);
            textView.SetPadding(px, px, 0, px);
            textView.SetBackgroundColor(HolidayColor);
            textView.SetTextColor(Color.White);
            textView.Click += delegate
            {
                HolidayItemClick?.Invoke(textView, holiday);
            };
            return textView;
        }

        public static Color HolidayColor = new Color(228, 0, 137);

        private void TimeItem_Click(object sender, EventArgs e)
        {
            ScheduleItemClick?.Invoke(this, (sender as MyScheduleItem).Schedule);
        }

        private class MyScheduleItem : LinearLayout
        {
            public ViewItemSchedule Schedule { get; private set; }

            public MyScheduleItem(Context context, ViewItemSchedule s) : base(context)
            {
                Schedule = s;

                ViewItemClass c = s.Class as ViewItemClass;

                base.Orientation = Orientation.Vertical;
                base.Background = new ColorDrawable(ColorTools.GetColor(c.Color));

                double hours = (s.EndTime.TimeOfDay - s.StartTime.TimeOfDay).TotalHours;

                base.LayoutParameters = new RelativeLayout.LayoutParams(
                    RelativeLayout.LayoutParams.MatchParent,
                    (int)Math.Max(HeightOfHour * hours, 0));

                var marginSides = ThemeHelper.AsPx(context, 6);
                var marginBetween = ThemeHelper.AsPx(context, -2);

                var tvClass = new TextView(context)
                {
                    Text = c.Name,
                    Typeface = Typeface.DefaultBold
                };
                tvClass.SetTextColor(Color.White);
                tvClass.SetSingleLine(true);
                tvClass.SetPadding(marginSides, 0, marginSides, 0);

                var tvTime = new TextView(context)
                {
                    Text = PowerPlannerResources.GetStringTimeToTime(DateHelper.ToShortTimeString(s.StartTime), DateHelper.ToShortTimeString(s.EndTime)),
                    Typeface = Typeface.DefaultBold
                };
                tvTime.SetTextColor(Color.White);
                tvTime.SetSingleLine(true);
                tvTime.SetPadding(marginSides, marginBetween, marginSides, 0);

                var tvRoom = new TextView(context)
                {
                    Text = s.Room,
                    Typeface = Typeface.DefaultBold
                };
                tvRoom.SetTextColor(Color.White);
                tvRoom.SetPadding(marginSides, marginBetween, marginSides, 0);

                if (hours >= 1.1)
                {
                    base.AddView(tvClass);
                    base.AddView(tvTime);
                    base.AddView(tvRoom);
                }

                else
                {
                    LinearLayout firstGroup = new LinearLayout(context);

                    tvClass.LayoutParameters = new LinearLayout.LayoutParams(
                        0,
                        LinearLayout.LayoutParams.WrapContent)
                    {
                        Weight = 1
                    };
                    firstGroup.AddView(tvClass);

                    tvTime.SetPadding(marginSides, 0, marginSides, 0);
                    tvTime.LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.WrapContent,
                        LinearLayout.LayoutParams.WrapContent);
                    firstGroup.AddView(tvTime);

                    base.AddView(firstGroup);

                    base.AddView(tvRoom);
                }
            }
        }
    }

    public abstract class MyBaseEventVisual : FrameLayout
    {
        private MyAdditionalItemsVisual _additionalItemsVisual;
        private LinearLayout _normalGrid;
        private FrameLayout _expandedContainer;

        public const string TELEMETRY_ON_CLICK_EVENT_NAME = "Click_ScheduleEventItem";

        public MyBaseEventVisual(Context context) : base(context)
        {
            _normalGrid = new LinearLayout(context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            _normalGrid.SetVerticalGravity(GravityFlags.Top);

            base.SetForegroundGravity(GravityFlags.Top | GravityFlags.FillHorizontal);

            _additionalItemsVisual = new MyAdditionalItemsVisual(context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent)
            };
            _normalGrid.AddView(_additionalItemsVisual);

            base.AddView(_normalGrid);

            var expandedMargin = ThemeHelper.AsPx(context, -3);

            _expandedContainer = new FrameLayout(context)
            {
                Visibility = ViewStates.Gone,
                Background = ContextCompat.GetDrawable(context, Resource.Drawable.expanded_schedule_items_background),
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
                {
                    LeftMargin = expandedMargin,
                    TopMargin = expandedMargin,
                    RightMargin = expandedMargin,
                    BottomMargin = expandedMargin
                }
            };
            _expandedContainer.SetPadding(0, ThemeHelper.AsPx(context, 6), 0, ThemeHelper.AsPx(context, 6));
            
            base.AddView(_expandedContainer);
        }

        public bool IsFullItem
        {
            set
            {
                if (_normalGrid.ChildCount >= 2)
                {
                    _normalGrid.GetChildAt(0).LayoutParameters.Width = value ? 0 : ThemeHelper.AsPx(Context, MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM);
                    (_normalGrid.GetChildAt(0).LayoutParameters as LinearLayout.LayoutParams).Weight = value ? 1 : 0;
                }
            }
        }

        private DayScheduleItemsArranger.EventItem _item;
        public DayScheduleItemsArranger.EventItem Item
        {
            get { return _item; }
            set
            {
                _item = value;

                _additionalItemsVisual.AdditionalItems = null;

                if (_normalGrid.ChildCount == 2)
                {
                    _normalGrid.RemoveViewAt(0);
                }

                if (value != null)
                {
                    var height = ThemeHelper.AsPx(Context, value.Height);
                    var content = GenerateContent(value);
                    _normalGrid.AddView(content, 0);
                    _additionalItemsVisual.AdditionalItems = value.AdditionalItems;
                    _normalGrid.LayoutParameters.Height = height;
                    _expandedContainer.SetMinimumHeight(height);

                    if (value.CanExpand())
                    {
                        content.Click += MyBaseEventVisual_Click;
                    }
                    else
                    {
                        base.Click += MyBaseEventVisual_TappedForOpen;
                    }
                }
            }
        }

        private void MyBaseEventVisual_Click(object sender, EventArgs e)
        {
            ShowFull();
        }

        private void MyBaseEventVisual_TappedForOpen(object sender, EventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item.Item);
            TelemetryExtension.Current?.TrackEvent(TELEMETRY_ON_CLICK_EVENT_NAME);
        }

        private bool _isFullShown = false;
        private EventHandler<CancelEventArgs> _backPressedEventHandler;
        private BareMvvm.Core.Windows.PortableAppWindow _window;

        private bool ShowFull()
        {
            if (Item == null || !Item.CanExpand())
            {
                return false;
            }

            if (_backPressedEventHandler == null)
            {
                try
                {
                    // Wire the back press event
                    _backPressedEventHandler = new WeakEventHandler<CancelEventArgs>(MyBaseEventVisual_BackPressed).Handler;
                    _window = PortableApp.Current.GetCurrentWindow();
                    _window.BackPressed += _backPressedEventHandler;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            if (_expandedContainer.ChildCount == 0)
            {
                _expandedContainer.AddView(GenerateFullContent(Item));
            }

            _isFullShown = true;

            //var dontWait = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
            //{
            //    if (_isFullShown)
            //    {
                    _expandedContainer.Visibility = ViewStates.Visible;
            //    }
            //});

            return true;
        }

        protected override void OnDetachedFromWindow()
        {
            if (_backPressedEventHandler != null && _window != null)
            {
                try
                {
                    _window.BackPressed -= _backPressedEventHandler;
                    _window = null;
                    _backPressedEventHandler = null;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            base.OnDetachedFromWindow();
        }

        private void MyBaseEventVisual_BackPressed(object sender, CancelEventArgs e)
        {
            try
            {
                if (HideFull())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Returns true if expanded was open and it actually closed it
        /// </summary>
        /// <returns></returns>
        public bool HideFull()
        {
            if (_isFullShown)
            {
                _isFullShown = false;
                _expandedContainer.Visibility = ViewStates.Gone;
                return true;
            }

            return false;
        }

        protected abstract View GenerateContent(DayScheduleItemsArranger.EventItem item);

        protected View GenerateFullContent(DayScheduleItemsArranger.EventItem item)
        {
            LinearLayout sp = new LinearLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent),
                Orientation = Orientation.Vertical
            };
            sp.AddView(new MainCalendarItemView(Context)
            {
                Item = item.Item,

                // After opened, we hide this popup, otherwise when the user presses back, it'll close the popup rather than the homework
                // Ideally we would implement the back handling as part of the view model like we did for UWP, but for simplicity we're going
                // to leave it like this for now
                AfterOpenedHomeworkAction = delegate { HideFull(); }
            });
            if (item.AdditionalItems != null)
            {
                foreach (var i in item.AdditionalItems)
                {
                    sp.AddView(new MainCalendarItemView(Context)
                    {
                        Item = i,
                        AfterOpenedHomeworkAction = delegate { HideFull(); }
                    });
                }
            }

            return sp;
        }
    }

    public class MyFullEventItem : MyBaseEventVisual
    {
        public MyFullEventItem(Context context) : base(context)
        {
            IsFullItem = true;
        }

        protected override View GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new FrameLayout(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(0, LayoutParams.MatchParent)
                {
                    Weight = 1
                },
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.schedule_item_rounded_rectangle)
            };

            ViewCompat.SetBackgroundTintList(grid, GetBackgroundColorStateList(item.Item));

            var tb = new TextView(Context)
            {
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                {
                    LeftMargin = ThemeHelper.AsPx(Context, 6),
                    TopMargin = ThemeHelper.AsPx(Context, 6)
                },
                Text = item.Item.Name
            };
            tb.SetTextColor(Color.White);
            if (item.Item.IsComplete())
            {
                tb.SetStrikethrough(true);
            }
            grid.AddView(tb);

            return grid;
        }

        public static Android.Content.Res.ColorStateList GetBackgroundColorStateList(BaseViewItemHomeworkExam item)
        {
            return new Android.Content.Res.ColorStateList(new int[][]
            {
                new int[] { }
            },
            new int[]
            {
                item.IsComplete() ? new Color(180, 180, 180).ToArgb() : ColorTools.GetColor(item.GetClassOrNull().Color).ToArgb()
            });
        }
    }

    public class MyCollapsedEventItem : MyBaseEventVisual
    {
        public const double WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly double SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly double SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 14;

        public MyCollapsedEventItem(Context context) : base(context) { }

        protected override View GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new FrameLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ThemeHelper.AsPx(Context, WIDTH_OF_COLLAPSED_ITEM), LayoutParams.MatchParent),
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.schedule_item_rounded_rectangle)
            };

            ViewCompat.SetBackgroundTintList(grid, MyFullEventItem.GetBackgroundColorStateList(item.Item));

            var tb = new TextView(Context)
            {
                //FontSize = 18,
                Gravity = GravityFlags.CenterHorizontal | GravityFlags.Top,
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                {
                    TopMargin = ThemeHelper.AsPx(Context, 6)
                },
                Text = item.Item.Name.Substring(0, 1)
            };
            tb.SetTextColor(Color.White);
            grid.AddView(tb);

            return grid;
        }
    }

    public class MyAdditionalItemsVisual : LinearLayout
    {
        public MyAdditionalItemsVisual(Context context) : base(context)
        {
            base.SetPaddingRelative(ThemeHelper.AsPx(context, 2), 0, 0, 0);
            Visibility = ViewStates.Gone;
        }

        private IEnumerable<BaseViewItemHomeworkExam> _additionalItems;
        public IEnumerable<BaseViewItemHomeworkExam> AdditionalItems
        {
            get { return _additionalItems; }
            set
            {
                _additionalItems = value;

                if (value == null)
                {
                    base.Visibility = ViewStates.Gone;
                    return;
                }

                foreach (var additional in value)
                {
                    this.AddView(CreateCircle(this, additional));
                }

                base.Visibility = this.ChildCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private View CreateCircle(ViewGroup root, BaseViewItemHomeworkExamGrade item)
        {
            View view = new View(root.Context)
            {
                Background = ContextCompat.GetDrawable(root.Context, Resource.Drawable.circle),
                LayoutParameters = new LinearLayout.LayoutParams(
                    ThemeHelper.AsPx(Context, 9),
                    ThemeHelper.AsPx(Context, 9))
                {
                    BottomMargin = ThemeHelper.AsPx(Context, 2)
                }
            };

            if (item is BaseViewItemHomeworkExam)
            {
                ViewCompat.SetBackgroundTintList(view, MyFullEventItem.GetBackgroundColorStateList(item as BaseViewItemHomeworkExam));
            }

            return view;
        }
    }
}