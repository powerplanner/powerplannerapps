using InterfacesUWP.CalendarFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfacesUWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Text;
using ToolsUniversal;
using ToolsPortable;
using Windows.UI;

using Windows.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.PPEventArgs;
using Windows.UI.Xaml.Shapes;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System.ComponentModel;
using PowerPlannerUWP.Controls;
using PowerPlannerUWP.Helpers;

namespace PowerPlannerUWP.Views.CalendarViews
{
    public class MainCalendarSquare : TCalendarSquare
    {
        public event EventHandler<ChangeItemDateEventArgs> OnRequestChangeItemDate;

        private HolidaysOnDay _holidays;
        private CalendarViewModel _calendarViewModel;
        private MyObservableList<BaseViewItemMegaItem> _allItems;

        public MainCalendarSquare(MainCalendarGrid calendarGrid, DateTime date, MyObservableList<BaseViewItemMegaItem> allItems) : base(calendarGrid, date)
        {
            // Render is called before this

            _calendarViewModel = calendarGrid.ViewModel;
            _allItems = allItems;
            AllowDrop = true;
            DragOver += MainCalendarSquare_DragOver;
            Drop += MainCalendarSquare_Drop;
            _calendarViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(_calendarViewModel_PropertyChanged).Handler;
            UpdateItemsSource();
            _holidays = HolidaysOnDay.Create(allItems, date);
            _holidays.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(delegate { UpdateIsHoliday(); }).Handler;
            UpdateIsHoliday();
        }

