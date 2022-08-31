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
        public bool IncludeHeader { get; set; } = true;

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
                    IncludeHeader ? Divider() : null,

                    IncludeHeader ? new ListHeaderComponent
                    {
                        Text = GetHeaderText(Date)
                    } : null,

                    IncludeHeader ? Divider() : null,

                    new ListView
                    {
                        Items = _dayWithScheduleSnapshot.Items,
                        ItemTemplate = RenderItem,
                        Padding = new Thickness(0, 0, 0, 12)
                    }.LinearLayoutWeight(1)
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
