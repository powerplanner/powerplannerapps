using InterfacesUWP;
using PowerPlannerSending;
using PowerPlannerUWP.Views.ScheduleViews;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewItems;
using Windows.Globalization.DateTimeFormatting;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewLists;
using System.ComponentModel;
using InterfacesUWP.Converters;
using System.Globalization;
using PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary;
using PowerPlannerUWP.TileHelpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScheduleView : MainScreenContentViewHostGeneric
    {
        private class CurrentWeekToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                if (value is Schedule.Week)
                {
                    return $", {LocalizedResources.Common.GetLocalizedWeek((Schedule.Week)value)}";
                }

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }

        private static readonly int INITIAL_MARGIN = 55;
        public static readonly int HEIGHT_OF_HOUR = 120;

        private FrameworkElement _appBarContent;
        private FrameworkElement AppBarContent
        {
            get
            {
                if (_appBarContent == null)
                {
                    var sp = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    AppBarButton buttonPrev = CreateAppBarButton(Symbol.Back, LocalizedResources.GetString("String_LastWeek"), ButtonPrevious_Click);
                    AppBarButton buttonNext = CreateAppBarButton(Symbol.Forward, LocalizedResources.GetString("String_NextWeek"), ButtonNext_Click);
                    buttonPrev.IsCompact = true;
                    buttonNext.IsCompact = true;

                    sp.Children.Add(buttonPrev);
                    sp.Children.Add(buttonNext);

                    var tbStart = new TextBlock()
                    {
                        Margin = new Thickness(3, 9, 0, 0),
                        Style = Resources["SubtitleTextBlockStyle"] as Style
                    };
                    tbStart.SetBinding(TextBlock.TextProperty, new Binding()
                    {
                        Source = ViewModel,
                        Path = new PropertyPath(nameof(ViewModel.DisplayStartDate)),
                        Converter = new DateToStringConverter(),
                        ConverterParameter = GetWeekDateConverterParameter()
                    });
                    sp.Children.Add(tbStart);

                    var tbMiddle = new TextBlock()
                    {
                        Margin = new Thickness(4, 9, 0, 0),
                        Text = "-",
                        Style = Resources["SubtitleTextBlockStyle"] as Style
                    };
                    sp.Children.Add(tbMiddle);

                    var tbEnd = new TextBlock()
                    {
                        Margin = new Thickness(4, 9, 0, 0),
                        Style = Resources["SubtitleTextBlockStyle"] as Style
                    };
                    tbEnd.SetBinding(TextBlock.TextProperty, new Binding()
                    {
                        Source = ViewModel,
                        Converter = new DateToStringConverter(),
                        Path = new PropertyPath(nameof(ViewModel.DisplayEndDate)),
                        ConverterParameter = GetWeekDateConverterParameter()
                    });
                    sp.Children.Add(tbEnd);

                    var tbYear = new TextBlock()
                    {
                        Margin = new Thickness(6, 15, 0, 0),
                        Opacity = 0.6
                    };
                    tbYear.SetBinding(TextBlock.TextProperty, new Binding()
                    {
                        Source = ViewModel,
                        Path = new PropertyPath(nameof(ViewModel.DisplayStartDate)),
                        Converter = new DateToStringConverter(),
                        ConverterParameter = "yyyy"
                    });
                    sp.Children.Add(tbYear);

                    var tbWeek = new TextBlock()
                    {
                        Margin = new Thickness(0, 15, 0, 0),
                        Opacity = 0.6
                    };
                    tbWeek.SetBinding(VisibilityProperty, new Binding()
                    {
                        Source = ViewModel,
                        Path = new PropertyPath(nameof(ViewModel.HasTwoWeekSchedule)),
                        Converter = new BoolToVisibilityConverter()
                    });
                    tbWeek.SetBinding(TextBlock.TextProperty, new Binding()
                    {
                        Source = ViewModel,
                        Path = new PropertyPath(nameof(ViewModel.CurrentWeek)),
                        Converter = new CurrentWeekToStringConverter()
                    });
                    sp.Children.Add(tbWeek);


                    _appBarContent = sp;
                }

                return _appBarContent;
            }
        }

        private static string GetWeekDateConverterParameter()
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("en"))
            {
                return "M/d";
            }
            else
            {
                return "d";
            }
        }

        private AppBarButton _appBarButtonPin;
        private AppBarButton AppBarButtonPin
        {
            get
            {
                if (_appBarButtonPin == null)
                    _appBarButtonPin = CreateAppBarButton(Symbol.Pin, LocalizedResources.Common.GetStringPinToStart(), ButtonPinToStart_Click);
                
                return _appBarButtonPin;
            }
        }

        private AppBarButton _appBarButtonEdit;
        private AppBarButton AppBarButtonEdit
        {
            get
            {
                if (_appBarButtonEdit == null)
                    _appBarButtonEdit = CreateAppBarButton(Symbol.Edit, LocalizedResources.GetString("String_EditSchedule"), ButtonEdit_Click);

                return _appBarButtonEdit;
            }
        }

        private AppBarButton _appBarButtonCloseEdit;
        private AppBarButton AppBarButtonCloseEdit
        {
            get
            {
                if (_appBarButtonCloseEdit == null)
                    _appBarButtonCloseEdit = CreateAppBarButton(Symbol.Cancel, LocalizedResources.Common.GetStringClose(), ButtonCloseEdit_Click);

                return _appBarButtonCloseEdit;
            }
        }

        private AppBarButton _appBarButtonExportToImage;
        private AppBarButton AppBarButtonExportToImage
        {
            get
            {
                if (_appBarButtonExportToImage == null)
                    _appBarButtonExportToImage = CreateAppBarButton(Symbol.Pictures, LocalizedResources.GetString("String_ExportToImage"), ButtonExportToImage_Click);

                return _appBarButtonExportToImage;
            }
        }

        private ICommandBarElement[] _secondaryMenuCommands;
        private ICommandBarElement[] SecondaryMenuCommands
        {
            get
            {
                if (_secondaryMenuCommands == null)
                    _secondaryMenuCommands = new ICommandBarElement[] { AppBarButtonExportToImage };

                return _secondaryMenuCommands;
            }
        }

        private TextBlock _tbWeekOne;
        private TextBlock tbWeekOne
        {
            get
            {
                if (_tbWeekOne == null)
                {
                    _tbWeekOne = new TextBlock()
                    {
                        Text = LocalizedResources.Common.GetStringWeekA(),
                        Style = Resources["SelectedWeekStyle"] as Style
                    };

                    _tbWeekOne.Tapped += tbWeekOne_Tapped;
                }

                return _tbWeekOne;
            }
        }

        private TextBlock _tbWeekTwo;
        private TextBlock tbWeekTwo
        {
            get
            {
                if (_tbWeekTwo == null)
                {
                    _tbWeekTwo = new TextBlock()
                    {
                        Text = LocalizedResources.Common.GetStringWeekB(),
                        Style = Resources["WeekStyle"] as Style
                    };

                    _tbWeekTwo.Tapped += tbWeekTwo_Tapped;
                }

                return _tbWeekTwo;
            }
        }

        private ScrollViewer _selectWeekUIElement;
        private ScrollViewer SelectWeekUIElement
        {
            get
            {
                if (_selectWeekUIElement == null)
                {
                    _selectWeekUIElement = new ScrollViewer()
                    {
                        Visibility = Visibility.Collapsed,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
                    };

                    StackPanel sp = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,

                        Children =
                        {
                            tbWeekOne,
                            tbWeekTwo
                        }
                    };

                    _selectWeekUIElement.Content = sp;
                }

                return _selectWeekUIElement;
            }
        }

        private static double getTopMargin(TimeSpan itemTime, TimeSpan baseTime)
        {
            return INITIAL_MARGIN + Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0);
        }

        public ScheduleView()
        {
            this.InitializeComponent();
        }
        
        public new ScheduleViewModel ViewModel
        {
            get { return base.ViewModel as ScheduleViewModel; }
            set { base.ViewModel = value; }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();
            
            try
            {
                ViewModel.InitializeArrangers(HEIGHT_OF_HOUR, MyCollapsedEventItem.SPACING_WITH_NO_ADDITIONAL, MyCollapsedEventItem.SPACING_WITH_ADDITIONAL, MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM);

                updateWeekDisplay(ViewModel.CurrentWeek);

                ViewModel.OnItemsForDateChanged += new WeakEventHandler<DateTime>(ViewModel_OnItemsForDateChanged).Handler;
                ViewModel.OnFullReset += new WeakEventHandler<EventArgs>(ViewModel_OnFullReset).Handler;
                ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

                UpdateState();
                UpdateDayHeaders();

                RenderAll();

                UpdatePinVisibility();

                ViewModel.BackRequested += new WeakEventHandler<CancelEventArgs>(ViewModel_BackRequested).Handler;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ViewModel_BackRequested(object sender, CancelEventArgs e)
        {
            if (HideAllExpansions())
            {
                e.Cancel = true;
            }
        }

        private void UpdateDayHeaders()
        {
            DayOfWeek day = ViewModel.FirstDayOfWeek;
            foreach (var tb in GridDaysHeader.Children.OfType<TextBlock>())
            {
                tb.Text = DateTools.ToLocalizedString(day);

                day++;
            }
        }

        private void ViewModel_OnFullReset(object sender, EventArgs e)
        {
            RenderAll();
        }

        private void ViewModel_OnItemsForDateChanged(object sender, DateTime e)
        {
            RenderDate(e);
        }

        private IEnumerable<ItemsControl> GetAllDayItemsControls()
        {
            return AllDayItemsGrid.Children.OfType<ScrollViewer>().Select(i => i.Content).OfType<ItemsControl>();
        }

        private void RenderDate(DateTime date)
        {
            int col = getColumn(date.DayOfWeek);

            grid.ColumnDefinitions[col].Width = (GridLength)Resources["ColumnGridLength"];
            GridDaysHeader.ColumnDefinitions[col].Width = (GridLength)Resources["ColumnGridLength"];
            AllDayItemsGrid.ColumnDefinitions[col].Width = (GridLength)Resources["ColumnGridLength"];

            // Clear current items (skip first since that's the rectangle background)
            var toRemove = ChildrenInColumn(col).ToArray();
            foreach (var remove in toRemove)
            {
                grid.Children.Remove(remove);
            }

            // Get arranger for the date
            var arranger = ViewModel.Items[date.DayOfWeek];

            // Assign all day items (minus 1 since the column includes the first column for times)
            GetAllDayItemsControls().ElementAt(col - 1).ItemsSource = arranger.HolidayAndAllDayItems;

            foreach (var s in arranger.ScheduleItems)
            {
                MyScheduleItem visual = new MyScheduleItem()
                {
                    Schedule = s.Item,
                    Height = s.Height
                };

                visual.Tapped += ScheduleItem_Tapped;

                visual.PointerEntered += ScheduleItem_PointerEntered;
                visual.PointerExited += ScheduleItem_PointerExited;

                AddVisualItem(visual, s, arranger.Date.DayOfWeek);
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in arranger.EventItems.Reverse())
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
                    visual = new MyFullEventItem
                    {
                        Item = e
                    };
                }

                AddVisualItem(visual, e, arranger.Date.DayOfWeek);
            }

            if (!ChildrenInColumn(col).Any() && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) && !arranger.HolidayAndAllDayItems.Any())
            {
                grid.ColumnDefinitions[col].Width = new GridLength(0);
                GridDaysHeader.ColumnDefinitions[col].Width = new GridLength(0);
                AllDayItemsGrid.ColumnDefinitions[col].Width = new GridLength(0);
            }

            if (arranger.IsDifferentSemester)
            {
                var diffSemesterOverlay = new DifferentSemesterOverlayControl();
                Grid.SetColumn(diffSemesterOverlay, col);
                Grid.SetRowSpan(diffSemesterOverlay, int.MaxValue);
                diffSemesterOverlay.VerticalAlignment = VerticalAlignment.Top;
                diffSemesterOverlay.Height = 300;
                grid.Children.Add(diffSemesterOverlay);
            }

            if (arranger.HasHolidays)
            {
                // Dim children
                foreach (var child in ChildrenInColumn(col))
                {
                    child.Opacity = 0.7;
                }

                var holidayOverlay = new Rectangle()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(80, 228, 0, 137))
                };
                Grid.SetColumn(holidayOverlay, col);
                Grid.SetRowSpan(holidayOverlay, int.MaxValue);
                grid.Children.Add(holidayOverlay);
            }

            UpdateTimePaddingBasedOnAllDayItems();
        }

        public IEnumerable<FrameworkElement> ChildrenInColumn(int col)
        {
            // Skip first since that's the rectangle background
            return grid.Children.OfType<FrameworkElement>().Where(i => Grid.GetColumn(i) == col).Skip(1);
        }

        private void AddVisualItem(FrameworkElement visual, DayScheduleItemsArranger.BaseScheduleItem item, DayOfWeek day)
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

            double leftMargin = item.LeftOffset;
            leftMargin += 12;

            root.Margin = new Thickness(leftMargin, item.TopOffset + INITIAL_MARGIN, 12, 24);
            Grid.SetColumn(root, getColumn(day));
            Grid.SetRowSpan(root, int.MaxValue);

            grid.Children.Add(root);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.LayoutMode):
                    UpdateState();
                    break;

                case nameof(ViewModel.HasAllDayItems):
                    UpdateTimePaddingBasedOnAllDayItems();
                    break;
            }
        }

        private void UpdateState()
        {
            switch (ViewModel.LayoutMode)
            {
                case ScheduleViewModel.LayoutModes.Normal:
                    GoToNormalState();
                    break;

                case ScheduleViewModel.LayoutModes.SplitEditing:
                    GoToEditingState();
                    break;

                case ScheduleViewModel.LayoutModes.FullEditing:
                    GoToFullEditingState();
                    break;

                case ScheduleViewModel.LayoutModes.Welcome:
                    GoToWelcomeState();
                    break;
            }
        }

        private void SetCommandBarContent()
        {
            SetCommandBarContent(AppBarContent);
        }

        private void SetNormalCommandBarButtons()
        {
            SetCommandBarCommands(new ICommandBarElement[]
            {
                AppBarButtonPin,
                AppBarButtonEdit
            }, SecondaryMenuCommands);
        }

        private void SetFullEditingCommandBarButtons()
        {
            SetCommandBarCommands(new ICommandBarElement[0], null);
        }

        private void SetEditingCommandBarButtons()
        {
            SetCommandBarCommands(new ICommandBarElement[]
            {
                AppBarButtonCloseEdit
            }, null);
        }

        private void RenderAll()
        {
            try
            {
                // Remove current items (only other items in this grid are the rectangles (the backgrounds for the columns)
                foreach (var item in grid.Children.Where(i => !(i is Rectangle)).ToArray())
                {
                    grid.Children.Remove(item);
                }

                // Clear the times
                foreach (var tb in GridTimes.Children.OfType<TextBlock>().ToArray())
                    GridTimes.Children.Remove(tb);

                // If there's no items, stop
                if (!ViewModel.IsValid())
                {
                    return;
                }

                // Get earliest start and end date
                DateTime today = DateTime.Today;
                DateTime classStartTime = today.Add(ViewModel.StartTime);
                DateTime classEndTime = today.Add(ViewModel.EndTime);

                var timeFormatter = new DateTimeFormatter("{hour.integer}‎:‎{minute.integer(2)}");

                // Fill in the times on the left column
                for (DateTime tempClassStartTime = classStartTime; classEndTime >= tempClassStartTime; tempClassStartTime = tempClassStartTime.AddHours(1))
                {
                    GridTimes.Children.Add(new TextBlock()
                    {
                        Text = timeFormatter.Format(tempClassStartTime),
                        Style = (Style)Resources["TimeStyle"],
                        Margin = new Thickness
                            (
                                12,
                                getTopMargin(tempClassStartTime.TimeOfDay, classStartTime.TimeOfDay) - 4,
                                12,
                                24
                            )
                    });
                }

                for (int i = 0; i < 7; i++)
                {
                    RenderDate(ViewModel.StartDate.AddDays(i));
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ScheduleItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEditing())
            {
                return;
            }

            var schedule = (sender as MyScheduleItem).Schedule;

            UnhighlightSchedules(schedule.Class.GetGroupOfSchedulesWithSharedEditingValues(schedule));
        }

        private void ScheduleItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEditing())
            {
                return;
            }

            var schedule = (sender as MyScheduleItem).Schedule;

            HighlightSchedules(
                schedule.Class.GetGroupOfSchedulesWithSharedEditingValues(schedule),
                scrollToArea: false);
        }

        private int getColumn(ViewItemSchedule s)
        {
            return getColumn(s.DayOfWeek);
        }

        private int getColumn(DayOfWeek dayOfWeek)
        {
            int answer = (int)dayOfWeek - (int)ViewModel.FirstDayOfWeek;
            if (answer < 0)
            {
                answer = answer + 7;
            }
            return answer + 1; // Plus 1 since we skip the times column
        }

        private void ScheduleItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var schedule = (sender as MyScheduleItem).Schedule;
                
                // If currently editing
                if (IsEditing())
                {
                    // Open the editing for that schedule group
                    ViewModel.EditTimes(schedule);
                }

                // Otherwise...
                else
                {
                    // Open the class
                    ViewModel.ViewClass(schedule.Class);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool IsEditing()
        {
            return ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.FullEditing || ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.SplitEditing;
        }

        private void updateWeekDisplay(Schedule.Week week)
        {
            if (week == Schedule.Week.WeekOne)
            {
                tbWeekOne.Style = (Style)Resources["SelectedWeekStyle"];
                tbWeekTwo.Style = (Style)Resources["WeekStyle"];
            }

            else
            {
                tbWeekOne.Style = (Style)Resources["WeekStyle"];
                tbWeekTwo.Style = (Style)Resources["SelectedWeekStyle"];
            }
        }

        private void tbWeekTwo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO
            //try
            //{
            //    if (tbWeekTwo.Style == Resources["SelectedWeekStyle"])
            //        return;

            //    ViewModel.CurrentWeek = Schedule.Week.WeekTwo;
            //    updateWeekDisplay(Schedule.Week.WeekTwo);

            //    GenerateSchedule();
            //}

            //catch (Exception ex)
            //{
            //    TelemetryExtension.Current?.TrackException(ex);
            //}
        }

        private void tbWeekOne_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO
            //try
            //{
            //    if (tbWeekOne.Style == Resources["SelectedWeekStyle"])
            //        return;

            //    ViewModel.CurrentWeek = Schedule.Week.WeekOne;
            //    updateWeekDisplay(Schedule.Week.WeekOne);

            //    GenerateSchedule();
            //}

            //catch (Exception ex)
            //{
            //    TelemetryExtension.Current?.TrackException(ex);
            //}
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PreviousWeek();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NextWeek();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EnterEditMode();
        }

        private void ButtonSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExitEditMode();
        }

        private void ButtonCloseEdit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExitEditMode();
        }

        private async void ButtonPinToStart_Click(object sender, RoutedEventArgs e)
        {
            if ((AppBarButtonPin.Icon as SymbolIcon).Symbol == Symbol.Pin)
            {
                try
                {
                    var currAccount = ViewModel.MainScreenViewModel.CurrentAccount;
                    var currData = await AccountDataStore.Get(currAccount.LocalAccountId);
                    await ScheduleTileHelper.PinTile(currAccount, currData);

                    UpdatePinVisibility();
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            else
            {
                try
                {
                    await ScheduleTileHelper.UnpinTile(ViewModel.MainScreenViewModel.CurrentLocalAccountId);

                    UpdatePinVisibility();
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private void UpdatePinVisibility()
        {
            if (ScheduleTileHelper.IsPinned(ViewModel.MainScreenViewModel.CurrentLocalAccountId))
            {
                AppBarButtonPin.Icon = new SymbolIcon(Symbol.UnPin);
                AppBarButtonPin.Label = LocalizedResources.Common.GetStringUnpinFromStart();
            }

            else
            {
                AppBarButtonPin.Icon = new SymbolIcon(Symbol.Pin);
                AppBarButtonPin.Label = LocalizedResources.Common.GetStringPinToStart();
            }
        }

        private void GoToEditingState()
        {
            SetEditingCommandBarButtons();
            SetCommandBarContent();

            VisualStateManager.GoToState(this, "EditingState", true);
        }

        private void GoToFullEditingState()
        {
            SetFullEditingCommandBarButtons();
            SetCommandBarContent(null);

            VisualStateManager.GoToState(this, "FullEditingState", true);
        }

        private void GoToNormalState()
        {
            SetNormalCommandBarButtons();
            SetCommandBarContent();

            VisualStateManager.GoToState(this, "DefaultState", true);
        }

        private void GoToWelcomeState()
        {
            HideCommandBar();

            VisualStateManager.GoToState(this, "WelcomeState", true);
            ButtonWelcomeAddClass.Content = PowerPlannerResources.GetString("SchedulePage_ButtonAddClass/Content").ToUpper();
        }

        private void ButtonAddClass_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddClass();
        }

        private IEnumerable<MyScheduleItem> GetMyScheduleItems()
        {
            return grid.Children.OfType<MyScheduleItem>().Concat(grid.Children.OfType<Grid>().SelectMany(i => i.Children).OfType<MyScheduleItem>());
        }

        private void HighlightSchedules(IEnumerable<ViewItemSchedule> schedules, bool scrollToArea = true)
        {
            // Unhighlight everything else
            foreach (var scheduleItem in GetMyScheduleItems().Where(i => !schedules.Contains(i.Schedule)))
                scheduleItem.IsHighlighted = false;

            // If empty, do nothing
            if (!schedules.Any())
                return;

            MyScheduleItem[] scheduleItems = GetMyScheduleItems().Where(i => schedules.Contains(i.Schedule)).ToArray();

            if (scheduleItems.Length == 0)
                return;

            foreach (var i in scheduleItems)
                i.IsHighlighted = true;


            if (scrollToArea)
            {
                Rect entireArea;
                bool first = true;

                foreach (var s in scheduleItems)
                {
                    Rect rect = new Rect();
                    rect = s.TransformToVisual(grid).TransformBounds(rect);
                    rect.Width = s.ActualWidth;
                    rect.Height = s.ActualHeight;

                    if (first)
                    {
                        entireArea = rect;
                        first = false;
                    }

                    else
                        entireArea.Union(rect);
                }

                if (entireArea.IsEmpty)
                    return;

                double leftPadding = GetActualWidthOfTimesColumn() + 20;
                double topPadding = INITIAL_MARGIN;

                double decreaseX = Math.Min(entireArea.X, leftPadding);
                double decreaseY = Math.Min(entireArea.Y, topPadding);

                entireArea.X -= decreaseX;
                entireArea.Y -= decreaseY;

                entireArea.Height += 20 + decreaseY;
                entireArea.Width += 20 + decreaseX;

#if NOT_NEEDED_DEBUG
            DebuggingHighlightRegionOnSchedule(entireArea);
#endif

                MyScrollViewerExtensions.ShowRegion(ScrollViewerSchedule, entireArea);
            }
        }

#if DEBUG
        private Rectangle _debugHighlightRectangle;
        private void DebuggingHighlightRegionOnSchedule(Rect region)
        {
            if (_debugHighlightRectangle == null)
            {
                _debugHighlightRectangle = new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.Red) { Opacity = 0.5 },
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumnSpan(_debugHighlightRectangle, 10);
                grid.Children.Add(_debugHighlightRectangle);
            }

            _debugHighlightRectangle.Height = region.Height;
            _debugHighlightRectangle.Width = region.Width;
            _debugHighlightRectangle.Margin = new Thickness(region.X, region.Y, 0, 0);
        }
#endif

        private int GetDayOfWeekNumber(DayOfWeek dayOfWeek)
        {
            // Make Monday be first day
            int answer = (int)dayOfWeek - 1;
            if (answer == -1)
                return 6;
            return answer;
        }

        private double GetActualWidthOfTimesColumn()
        {
            return grid.ColumnDefinitions.First().ActualWidth;
        }

        private double GetScrollViewerTopCoordinate()
        {
            return ScrollViewerSchedule.VerticalOffset;
        }

        private double GetScrollViewerBottomCoordinate()
        {
            return GetScrollViewerTopCoordinate() + ScrollViewerSchedule.ViewportHeight;
        }

        private double GetScrollViewerLeftCoordinate()
        {
            return ScrollViewerSchedule.HorizontalOffset;
        }

        private double GetScrollViewerRightCoordinate()
        {
            return GetScrollViewerLeftCoordinate() + ScrollViewerSchedule.ViewportWidth;
        }

        private void UnhighlightSchedules(IEnumerable<ViewItemSchedule> schedules)
        {
            if (!schedules.Any())
                return;

            // Unhighlight
            foreach (var scheduleItem in GetMyScheduleItems().Where(i => schedules.Contains(i.Schedule)))
                scheduleItem.IsHighlighted = false;
        }

        private void EditingAllSchedulesSingleClassControl_HighlightRequested(object sender, ViewItemSchedule[] e)
        {
            try
            {
                HighlightSchedules(e);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void EditingAllSchedulesSingleClassControl_UnhighlightRequested(object sender, ViewItemSchedule[] e)
        {
            try
            {
                UnhighlightSchedules(e);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ScrollViewerSchedule_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            TransformGridDaysHeader.TranslateX = e.NextView.HorizontalOffset * -1;
            TransformGridDaysHeader.TranslateY = 0;
            TransformGridDaysHeader.ScaleX = e.NextView.ZoomFactor;
            TransformGridDaysHeader.ScaleY = e.NextView.ZoomFactor;

            TransformStackPanelTimes.TranslateY = e.NextView.VerticalOffset * -1;
            TransformStackPanelTimes.TranslateX = 0;
            TransformStackPanelTimes.ScaleX = e.NextView.ZoomFactor;
            TransformStackPanelTimes.ScaleY = e.NextView.ZoomFactor;

            // If scrolled out and
            // ONLY if there was originally scrollable width... otherwise it snaps to the left
            if (ScrollViewerSchedule.ExtentWidth < ScrollViewerSchedule.ViewportWidth && (ScrollViewerSchedule.Content as FrameworkElement).ActualWidth > ScrollViewerSchedule.ViewportWidth)
            {
                // Need to shrink to the center
                TransformGridDaysHeader.TranslateX = (ScrollViewerSchedule.ViewportWidth - ScrollViewerSchedule.ExtentWidth) / 2;

                // We also need to move the left header over towards the center
                TransformStackPanelTimes.TranslateX = TransformGridDaysHeader.TranslateX;
            }

            // If scrolled out and
            // ONLY if there was originally scrollable height... otherwise it snaps to the top for some reason
            if (ScrollViewerSchedule.ExtentHeight < ScrollViewerSchedule.ViewportHeight && (ScrollViewerSchedule.Content as FrameworkElement).ActualHeight > ScrollViewerSchedule.ViewportHeight)
            {
                // Need to shrink to the center
                TransformStackPanelTimes.TranslateY = (ScrollViewerSchedule.ViewportHeight - ScrollViewerSchedule.ExtentHeight) / 2;

                // We also need to move the top header down towards the center
                TransformGridDaysHeader.TranslateY = TransformStackPanelTimes.TranslateY;
            }

            HideAllExpansions();
        }

        private bool HideAllExpansions()
        {
            bool closedExpansion = false;
            foreach (var vis in GetEventVisuals())
            {
                closedExpansion = vis.HideFull() || closedExpansion;
            }

            return closedExpansion;
        }

        private IEnumerable<MyBaseEventVisual> GetEventVisuals()
        {
            return grid.Children.Concat(grid.Children.OfType<Grid>()).OfType<MyBaseEventVisual>();
        }

        private void GridTimes_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridDaysHeaderTimesColumn.Width = new GridLength(e.NewSize.Width);
            SchedulesGridTimesColumn.Width = new GridLength(e.NewSize.Width);
        }

        private void EditingAllSchedulesSingleClassControl_OnRequestAddTime(object sender, ViewItemClass e)
        {
            ViewModel.AddTime(e);
        }

        private void EditingAllSchedulesSingleClassControl_OnRequestEditClass(object sender, ViewItemClass e)
        {
            ViewModel.EditClass(e);
        }

        private void EditingAllSchedulesSingleClassControl_OnRequestEditGroup(object sender, ViewItemSchedule[] e)
        {
            ViewModel.EditTimes(e);
        }

        private void thisPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        private void GridDaysHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var control in GetAllDayItemsControls())
            {
                control.Margin = new Thickness(1, e.NewSize.Height, 1, 0);
            }
        }

        private void AllDayItemsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTimePaddingBasedOnAllDayItems();
        }

        private void UpdateTimePaddingBasedOnAllDayItems()
        {
            if (ViewModel != null)
            {
                if (ViewModel.HasAllDayItems)
                {
                    GridTimes.Padding = new Thickness(0, AllDayItemsGrid.ActualHeight, 0, 0);
                    RectangleGridTimes.Margin = new Thickness(0, -1 * AllDayItemsGrid.ActualHeight, 0, 0);
                }
                else
                {
                    GridTimes.Padding = new Thickness();
                    RectangleGridTimes.Margin = new Thickness();
                }
            }
        }

        private void ScrollViewerSchedule_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideAllExpansions();
        }

        private void ButtonExportToImage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MainScreenViewModel.ShowPopup(new ExportSchedulePopupViewModel(ViewModel.MainScreenViewModel, ViewModel));
        }

        public void ConfigureForExport()
        {
            ScrollViewerSchedule.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewerSchedule.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private void ButtonLogIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LogIn();
        }
    }
}
