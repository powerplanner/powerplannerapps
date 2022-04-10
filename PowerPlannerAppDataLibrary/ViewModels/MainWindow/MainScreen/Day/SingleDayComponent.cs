using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
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

        protected override View Render()
        {
            var itemsOnDay = TasksOrEventsOnDay.Get(ViewModel.MainScreenViewModel.CurrentAccount, SemesterItemsViewGroup.Items, Date, Today);

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
                        Items = itemsOnDay,
                        ItemTemplate = RenderItem,
                        ItemClicked = item =>
                        {
                            ViewModel.MainScreenViewModel.ShowItem(item as ViewItemTaskOrEvent);
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }

        private View RenderItem(object objItem)
        {
            return new TaskOrEventListItemComponent
            {
                Item = objItem as ViewItemTaskOrEvent,
                ViewModel = ViewModel,
                IncludeDate = false
            };
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
