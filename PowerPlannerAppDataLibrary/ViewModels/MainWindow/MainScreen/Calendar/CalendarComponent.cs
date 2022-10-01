using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx;
using Vx.Views;
using Vx.Views.DragDrop;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar
{
    public class CalendarComponent : VxComponent
    {
        [VxSubscribe]
        private CalendarViewModel _viewModel;

        private DateTime _thisMonth;
        private Func<int, View> _itemTemplate;
        private View _addButtonRef, _filterButtonRef;

        public static bool IntegratedTopControls = VxPlatform.Current == Platform.Uwp;

        public override bool SubscribeToIsMouseOver => IntegratedTopControls;

        public bool IsFullSize => _viewModel.DisplayState == CalendarViewModel.DisplayStates.FullCalendar;

        public static Color CalendarColor => Theme.Current.IsDarkTheme ? Color.FromArgb(18, 18, 18) : Color.FromArgb(240, 240, 240);

        public CalendarComponent(CalendarViewModel viewModel)
        {
            _viewModel = viewModel;
            _thisMonth = DateTools.GetMonth(DateTime.Today);
            _itemTemplate = RenderContent;
        }

        protected override View Render()
        {
            var slideView = new SlideView
            {
                Position = VxValue.Create(DateTools.DifferenceInMonths(_viewModel.DisplayMonth, _thisMonth), i => _viewModel.DisplayMonth = _thisMonth.AddMonths(i)),
                ItemTemplate = _itemTemplate
            };

            if (IntegratedTopControls)
            {
                return new FrameLayout
                {
                    Children =
                    {
                        slideView,

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, IsFullSize ? 20 : 10, 0, 0),
                            Children =
                            {
                                CreateArrowButton(
                                    glyph: MaterialDesign.MaterialDesignIcons.ArrowLeft,
                                    altText: "Previous month",
                                    click: () => _viewModel.Previous(),
                                    margin: new Thickness(0)),

                                new Border().LinearLayoutWeight(1),

                                CreateIconButton(
                                    glyph: MaterialDesign.MaterialDesignIcons.Add,
                                    tooltipText: PowerPlannerResources.GetString("Calendar_FullCalendarAddButton.ToolTipService.ToolTip"),
                                    altText: null,
                                    click: () => new ContextMenu
                                    {
                                        Items =
                                        {
                                            new ContextMenuItem
                                            {
                                                Text = PowerPlannerResources.GetString("String_Task"),
                                                Click = () => _viewModel.AddTask(false)
                                            },
                                            new ContextMenuItem
                                            {
                                                Text = PowerPlannerResources.GetString("String_Event"),
                                                Click = () => _viewModel.AddEvent(false)
                                            },
                                            new ContextMenuItem
                                            {
                                                Text = PowerPlannerResources.GetString("String_Holiday"),
                                                Click = () => _viewModel.AddHoliday(false)
                                            }
                                        }
                                    }.Show(_addButtonRef), v => _addButtonRef = v),

                                // Filter button (only on full size calendar)
                                IsFullSize ? CreateIconButton(
                                    glyph: MaterialDesign.MaterialDesignIcons.FilterAlt,
                                    tooltipText: PowerPlannerResources.GetString("Calendar_FullCalendarFilterButton.ToolTipService.ToolTip"),
                                    altText: null,
                                    click: () => new ContextMenu
                                    {
                                        Items =
                                        {
                                            new ContextMenuItem
                                            {
                                                Text = PowerPlannerResources.GetString(_viewModel.ShowPastCompleteItemsOnFullCalendar ? "HidePastCompleteItems" : "ShowPastCompleteItems.Text"),
                                                Click = () => _viewModel.ShowPastCompleteItemsOnFullCalendar = !_viewModel.ShowPastCompleteItemsOnFullCalendar
                                            }
                                        }
                                    }.Show(_filterButtonRef), v => _filterButtonRef = v) : null,

                                CreateIconButton(
                                    glyph: MaterialDesign.MaterialDesignIcons.Today,
                                    tooltipText: PowerPlannerResources.GetString("String_GoToToday"),
                                    altText: null,
                                    click: () => _viewModel.GoToToday()),

                                CreateArrowButton(
                                    glyph: MaterialDesign.MaterialDesignIcons.ArrowRight,
                                    altText: "Next month",
                                    click: () => _viewModel.Next(),
                                    margin: new Thickness(24, 0, 0, 0))
                            }
                        }
                    }
                };
            }
            else
            {
                return slideView;
            }
        }

        private View RenderContent(int position)
        {
            var month = _thisMonth.AddMonths(position);

            return new CalendarMonthComponent
            {
                Month = month,
                Items = _viewModel.SemesterItemsViewGroup.Items,
                ViewModel = _viewModel
            };
        }

        private TransparentContentButton CreateIconButton(string glyph, string tooltipText, string altText, Action click, Action<View> viewRef = null)
        {
            return new TransparentContentButton
            {
                Content = new FontIcon
                {
                    Glyph = glyph,
                    Color = Theme.Current.SubtleForegroundColor,
                    FontSize = Theme.Current.TitleFontSize,
                    Margin = new Thickness(6)
                },
                ViewRef = viewRef,
                Click = click,
                Margin = new Thickness(9, 0, 0, 0),
                TooltipText = tooltipText,
                AltText = altText
            };
        }

        private TransparentContentButton CreateArrowButton(string glyph, string altText, Action click, Thickness margin)
        {
            return new TransparentContentButton
            {
                Content = new FontIcon
                {
                    Glyph = glyph,
                    Color = Theme.Current.SubtleForegroundColor,
                    FontSize = Theme.Current.TitleFontSize,
                    Margin = new Thickness(6)
                },
                Click = click,
                Margin = margin,
                Opacity = IsMouseOver ? 1 : 0,
                AltText = altText
            };
        }

        private class CalendarMonthComponent : VxComponent
        {
            public DateTime Month { get; set; }
            public MyObservableList<BaseViewItemMegaItem> Items { get; set; }

            // Note that subsequent properties on the ViewModel are NOT observed except for anything defined in ViewModel_PropertyChanged
            public CalendarViewModel ViewModel { get; set; }

            public bool IsFullSize => ViewModel.DisplayState == CalendarViewModel.DisplayStates.FullCalendar;
            public bool IsSplit => ViewModel.DisplayState == CalendarViewModel.DisplayStates.Split;

            protected override void Initialize()
            {
                ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            }

            private VxState<DateTime?> DismissedDifferentSemesterMonth = new VxState<DateTime?>();

            protected override View Render()
            {
                SubscribeToCollection(Items);

                DateTime[,] array = CalendarArray.Generate(Month, ViewModel.FirstDayOfWeek);

                var dayHeaders = new LinearLayout
                {
                    Orientation = Orientation.Horizontal
                };

                for (int i = 0; i < 7; i++)
                {
                    var day = DateTools.ToLocalizedString(ViewModel.FirstDayOfWeek + i);
                    if (!IsFullSize && day.Length > 3)
                    {
                        day = day.Substring(0, 3);
                    }

                    dayHeaders.Children.Add(new TextBlock
                    {
                        Text = day,
                        WrapText = false,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        FontSize = IsFullSize ? 16 : Theme.Current.CaptionFontSize,
                        Margin = new Thickness(
                            left: IsFullSize ? CalendarDayComponent.FullSizeLeftRightMargin : CalendarDayComponent.CompactLeftMargin,
                            top: 6,
                            right: IsFullSize ? CalendarDayComponent.FullSizeLeftRightMargin : CalendarDayComponent.CompactRightMargin,
                            bottom: 6),
                        TextAlignment = IsFullSize ? HorizontalAlignment.Left : HorizontalAlignment.Right
                    }.LinearLayoutWeight(1));
                }

                var grid = new LinearLayout();

                for (int r = 0; r < 6; r++)
                {
                    grid.Children.Add(new Border
                    {
                        BackgroundColor = Theme.Current.SubtleForegroundColor,
                        Opacity = 0.3f,
                        Height = 1
                    });

                    var row = new LinearLayout
                    {
                        Orientation = Orientation.Horizontal
                    };

                    for (int c = 0; c < 7; c++)
                    {
                        var date = array[r, c];

                        row.Children.Add(RenderDay(date).LinearLayoutWeight(1));

                        if (c != 6)
                        {
                            row.Children.Add(new Border
                            {
                                BackgroundColor = Theme.Current.SubtleForegroundColor,
                                Opacity = 0.3f,
                                Width = 1
                            });
                        }
                    }

                    grid.Children.Add(row.LinearLayoutWeight(1));
                }

                View monthHeader = null;

                if (IntegratedTopControls)
                {
                    monthHeader = new TextBlock
                    {
                        Text = Month.ToString("MMMM yyyy"),
                        FontSize = IsFullSize ? Theme.Current.HeaderFontSize : Theme.Current.TitleFontSize,
                        FontWeight = FontWeights.SemiLight,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        Margin = new Thickness(IsFullSize ? 60 : 48, 10, 12, 6),
                        VerticalAlignment = VerticalAlignment.Center,
                        WrapText = false
                    };
                }

                var finalLayout = new LinearLayout
                {
                    BackgroundColor = CalendarColor,
                    Children =
                    {
                        monthHeader,

                        dayHeaders,

                        grid.LinearLayoutWeight(1)
                    }
                };

                // If different semester
                if (ViewModel.SemesterItemsViewGroup.Semester != null && !ViewModel.SemesterItemsViewGroup.Semester.IsMonthDuringThisSemester(Month) && DismissedDifferentSemesterMonth.Value != Month)
                {
                    return new FrameLayout
                    {
                        Children =
                        {
                            finalLayout,

                            new DifferentSemesterComponent()
                            {
                                OnDismiss = () => DismissedDifferentSemesterMonth.Value = Month,
                                OnOpenSemester = () => ViewModel.MainScreenViewModel.OpenYears()
                            }
                        }
                    };
                }
                else
                {
                    return finalLayout;
                }
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.ShowPastCompleteItemsOnFullCalendar):
                    case nameof(ViewModel.FirstDayOfWeek):
                    case nameof(ViewModel.DisplayState):
                    case nameof(ViewModel.SelectedDate):
                        MarkDirty();
                        break;
                }
            }

            private VxComponent RenderDay(DateTime date)
            {
                return new CalendarDayComponent
                {
                    Month = Month,
                    Items = Items,
                    ViewModel = ViewModel,
                    Date = date,
                    IsFullSize = IsFullSize,
                    IsSelected = IsSplit && ViewModel.SelectedDate == date,
                    ShowPastCompleteItemsOnFullCalendar = ViewModel.ShowPastCompleteItemsOnFullCalendar
                };
            }
        }

        private class CalendarDayComponent : VxComponent
        {
            public DateTime Month { get; set; }
            public MyObservableList<BaseViewItemMegaItem> Items { get; set; }

            // Note that subsequent properties on the ViewModel are NOT observed
            public CalendarViewModel ViewModel { get; set; }
            public DateTime Date { get; set; }
            public bool IsFullSize { get; set; }
            public bool IsSelected { get; set; }
            public bool ShowPastCompleteItemsOnFullCalendar { get; set; }

            public override bool SubscribeToIsMouseOver => true;

            protected override View Render()
            {
                return RenderDay(Date);
            }

            private View _addButtonRef;

            private static Color TodayColor => Theme.Current.IsDarkTheme ? Color.FromArgb(56, 56, 56) : Color.FromArgb(117, 117, 117);
            private static Color OtherMonthColor => Theme.Current.IsDarkTheme ? Color.FromArgb(30, 30, 30) : Color.FromArgb(228, 228, 228);

            public const int CompactRightMargin = 8;
            public const int CompactLeftMargin = 10;
            public const int FullSizeLeftRightMargin = 10;

            private View RenderDay(DateTime date)
            {
                bool isToday = date == ViewModel.Today;
                DayType dayType;

                if (DateTools.SameMonth(date, Month))
                {
                    dayType = DayType.ThisMonth;
                }
                else if (date < Month)
                {
                    dayType = DayType.PrevMonth;
                }
                else
                {
                    dayType = DayType.NextMonth;
                }

                Color foregroundColor = isToday ? Color.White : Theme.Current.SubtleForegroundColor;

                var itemsOnDay = TasksOrEventsOnDay.Get(ViewModel.MainScreenViewModel.CurrentAccount, Items, date, ViewModel.Today, activeOnly: !IsFullSize || !ShowPastCompleteItemsOnFullCalendar);
                SubscribeToCollectionStrong(itemsOnDay, nameof(itemsOnDay));
                var holidays = HolidaysOnDay.Create(Items, date);
                SubscribeToCollectionStrong(holidays, nameof(holidays));

                var tbDay = new TextBlock
                {
                    Text = date.Day.ToString(),
                    Margin = IsFullSize ? new Thickness(FullSizeLeftRightMargin, 6, FullSizeLeftRightMargin, 6) : new Thickness(CompactLeftMargin, 4, CompactRightMargin, 0),
                    FontSize = IsFullSize ? Theme.Current.SubtitleFontSize : Theme.Current.BodyFontSize,
                    FontWeight = FontWeights.SemiLight,
                    TextColor = foregroundColor,
                    VerticalAlignment = IsFullSize ? VerticalAlignment.Center : VerticalAlignment.Top,
                    TextAlignment = IsFullSize ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                    WrapText = false
                };

                var dayBackgroundColor = isToday ? TodayColor : dayType == DayType.ThisMonth ? CalendarColor : OtherMonthColor;

                if (holidays.Any())
                {
                    dayBackgroundColor = dayBackgroundColor.Overlay(Color.Red, 0.3);
                }

                var linearLayout = new LinearLayout
                {
                    Tapped = () =>
                    {
                        if (ViewModel.DisplayState == CalendarViewModel.DisplayStates.Split)
                        {
                            ViewModel.SelectedDate = date;
                        }
                        else
                        {
                            ViewModel.OpenDay(date);
                        }
                    },
                    Orientation = Orientation.Vertical
                };

                if (!IsFullSize)
                {
                    var itemCircles = new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(4,0,4,4)
                    };

                    foreach (var item in itemsOnDay.OfType<ViewItemTaskOrEvent>().Where(i => !i.IsComplete))
                    {
                        itemCircles.Children.Add(new Border
                        {
                            Width = 4,
                            Height = 4,
                            CornerRadius = 4,
                            BackgroundColor = item.Class.Color.ToColor(),
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Margin = new Thickness(0, 0, 4, 0)
                        });
                    }

                    linearLayout.Children.Add(tbDay);
                    linearLayout.Children.Add(itemCircles.LinearLayoutWeight(1));
                }
                else
                {
                    linearLayout.Children.Add(new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            tbDay.LinearLayoutWeight(holidays.Any() ? 0 : 1),

                            holidays.Any() ? new TextBlock
                            {
                                Text = holidays.First().Name,
                                WrapText = false,
                                FontSize = 10,
                                TextColor = tbDay.TextColor,
                                VerticalAlignment = VerticalAlignment.Center
                            }.LinearLayoutWeight(1) : null,

                            new TransparentContentButton
                            {
                                AltText = PowerPlannerResources.GetString("Calendar_FullCalendarAddButton.ToolTipService.ToolTip"),
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Add,
                                    Color = foregroundColor,
                                    FontSize = tbDay.FontSize,
                                    Margin = new Thickness(6),
                                    Opacity = IsMouseOver ? 1 : 0
                                },
                                ViewRef = v => _addButtonRef = v,
                                Click = () => new ContextMenu
                                {
                                    Items =
                                    {
                                        new ContextMenuItem
                                        {
                                            Text = PowerPlannerResources.GetString("String_Task"),
                                            Click = () => ViewModel.AddTask(date)
                                        },
                                        new ContextMenuItem
                                        {
                                            Text = PowerPlannerResources.GetString("String_Event"),
                                            Click = () => ViewModel.AddEvent(date)
                                        },
                                        new ContextMenuItem
                                        {
                                            Text = PowerPlannerResources.GetString("String_Holiday"),
                                            Click = () => ViewModel.AddHoliday(date)
                                        }
                                    }
                                }.Show(_addButtonRef)
                            }
                        }
                    });

                    foreach (var item in itemsOnDay.OfType<ViewItemTaskOrEvent>())
                    {
                        linearLayout.Children.Add(RenderDayItem(item));
                    }

                    linearLayout.AllowDrop = true;
                    linearLayout.DragOver = e =>
                    {
                        if (e.Data.Properties.TryGetValue("ViewItem", out object o) && o is ViewItemTaskOrEvent draggedTaskOrEvent)
                        {
                            bool duplicate = (e.Modifiers & DragDropModifiers.Control) != 0;  // Duplicate if holding Ctrl key

                        if (duplicate)
                            {
                                e.AcceptedOperation = DataPackageOperation.Copy;
                            }
                            else if (draggedTaskOrEvent.EffectiveDateForDisplayInDateBasedGroups.Date != this.Date.Date)
                            {
                                e.AcceptedOperation = DataPackageOperation.Move;
                            }
                            else
                            {
                                e.AcceptedOperation = DataPackageOperation.None;
                            }
                        }
                    };
                    linearLayout.Drop = e =>
                    {
                        try
                        {
                            if (e.Data.Properties.TryGetValue("ViewItem", out object o) && o is ViewItemTaskOrEvent draggedTaskOrEvent)
                            {
                                bool duplicate = (e.Modifiers & DragDropModifiers.Control) != 0;  // Duplicate if holding Ctrl key

                            if (duplicate)
                                {
                                    PowerPlannerApp.Current.GetMainScreenViewModel()?.DuplicateTaskOrEvent(draggedTaskOrEvent, this.Date.Date);
                                }
                                else
                                {
                                    _ = ViewModel.MoveItem(draggedTaskOrEvent, this.Date.Date);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    };
                }

                return new Border
                {
                    BorderThickness = new Thickness(2),
                    BackgroundColor = dayBackgroundColor,
                    BorderColor = IsSelected ? Theme.Current.AccentColor : dayBackgroundColor,
                    Content = linearLayout
                };
            }

            private View RenderDayItem(ViewItemTaskOrEvent item)
            {
                return new MainCalendarItemComponent
                {
                    Item = item,
                    ViewModel = ViewModel,
                    ShowItem = ShowItem,
                    Margin = new Thickness(0, 0, 0, 1)
                };
            }

            private void ShowItem(ViewItemTaskOrEvent item)
            {
                ViewModel.ShowItem(item);
            }

            public enum DayType
            {
                ThisMonth,
                PrevMonth,
                NextMonth
            }
        }
    }
}
