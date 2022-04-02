﻿using PowerPlannerAppDataLibrary.App;
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
    public class FullSizeCalendarComponent : VxComponent
    {
        [VxSubscribe]
        private CalendarViewModel _viewModel;

        private DateTime _thisMonth;
        private Func<int, View> _itemTemplate;
        private View _addButtonRef, _filterButtonRef;

        private static bool IntegratedTopControls = VxPlatform.Current == Platform.Uwp;

        public override bool SubscribeToIsMouseOver => IntegratedTopControls;

        public FullSizeCalendarComponent(CalendarViewModel viewModel)
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
                            Margin = new Thickness(0, 20, 0, 0),
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

                                CreateIconButton(
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
                                    }.Show(_filterButtonRef), v => _filterButtonRef = v),

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

            return new FullSizeCalendarMonthComponent
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

        private class FullSizeCalendarMonthComponent : VxComponent
        {
            public DateTime Month { get; set; }
            public MyObservableList<BaseViewItemMegaItem> Items { get; set; }

            // Note that subsequent properties on the ViewModel are NOT observed
            public CalendarViewModel ViewModel { get; set; }

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
                    dayHeaders.Children.Add(new TextBlock
                    {
                        Text = DateTools.ToLocalizedString(ViewModel.FirstDayOfWeek + i),
                        WrapText = false,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        FontSize = 16,
                        Margin = new Thickness(12, 6, 12, 6),
                        VerticalAlignment = VerticalAlignment.Center
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
                        FontSize = Theme.Current.HeaderFontSize,
                        FontWeight = FontWeights.SemiLight,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        Margin = new Thickness(60, 10, 12, 6),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }

                var finalLayout = new LinearLayout
                {
                    BackgroundColor = Theme.Current.BackgroundAlt2Color,
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
                        MarkDirty();
                        break;
                }
            }

            private VxComponent RenderDay(DateTime date)
            {
                return new FullSizeCalendarDayComponent
                {
                    Month = Month,
                    Items = Items,
                    ViewModel = ViewModel,
                    Date = date
                };
            }
        }

        private class FullSizeCalendarDayComponent : VxComponent
        {
            public DateTime Month { get; set; }
            public MyObservableList<BaseViewItemMegaItem> Items { get; set; }

            // Note that subsequent properties on the ViewModel are NOT observed
            public CalendarViewModel ViewModel { get; set; }
            public DateTime Date { get; set; }

            public override bool SubscribeToIsMouseOver => true;

            protected override View Render()
            {
                return RenderDay(Date);
            }

            private View _addButtonRef;

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

                var itemsOnDay = TasksOrEventsOnDay.Get(ViewModel.MainScreenViewModel.CurrentAccount, Items, date, ViewModel.Today, activeOnly: !ViewModel.ShowPastCompleteItemsOnFullCalendar);
                var holidays = HolidaysOnDay.Create(Items, date);

                var tbDay = new TextBlock
                {
                    Text = date.Day.ToString(),
                    Margin = new Thickness(10,6,10,6),
                    FontSize = Theme.Current.SubtitleFontSize,
                    FontWeight = FontWeights.SemiLight,
                    TextColor = isToday ? Theme.Current.ForegroundColor.Invert() : Theme.Current.SubtleForegroundColor,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var dayBackgroundColor = isToday ? Theme.Current.SubtleForegroundColor : dayType == DayType.ThisMonth ? Color.Transparent : Theme.Current.BackgroundAlt1Color;

                if (holidays.Any())
                {
                    dayBackgroundColor = dayBackgroundColor.Overlay(Color.Red, 0.3);
                }

                var linearLayout = new LinearLayout
                {
                    BackgroundColor = dayBackgroundColor,
                    Tapped = () => ViewModel.OpenDay(date),
                    Children =
                    {
                        new LinearLayout
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
                                        Color = tbDay.TextColor,
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
                        }
                    }
                };

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

                return linearLayout;
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
