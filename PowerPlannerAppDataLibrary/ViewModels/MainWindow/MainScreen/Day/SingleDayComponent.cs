using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day
{
    public class SingleDayComponent : VxComponent
    {
        public DateTime Date { get; set; }
        public DateTime Today { get; set; } = DateTime.Today;
        public SemesterItemsViewGroup SemesterItemsViewGroup { get; set; }
        public BaseMainScreenViewModelDescendant ViewModel { get; set; }

        private static object SCHEDULE_SNAPSHOT = new object();

        private SemesterItemsViewGroup.DayWithScheduleSnapshot _dayWithScheduleSnapshot;

        protected override View Render()
        {
            _dayWithScheduleSnapshot = SemesterItemsViewGroup.GetDayWithScheduleSnapshot(Date);

            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    Divider(),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Border
                            {
                                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                                Content = new TextBlock
                                {
                                    Text = GetHeaderText(Date),
                                    FontSize = Theme.Current.SubtitleFontSize,
                                    Margin = new Thickness(12,6,6,6),
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }
                            }.LinearLayoutWeight(1),

                            //new Border
                            //{
                            //    BackgroundColor = Theme.Current.BackgroundAlt2Color,
                            //    Content = new FontIcon
                            //    {
                            //        Glyph = MaterialDesign.MaterialDesignIcons.Add,
                            //        FontSize = Theme.Current.TitleFontSize,
                            //        Margin = new Thickness(6),
                            //        Color = Theme.Current.SubtleForegroundColor
                            //    },
                            //    Tapped = () =>
                            //    {
                            //        // TODO
                            //    }
                            //}
                        }
                    },

                    Divider(),

                    new ListView
                    {
                        Items = _dayWithScheduleSnapshot.Items,
                        ItemTemplate = RenderItem,
                        ItemClicked = item =>
                        {
                            if (item is ViewItemTaskOrEvent taskOrEvent)
                            {
                                ViewModel.MainScreenViewModel.ShowItem(taskOrEvent);
                            }
                            else if (item is ViewItemHoliday holiday)
                            {
                                ViewModel.MainScreenViewModel.ViewHoliday(holiday);
                            }
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }

        private View RenderItem(object objItem)
        {
            if (objItem is ViewItemTaskOrEvent taskOrEvent)
            {
                return new TaskOrEventListItemComponent
                {
                    Item = taskOrEvent,
                    ViewModel = ViewModel,
                    IncludeDate = false
                };
            }
            else if (objItem is DayScheduleItemsArranger arranger)
            {
                return new DayScheduleSnapshotComponent
                {
                    ArrangedItems = arranger
                };
            }
            else if (objItem is ViewItemHoliday holiday)
            {
                return new LinearLayout
                {
                    Children =
                    {
                        new Border
                        {
                            BackgroundColor = Color.FromArgb(255, 228, 0, 137),
                            Content = new TextBlock
                            {
                                Text = holiday.Name,
                                Margin = new Thickness(12),
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