        private void _calendarViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_calendarViewModel.ShowPastCompleteItemsOnFullCalendar):
                    UpdateItemsSource();
                    break;
            }
        }

        private void UpdateItemsSource()
        {
            if (_calendarViewModel.ShowPastCompleteItemsOnFullCalendar)
            {
                _itemsControl.ItemsSource = TasksOrEventsOnDay.Get(_calendarViewModel.MainScreenViewModel.CurrentAccount, _allItems, Date);
            }
            else
            {
                _itemsControl.ItemsSource = TasksOrEventsOnDay.Get(_calendarViewModel.MainScreenViewModel.CurrentAccount, _allItems, Date, today: _calendarViewModel.Today, activeOnly: true);
            }
        }

        private void UpdateIsHoliday()
        {
            if (_holidays.Count > 0)
            {
                _holidayOverlay.Visibility = Visibility.Visible;
                _holidayLabel.Visibility = Visibility.Visible;

                string holidayName = _holidays.First().Name;
                if (_holidays != null)
                {
                    _holidayLabel.Text = holidayName;
                }
            }
            else
            {
                _holidayOverlay.Visibility = Visibility.Collapsed;
                _holidayLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void MainCalendarSquare_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var item = DataPackageHelpers.GetViewItem<ViewItemTaskOrEvent>(e.DataView);
                if (item != null)
                {
                    OnRequestChangeItemDate?.Invoke(this, new ChangeItemDateEventArgs(item, this.Date.Date));
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void MainCalendarSquare_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                var item = DataPackageHelpers.GetViewItem<ViewItemTaskOrEvent>(e.DataView);
                if (item != null)
                {
                    if (item.EffectiveDateForDisplayInDateBasedGroups.Date != this.Date.Date)
                    {
                        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static SolidColorBrush makeBrush(byte val)
        {
            return new SolidColorBrush(Color.FromArgb(255, val, val, val));
        }

        public override void ClearItems()
        {
            throw new NotImplementedException();
        }

        public override void InsertItem(int index, IRenderable el)
        {
            throw new NotImplementedException();
        }

        protected override UIElement removeItem(IRenderable el)
        {
            throw new NotImplementedException();
        }
        
        private TextBlock _textBlock;
        private VirtualizedItemsControl _itemsControl;
        private Button _addButton;
        private Rectangle _holidayOverlay;
        private TextBlock _holidayLabel;

        private const double HEADER_HEIGHT = 41;

        protected override void Render(DateTime date)
        {
            base.Margin = new Windows.UI.Xaml.Thickness(1.5);

            base.RowDefinitions.Add(new Windows.UI.Xaml.Controls.RowDefinition() { Height = new GridLength(HEADER_HEIGHT) });
            base.RowDefinitions.Add(new Windows.UI.Xaml.Controls.RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            base.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Holiday background
            _holidayOverlay = new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Opacity = 0.2,
                Visibility = Visibility.Collapsed
            };
            Grid.SetRowSpan(_holidayOverlay, 2);
            Grid.SetColumnSpan(_holidayOverlay, 2);
            base.Children.Add(_holidayOverlay);

            // Day number
            _textBlock = new TextBlock()
            {
                Text = date.Day.ToString(),
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(13,0, 10, 2),
                FontWeight = FontWeights.ExtraLight
            };

            base.Children.Add(_textBlock);

            // Holiday label
            _holidayLabel = new TextBlock()
            {
                Text = "Holiday", // No need to localize, it will use the name of the holiday
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.ExtraLight,
                Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(_holidayLabel, 1);
            base.Children.Add(_holidayLabel);

            // Add button
            _addButton = new Button()
            {
                Width = HEADER_HEIGHT,
                Height = HEADER_HEIGHT,
                Style = App.Current.Resources["TransparentButtonStyle"] as Style,
                Content = new SymbolIcon(Symbol.Add)
                {
                    RenderTransform = new ScaleTransform()
                    {
                        CenterX = 0.4,
                        CenterY = 0.4
                    },
                    RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5)
                },
                HorizontalAlignment = HorizontalAlignment.Right,
                Opacity = 0
            };
            _addButton.Tapped += _addButton_Tapped;

            Grid.SetColumn(_addButton, 1);

            base.Children.Add(_addButton);



            // Task and event items
            _itemsControl = new VirtualizedItemsControl()
            {
                ItemTemplate = (DataTemplate)App.Current.Resources["MainCalendarItemTemplate"]
            };
            Grid.SetRow(_itemsControl, 1);
            Grid.SetColumnSpan(_itemsControl, 2);
            base.Children.Add(_itemsControl);
        }

        private void _addButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            AddButtonClicked();
        }

        private void AddButtonClicked()
        {
            try
            {
                var parent = VisualTreeExtensions.FindParent<MainCalendarView>(this);
                if (parent == null)
                    throw new NullReferenceException("Couldn't find parent.");

                CalendarViewModel viewModel = parent.ViewModel;
                if (viewModel == null)
                    throw new NullReferenceException("Parent's view model was null");

                App.ShowFlyoutAddTaskOrEvent(
                    elToCenterFrom: _addButton,
                    addTaskAction: delegate { viewModel.AddTask(base.Date); },
                    addEventAction: delegate { viewModel.AddEvent(base.Date); },
                    addHolidayAction: delegate { viewModel.AddHoliday(base.Date); });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override void SetBackgroundColor(TCalendarSquare.DisplayType type, bool isMouseHovering, bool isMouseDown)
        {
            //default calendar does not have a "selected" mode, and also doesn't have a mouse hovering mode
            switch (type)
            {
                case DisplayType.ThisMonth:
                case DisplayType.ThisMonthSelected:

                    if (isMouseDown)
                        base.Background = CalendarView.SquareBackgroundThisMonthMouseDown;

                    else
                        base.Background = CalendarView.SquareBackgroundThisMonth;

                    break;

                case DisplayType.Today:
                case DisplayType.TodaySelected:

                    if (isMouseDown)
                        base.Background = CalendarView.SquareBackgroundTodayMouseDown;

                    else
                        base.Background = CalendarView.SquareBackgroundToday;

                    break;

                case DisplayType.OtherMonth:
                case DisplayType.OtherMonthSelected:

                    if (isMouseDown)
                        base.Background = CalendarView.SquareBackgroundOtherMonthMouseDown;

                    else
                        base.Background = CalendarView.SquareBackgroundOtherMonth;

                    break;
            }
        }

        protected override void SetForegroundColor(TCalendarSquare.DisplayType type, bool isMouseHovering, bool isMouseDown)
        {
            //default calendar's foreground always stays same except on Today
            if (type == DisplayType.Today || type == DisplayType.TodaySelected)
            {
                _textBlock.Foreground = CalendarView.SquareForegroundToday;
                _addButton.Foreground = CalendarView.SquareForegroundToday;
                _holidayLabel.Foreground = CalendarView.SquareForegroundToday;
            }
            else
            {
                _textBlock.Foreground = CalendarView.SquareForegroundNormal;
                _addButton.Foreground = CalendarView.SquareForegroundNormal;
                _holidayLabel.Foreground = CalendarView.SquareForegroundNormal;
            }
        }

        protected override void OnMouseOverChanged(PointerRoutedEventArgs e)
        {
            base.OnMouseOverChanged(e);
            
            _addButton.Opacity = base.IsMouseOver ? 1 : 0;
        }
    }
}
