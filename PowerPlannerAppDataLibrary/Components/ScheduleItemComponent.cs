using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class ScheduleItemComponent : VxComponent
    {
        public const float WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly float SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly float SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 16;

        public DayScheduleItemsArranger.ScheduleItem ScheduleItem { get; set; }

        private ViewItemSchedule s => ScheduleItem.Item;
        private ViewItemClass c => s.Class;

        private static Thickness TextMargin = new Thickness(6, 0, 6, 0);

        protected override View Render()
        {
            bool canFitThreeLines = ScheduleItem.Height >= 60;
            bool canFitTwoLines = ScheduleItem.Height >= 40;

            TextBlock tbRoom = null;
            if (!string.IsNullOrWhiteSpace(s.Room))
            {
                tbRoom = Text(s.Room);
            }

            bool multiColumn = !canFitThreeLines && tbRoom != null;

            var layout = new LinearLayout
            {
                BackgroundColor = c.Color.ToColor(),
                Height = (float)ScheduleItem.Height,
                Children =
                {
                    Text(c.Name)
                }
            };

            Func<DateTime, string> formatter = date =>
            {
                return multiColumn ? DateTimeFormatterExtension.Current.FormatAsShortTimeWithoutAmPm(date) : DateTimeFormatterExtension.Current.FormatAsShortTime(date);
            };

            var tbTime = Text(PowerPlannerResources.GetStringTimeToTime(formatter(s.StartTimeInLocalTime(ScheduleItem.Date)), formatter(s.EndTimeInLocalTime(ScheduleItem.Date))));

            

            if (canFitThreeLines)
            {
                layout.Children.Add(tbTime);
                layout.Children.Add(tbRoom);
            }
            else if (canFitTwoLines)
            {
                if (tbRoom == null)
                {
                    layout.Children.Add(tbTime);
                }
                else
                {
                    tbTime.Margin = new Thickness();
                    tbRoom.Margin = new Thickness();

                    layout.Children.Add(new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = TextMargin,
                        Children =
                        {
                            tbTime.LinearLayoutWeight(1),
                            tbRoom
                        }
                    });
                }
            }

            return layout;
        }

        private TextBlock Text(string txt)
        {
            return new TextBlock
            {
                Text = txt,
                FontWeight = FontWeights.SemiBold,
                TextColor = Color.White,
                Margin = TextMargin,
                WrapText = false,
                FontSize = 14
            };
        }
    }
}
