using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day
{
    public class SingleDayComponentLiveProps : BindableBase
    {
        private Action _onExpand;
        public Action OnExpand
        {
            get => _onExpand;
            set => SetProperty(ref _onExpand, value, nameof(OnExpand));
        }

        private bool _includeHeader = true;
        public bool IncludeHeader
        {
            get => _includeHeader;
            set => SetProperty(ref _includeHeader, value, nameof(IncludeHeader));
        }

        private bool _includeAdd = false;
        public bool IncludeAdd
        {
            get => _includeAdd;
            set => SetProperty(ref _includeAdd, value, nameof(IncludeAdd));
        }
    }

    public class SingleDayComponent : VxComponent
    {
        public DateTime Date { get; set; }
        public DateTime Today { get; set; } = DateTime.Today;
        public SemesterItemsViewGroup SemesterItemsViewGroup { get; set; }
        public ICalendarOrDayViewModel ViewModel { get; set; }

        [VxSubscribe]
        public SingleDayComponentLiveProps LiveProps { get; set; }

        private static object SCHEDULE_SNAPSHOT = new object();

        private SemesterItemsViewGroup.DayWithScheduleSnapshot _dayWithScheduleSnapshot;

        protected override View Render()
        {
            _dayWithScheduleSnapshot = SemesterItemsViewGroup.GetDayWithScheduleSnapshot(Date);
            var IncludeHeader = LiveProps.IncludeHeader;

            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    IncludeHeader ? Divider() : null,

                    IncludeHeader ? RenderHeader() : null,

                    IncludeHeader ? Divider() : null,

                    new ListView
                    {
                        Items = _dayWithScheduleSnapshot.Items,
                        ItemTemplate = RenderItem,
                        Padding = new Thickness(0, 0, 0, 12)
                    }.LinearLayoutWeight(1)
                }
            }.AllowDropTaskOrEvent(Date);
        }

        private View _addRef;

        private View RenderHeader()
        {
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                Children =
                {
                    new TextBlock
                    {
                        Text = GetHeaderText(Date),
                        FontSize = Theme.Current.SubtitleFontSize,
                        Margin = new Thickness(Theme.Current.PageMargin, 6, 6, 6),
                        TextColor = Theme.Current.SubtleForegroundColor,
                        WrapText = false
                    }.LinearLayoutWeight(1),

                    LiveProps.IncludeAdd ? new TransparentContentButton
                    {
                        Content = new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.Add,
                            FontSize = Theme.Current.TitleFontSize,
                            Margin = new Thickness(6),
                            Color = Theme.Current.SubtleForegroundColor
                        },
                        AltText = PowerPlannerResources.GetString("Calendar_FullCalendarAddButton.ToolTipService.ToolTip"),
                        ViewRef = v => _addRef = v,
                        Click = () => new ContextMenu
                        {
                            Items =
                            {
                                new MenuItem
                                {
                                    Text = PowerPlannerResources.GetString("String_Task"),
                                    Click = () => ViewModel.AddTask(Date)
                                },
                                new MenuItem
                                {
                                    Text = PowerPlannerResources.GetString("String_Event"),
                                    Click = () => ViewModel.AddEvent(Date)
                                },
                                new MenuItem
                                {
                                    Text = PowerPlannerResources.GetString("String_Holiday"),
                                    Click = () => ViewModel.AddHoliday(Date)
                                }
                            }
                        }.Show(_addRef),
                        Margin = new Thickness(0, 0, Theme.Current.PageMargin, 0)
                    } : null,

                    LiveProps.OnExpand != null ? new TransparentContentButton
                    {
                        Content = new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.ExpandLess,
                            FontSize = Theme.Current.TitleFontSize,
                            Margin = new Thickness(6, 6, Theme.Current.PageMargin, 6),
                            Color = Theme.Current.SubtleForegroundColor
                        },
                        AltText = "Show full day",
                        Click = LiveProps.OnExpand
                    } : null
                }
            };
        }

        private View RenderItem(object objItem)
        {
            if (objItem is SemesterItemsViewGroup.DayWithScheduleSnapshot.Spacing)
            {
                return new Border
                {
                    Height = 9
                };
            }
            if (objItem is string nothingDueStr)
            {
                return new TextBlock
                {
                    Text = nothingDueStr,
                    TextColor = Theme.Current.SubtleForegroundColor,
                    Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, 6)
                };
            }
            if (objItem is ViewItemTaskOrEvent taskOrEvent)
            {
                return TaskOrEventListItemComponent.Render(taskOrEvent, ViewModel as BaseMainScreenViewModelDescendant, IncludeDate: false, AllowDrag: true);
            }
            else if (objItem is DayScheduleItemsArranger arranger)
            {
                return new DayScheduleSnapshotComponent
                {
                    ArrangedItems = arranger,
                    Margin = new Thickness(0, 6, 0, 0)
                };
            }
            else if (objItem is ViewItemHoliday holiday)
            {
                return new LinearLayout
                {
                    Tapped = () => ViewModel.MainScreenViewModel.ViewHoliday(holiday),
                    Children =
                    {
                        new Border
                        {
                            BackgroundColor = Color.FromArgb(255, 228, 0, 137),
                            Content = new TextBlock
                            {
                                Text = holiday.Name,
                                Margin = new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, 12),
                                WrapText = false,
                                TextColor = Color.White
                            }
                        },

                        TaskOrEventListItemComponent.Divider()
                    }
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private View Divider()
        {
            return TaskOrEventListItemComponent.Divider();
        }

        private string GetHeaderText(DateTime date)
        {
            if (date.Date == Today)
                return PowerPlannerResources.GetRelativeDateToday().ToUpper();

            else if (date.Date == Today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow().ToUpper();

            else if (date.Date == Today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday().ToUpper();

            return PowerPlannerAppDataLibrary.Helpers.DateHelpers.ToMediumDateString(date).ToUpper();
        }
    }
}
