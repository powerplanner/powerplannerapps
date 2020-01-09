using InterfacesUWP;
using InterfacesUWP.Controls;
using InterfacesUWP.Converters;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerUWP.Views.CalendarViews;
using PowerPlannerUWPLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public class DayScheduleSnapshot : Button
    {
        private static readonly double TIME_INDICATOR_SIZE = 60;
        private static readonly double GAP_SIZE = 2;
        private static readonly double HEIGHT_OF_HOUR = TIME_INDICATOR_SIZE + GAP_SIZE;

        public event EventHandler<ViewItemHoliday> OnRequestViewHoliday;

        private class MyScheduleItem : Grid
        {
            public MyScheduleItem(ViewItemSchedule s)
            {
                ViewItemClass c = s.Class as ViewItemClass;

                base.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                base.Background = new SolidColorBrush(ColorTools.GetColor(c.Color));

                var timeSpan = s.EndTime.TimeOfDay - s.StartTime.TimeOfDay;
                var hours = timeSpan.TotalHours;
                var showTimeText = timeSpan.TotalMinutes >= 38;
                
                base.Height = Math.Max(HEIGHT_OF_HOUR * hours, 0);

                base.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                base.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                base.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                TextBlock tbClass = new TextBlock()
                {
                    Text = c.Name,
                    Style = Application.Current.Resources["BaseTextBlockStyle"] as Style,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Windows.UI.Xaml.Thickness(6, 0, 6, 0),
                    TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap,
                    FontSize = 14
                };
                Grid.SetColumnSpan(tbClass, 2);
                base.Children.Add(tbClass);

                // Add the time text (xx:xx to xx:xx) - do NOT show the date text for anything below 38 minutes as that will overlap with the title and get pushed beneath it.
                TextBlock tbTime = null;
                if (showTimeText)
                {
                    var timeFormatter = new DateTimeFormatter("shorttime");
                    tbTime = new TextBlock()
                    {
                        Text = LocalizedResources.Common.GetStringTimeToTime(timeFormatter.Format(s.StartTime), timeFormatter.Format(s.EndTime)),
                        Style = Application.Current.Resources["BaseTextBlockStyle"] as Style,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Windows.UI.Xaml.Thickness(6, -2, 6, 0),
                        TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap,
                        FontSize = 14
                    };
                }

                TextBlock tbRoom = new TextBlock()
                {
                    Text = s.Room,
                    Style = Application.Current.Resources["BaseTextBlockStyle"] as Style,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Windows.UI.Xaml.Thickness(6, -2, 6, 0),
                    TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords,
                    FontSize = 14
                };

                if (hours >= 1.1)
                {
                    if (showTimeText)
                    {
                        Grid.SetRow(tbTime, 1);
                        Grid.SetColumnSpan(tbTime, 2);
                        base.Children.Add(tbTime);
                    }

                    if (!string.IsNullOrWhiteSpace(s.Room))
                    {
                        Grid.SetRow(tbRoom, showTimeText ? 2 : 1);
                        Grid.SetColumnSpan(tbRoom, 2);
                        base.Children.Add(tbRoom);
                    }
                }

                else
                {
                    if (showTimeText)
                    {
                        tbTime.Margin = new Thickness(tbTime.Margin.Left, tbTime.Margin.Top, tbTime.Margin.Right, 8);
                        tbTime.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
                        Grid.SetRow(tbTime, 2);
                        base.Children.Add(tbTime);
                    }

                    tbRoom.Margin = new Thickness(tbRoom.Margin.Left, tbRoom.Margin.Top, tbRoom.Margin.Right, 8);
                    tbRoom.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
                    tbRoom.TextAlignment = TextAlignment.Right;
                    tbRoom.TextWrapping = TextWrapping.NoWrap;
                    tbRoom.TextTrimming = TextTrimming.CharacterEllipsis;
                    Grid.SetRow(tbRoom, showTimeText ? 2 : 1);
                    Grid.SetColumn(tbRoom, 1);
                    base.Children.Add(tbRoom);
                }
            }

            protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
            {
                return base.ArrangeOverride(finalSize);
            }
        }

        private Grid _schedulesGrid = new Grid();
        private Grid _mainGrid = new Grid();
        private ListView _holidaysListView;

        public DayScheduleSnapshot()
        {
            base.Style = Application.Current.Resources["ItemContainerStyle"] as Style;
            base.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;

            _mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            _mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            _holidaysListView = new ListView()
            {
                ItemTemplate = Application.Current.Resources["DataTemplateHolidaySnapshotItem"] as DataTemplate,
                Style = Application.Current.Resources["BlankListViewStyle"] as Style,
                IsItemClickEnabled = true
            };
            _holidaysListView.ItemClick += _holidaysListView_ItemClick;
            _mainGrid.Children.Add(_holidaysListView);

            _schedulesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(TIME_INDICATOR_SIZE) });
            _schedulesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(GAP_SIZE) });
            _schedulesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(_schedulesGrid, 1);
            _mainGrid.Children.Add(_schedulesGrid);

            base.Content = _mainGrid;

            base.Click += DayScheduleSnapshot_Click;
        }

        private void DayScheduleSnapshot_Click(object sender, RoutedEventArgs e)
        {
            HideAnyPopups();
        }

        public bool HideAnyPopups()
        {
            bool hidPopup = false;
            foreach (var vis in GetEventVisuals())
            {
                hidPopup = vis.HideFull() || hidPopup;
            }
            return hidPopup;
        }

        private void _holidaysListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewItemHoliday holiday = e.ClickedItem as ViewItemHoliday;
            OnRequestViewHoliday?.Invoke(this, holiday);
        }

        private EventHandler _arrangedItemsOnItemsChangedHandler;
        private NotifyCollectionChangedEventHandler _currHolidaysChangedHandler;
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

                if (_currHolidays != null && _currHolidaysChangedHandler != null)
                {
                    _currHolidays.CollectionChanged -= _currHolidaysChangedHandler;
                    _currHolidays = null;
                }

                _currHolidays = HolidaysOnDay.Create(ViewModel.Items, Date);
                _currHolidaysChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(_currHolidays_CollectionChanged).Handler;
                _currHolidays.CollectionChanged += _currHolidaysChangedHandler;

                _holidaysListView.ItemsSource = _currHolidays;
                UpdateHolidaysBehavior();

                if (_arrangedItemsOnItemsChangedHandler == null)
                {
                    _arrangedItemsOnItemsChangedHandler = new WeakEventHandler<EventArgs>(_arrangedItems_OnItemsChanged).Handler;
                }
                else if (_arrangedItems != null)
                {
                    _arrangedItems.OnItemsChanged -= _arrangedItemsOnItemsChangedHandler;
                }

                _arrangedItems = DayScheduleItemsArranger.Create(PowerPlannerApp.Current.GetCurrentAccount(), ViewModel, PowerPlannerApp.Current.GetMainScreenViewModel().ScheduleViewItemsGroup, Date, HEIGHT_OF_HOUR, MyCollapsedEventItem.SPACING_WITH_NO_ADDITIONAL, MyCollapsedEventItem.SPACING_WITH_ADDITIONAL, MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM, includeHomeworkAndHolidays: true);
                _arrangedItems.OnItemsChanged += _arrangedItemsOnItemsChangedHandler;

                render();
            }
            catch (Exception ex)
            {
                // There might have been a data error loading the main data, don't want to crash because of that
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdateHolidaysBehavior()
        {
            if (_currHolidays != null && _currHolidays.Count > 0)
            {
                _schedulesGrid.Opacity = 0.5;
            }
            else
            {
                _schedulesGrid.Opacity = 1;
            }
        }

        private void UpdateVisibility()
        {
            if (_schedulesGrid.Children.Count == 0 && (_currHolidays == null || _currHolidays.Count == 0))
            {
                base.Visibility = Visibility.Collapsed;
            }
            else
            {
                base.Visibility = Visibility.Visible;
            }
        }

        private HolidaysOnDay _currHolidays;
        private DayScheduleItemsArranger _arrangedItems;

        private void render()
        {
            _schedulesGrid.Children.Clear();
            _schedulesGrid.RowDefinitions.Clear();

            if (Classes == null || Date == DateTime.MinValue || !ViewModel.Semester.IsDateDuringThisSemester(Date))
            {
                UpdateVisibility();
                return;
            }

            if (!_arrangedItems.IsValid())
            {
                UpdateVisibility();
                return;
            }

            base.Visibility = Visibility.Visible;

            //put in the vertical gap divider
            Rectangle verticalGap = new Rectangle()
            {
                Fill = new SolidColorBrush((Color)Application.Current.Resources["SystemAltHighColor"])
            };
            Grid.SetColumn(verticalGap, 1);
            Grid.SetRowSpan(verticalGap, int.MaxValue);
            _schedulesGrid.Children.Add(verticalGap);

            var hourFormatter = new DateTimeFormatter("{hour.integer}");

            for (TimeSpan time = _arrangedItems.StartTime; time <= _arrangedItems.EndTime; time = time.Add(TimeSpan.FromHours(1)))
            {
                _schedulesGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(TIME_INDICATOR_SIZE) });

                TextBlock hour = new TextBlock()
                {
                    Text = hourFormatter.Format(DateTime.Today.Add(time)),
                    FontSize = 26,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                    FontWeight = FontWeights.Light
                };
                Grid.SetRow(hour, _schedulesGrid.RowDefinitions.Count - 1);
                _schedulesGrid.Children.Add(hour);

                //if not last row, add the divider
                if (time + TimeSpan.FromHours(1) <= _arrangedItems.EndTime)
                {
                    _schedulesGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(GAP_SIZE) });

                    Rectangle gap = new Rectangle()
                    {
                        Fill = new SolidColorBrush((Color)Application.Current.Resources["SystemAltHighColor"])
                    };
                    Grid.SetRow(gap, _schedulesGrid.RowDefinitions.Count - 1);
                    Grid.SetColumnSpan(gap, 3);
                    _schedulesGrid.Children.Add(gap);
                }
            }

            foreach (var s in _arrangedItems.ScheduleItems)
            {
                MyScheduleItem visual = new MyScheduleItem(s.Item);

                AddVisualItem(visual, s);
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in _arrangedItems.EventItems.Reverse())
            {
                FrameworkElement visual;
                if (e.IsCollapsedMode)
                {
                    visual = new MyCollapsedEventItem()
                    {
                        Item = e
                    };
                }
                else
                {
                    visual = new MyFullEventItem()
                    {
                        Item = e
                    };
                }

                AddVisualItem(visual, e);
            }
        }

        private void _arrangedItems_OnItemsChanged(object sender, EventArgs e)
        {
            try
            {
                render();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void AddVisualItem(FrameworkElement visual, DayScheduleItemsArranger.BaseScheduleItem item)
        {
            FrameworkElement root;

            if (item.NumOfColumns > 1)
            {
                Grid grid = new Grid()
                {
                    Margin = new Windows.UI.Xaml.Thickness(0, 0, -6, 0)
                };
                for (int i = 0; i < item.NumOfColumns; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                }
                visual.Margin = new Thickness(0, 0, 6, 0);
                Grid.SetColumn(visual, item.Column);
                grid.Children.Add(visual);
                root = grid;
            }
            else
            {
                root = visual;
            }

            root.Margin = new Thickness(item.LeftOffset, item.TopOffset, 0, 0);
            Grid.SetColumn(root, 2);
            Grid.SetRowSpan(root, int.MaxValue);

            _schedulesGrid.Children.Add(root);
        }

        private IEnumerable<MyBaseEventVisual> GetEventVisuals()
        {
            return _schedulesGrid.Children.Concat(_schedulesGrid.Children.OfType<Grid>().SelectMany(i => i.Children)).OfType<MyBaseEventVisual>();
        }

        private void _currHolidays_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateHolidaysBehavior();
                UpdateVisibility();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void Schedules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                render();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
        
        public DateTime Date { get; private set; }
        
        public MyObservableList<ViewItemClass> Classes
        {
            get { return ViewModel.Classes; }
        }

        public SemesterItemsViewGroup ViewModel { get; private set; }
    }



    public abstract class MyBaseEventVisual : Grid
    {
        private MyAdditionalItemsVisual _additionalItemsVisual;
        private Grid _normalGrid;
        private Border _expandedContainer;
        private const int EXPANDED_CONTAINER_MARGIN = -3;
        private const int EXPANDED_CONTAINER_TOP_BOTTOM_PADDING = 6;

        public const string TELEMETRY_ON_CLICK_EVENT_NAME = "Click_ScheduleEventItem";

        public MyBaseEventVisual()
        {
            _normalGrid = new Grid()
            {
                ColumnDefinitions =
                    {
                        new ColumnDefinition() { Width = new GridLength(MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM) },
                        new ColumnDefinition() { Width = GridLength.Auto }
                    }
            };

            _normalGrid.VerticalAlignment = VerticalAlignment.Top;

            base.VerticalAlignment = VerticalAlignment.Top;
            base.HorizontalAlignment = HorizontalAlignment.Stretch;

            _additionalItemsVisual = new MyAdditionalItemsVisual();
            Grid.SetColumn(_additionalItemsVisual, 1);
            _normalGrid.Children.Add(_additionalItemsVisual);

            base.Children.Add(_normalGrid);

            var borderBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            borderBrush = new SolidColorBrush(borderBrush.Color)
            {
                Opacity = 0.3
            };

            _expandedContainer = new Border()
            {
                Visibility = Visibility.Collapsed,
                Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush,
                CornerRadius = new CornerRadius(12),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(3),
                Padding = new Thickness(0, EXPANDED_CONTAINER_TOP_BOTTOM_PADDING, 0, EXPANDED_CONTAINER_TOP_BOTTOM_PADDING),
                Margin = new Thickness(EXPANDED_CONTAINER_MARGIN)
            };
            base.Children.Add(_expandedContainer);
        }

        public bool IsFullItem
        {
            set
            {
                if (value)
                {
                    _normalGrid.ColumnDefinitions[0] = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
                }
                else
                {
                    _normalGrid.ColumnDefinitions[0] = new ColumnDefinition() { Width = new GridLength(MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM) };
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

                if (_normalGrid.Children.Count == 2)
                {
                    _normalGrid.Children.RemoveAt(0);
                }

                if (value != null)
                {
                    var content = GenerateContent(value);
                    _normalGrid.Children.Insert(0, content);
                    _additionalItemsVisual.AdditionalItems = value.AdditionalItems;
                    _normalGrid.Height = value.Height;
                    _expandedContainer.MinHeight = value.Height;

                    if (value.CanExpand())
                    {
                        base.Tapped += MyBaseEventVisual_Tapped;
                        base.PointerEntered += MyFullEventItem_PointerEntered;
                        base.PointerExited += MyFullEventItem_PointerExited;
                        base.PointerCanceled += MyFullEventItem_PointerExited;
                        base.PointerCaptureLost += MyFullEventItem_PointerExited;
                    }
                    else
                    {
                        base.Tapped += MyBaseEventVisual_TappedForOpen;
                    }
                }
            }
        }

        private void MyBaseEventVisual_TappedForOpen(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item.Item);
            TelemetryExtension.Current?.TrackEvent(TELEMETRY_ON_CLICK_EVENT_NAME);
        }

        private bool _ignoreNextPointerExit;
        private void MyBaseEventVisual_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ShowFull())
            {
                e.Handled = true;
                _ignoreNextPointerExit = true;
            }
        }

        private void MyFullEventItem_PointerExited(object sender, object e)
        {
            if (_ignoreNextPointerExit)
            {
                _ignoreNextPointerExit = false;
                return;
            }
            HideFull();
        }

        private void MyFullEventItem_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
            ShowFull();
        }

        private bool _isFullShown = false;

        private bool ShowFull()
        {
            if (Item == null || !Item.CanExpand())
            {
                return false;
            }

            if (_expandedContainer.Child == null)
            {
                _expandedContainer.Child = GenerateFullContent(Item);
                (_expandedContainer.Child as FrameworkElement).SizeChanged += FullContent_SizeChanged;
            }

            _isFullShown = true;

            var dontWait = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
            {
                if (_isFullShown)
                {
                    _expandedContainer.Visibility = Visibility.Visible;
                }
            });

            return true;
        }

        private void FullContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double diff = (e.NewSize.Height + EXPANDED_CONTAINER_TOP_BOTTOM_PADDING + EXPANDED_CONTAINER_TOP_BOTTOM_PADDING - EXPANDED_CONTAINER_MARGIN) - _expandedContainer.ActualHeight;
            if (diff > 0)
            {
                _expandedContainer.Margin = new Thickness(
                    EXPANDED_CONTAINER_MARGIN,
                    EXPANDED_CONTAINER_MARGIN - diff,
                    EXPANDED_CONTAINER_MARGIN,
                    EXPANDED_CONTAINER_MARGIN);
            }
            else
            {
                _expandedContainer.Margin = new Thickness(EXPANDED_CONTAINER_MARGIN);
            }
        }

        public bool HideFull()
        {
            if (_isFullShown)
            {
                _isFullShown = false;
                _expandedContainer.Visibility = Visibility.Collapsed;
                return true;
            }

            return false;
        }

        protected abstract FrameworkElement GenerateContent(DayScheduleItemsArranger.EventItem item);

        protected FrameworkElement GenerateFullContent(DayScheduleItemsArranger.EventItem item)
        {
            StackPanel sp = new StackPanel();
            sp.Children.Add(new MainCalendarItemView()
            {
                DataContext = item.Item,
                TelemetryOnClickEventName = TELEMETRY_ON_CLICK_EVENT_NAME
            });
            if (item.AdditionalItems != null)
            {
                foreach (var i in item.AdditionalItems)
                {
                    sp.Children.Add(new MainCalendarItemView()
                    {
                        DataContext = i,
                        TelemetryOnClickEventName = TELEMETRY_ON_CLICK_EVENT_NAME
                    });
                }
            }

            return sp;
        }
    }

    public class MyFullEventItem : MyBaseEventVisual
    {
        public MyFullEventItem()
        {
            IsFullItem = true;
        }

        protected override FrameworkElement GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new Grid();

            var rectangle = new Rectangle()
            {
                RadiusX = 12,
                RadiusY = 12,
                Fill = MyCollapsedEventItem.GetBackgroundBrush(item.Item)
            };
            grid.Children.Add(rectangle);

            var tb = new TextBlock()
            {
                Foreground = Brushes.White,
                Margin = new Windows.UI.Xaml.Thickness(6, 6, 0, 0),
                Text = item.Item.Name
            };
            if (item.Item.IsComplete())
            {
                TextBlockCompat.SetStrikethrough(tb, true);
            }
            grid.Children.Add(tb);

            return grid;
        }
    }

    public class MyCollapsedEventItem : MyBaseEventVisual
    {
        public const double WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly double SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly double SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 14;

        protected override FrameworkElement GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new Grid()
            {
                Width = WIDTH_OF_COLLAPSED_ITEM,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var rectangle = new Rectangle()
            {
                RadiusX = 12,
                RadiusY = 12,
                Fill = GetBackgroundBrush(item.Item)
            };
            grid.Children.Add(rectangle);

            var tb = new TextBlock()
            {
                Foreground = Brushes.White,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Windows.UI.Xaml.Thickness(0, 6, 0, 0),
                Text = item.Item.Name.Substring(0, 1)
            };
            grid.Children.Add(tb);

            return grid;
        }

        public static Brush GetBackgroundBrush(BaseViewItemHomeworkExam item)
        {
            if (item.IsComplete())
            {
                return new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
            }

            else
            {
                return new ColorArrayToBrushConverter().Convert(item.GetClassOrNull()?.Color, null, null, null) as Brush;
            }
        }
    }

    public class MyAdditionalItemsVisual : StackPanel
    {
        public MyAdditionalItemsVisual()
        {
            Margin = new Thickness(2, 0, 0, 0);
            Visibility = Visibility.Collapsed;
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
                    base.Visibility = Visibility.Collapsed;
                    return;
                }

                foreach (var additional in value)
                {
                    this.Children.Add(new Ellipse()
                    {
                        Width = 9,
                        Height = 9,
                        Margin = new Thickness(0, 0, 0, 2),
                        Fill = MyCollapsedEventItem.GetBackgroundBrush(additional)
                    });
                }

                base.Visibility = this.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
