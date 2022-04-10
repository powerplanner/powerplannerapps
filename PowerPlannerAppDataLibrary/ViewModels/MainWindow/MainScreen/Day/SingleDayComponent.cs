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
                                    FontSize = Theme.Current.TitleFontSize,
                                    Margin = new Thickness(6)
                                }
                            }.LinearLayoutWeight(1),

                            new Border
                            {
                                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Add,
                                    FontSize = Theme.Current.TitleFontSize,
                                    Margin = new Thickness(6)
                                },
                                Tapped = () =>
                                {
                                    // TODO
                                }
                            }
                        }
                    },

                    new ListView
                    {
                        Items = itemsOnDay,
                        ItemTemplate = RenderItem
                    }.LinearLayoutWeight(1)
                }
            };
        }

        private View RenderItem(object objItem)
        {
            var item = objItem as ViewItemTaskOrEvent;
            if (item == null)
            {
                return new TextBlock
                {
                    Text = "Not a task or event",
                    Margin = new Thickness(12)
                };
            }

            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        Width = 6,
                        BackgroundColor = item.Class.Color.ToColor()
                    },

                    new LinearLayout
                    {
                        Margin = new Thickness(6,0,0,0),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = item.Name,
                                FontWeight = FontWeights.SemiBold,
                                WrapText = false
                            },

                            new TextBlock
                            {
                                Text = item.Subtitle,
                                FontWeight = FontWeights.SemiBold,
                                TextColor = item.Class.Color.ToColor(),
                                WrapText = false
                            },

                            !string.IsNullOrWhiteSpace(item.Details) ? new TextBlock
                            {
                                Text = item.Details.Replace("\n", "  "),
                                WrapText = false,
                                TextColor = Theme.Current.SubtleForegroundColor
                            } : null
                        }
                    }.LinearLayoutWeight(1)
                }
            };
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
