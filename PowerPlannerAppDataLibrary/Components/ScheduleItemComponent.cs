using PowerPlannerAppDataLibrary.App;
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
    public partial class ScheduleItemComponent : VxComponent
    {
        public const float WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly float SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly float SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 16;

        public DayScheduleItemsArranger.ScheduleItem ScheduleItem { get; set; }

        private ViewItemSchedule s => ScheduleItem.Item;
        private ViewItemClass c => s.Class;
        private bool IsCancelled => ScheduleItem.IsCancelled;

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

            return new Border
            {
                BackgroundColor = IsCancelled ? c.Color.ToColor().Opacity(0.1) : c.Color.ToColor(),
                Height = (float)ScheduleItem.Height,
                CornerRadius = 6,
                BorderColor = IsCancelled ? c.Color.ToColor().Opacity(0.5) : Color.Transparent,
                BorderThickness = new Thickness(IsCancelled ? 1 : 0),
                Content = layout,
                ContextMenu = () => new ContextMenu()
                {
                    Items =
                    {
                        new MenuItem
                        {
                            Text = R.S(IsCancelled ? "String_RestoreThisClass" : "String_CancelThisClass"),
                            Glyph = IsCancelled ? MaterialDesign.MaterialDesignIcons.Restore : MaterialDesign.MaterialDesignIcons.Delete,
                            Click = delegate
                            {
                                if (IsCancelled)
                                {
                                    PowerPlannerApp.Current.GetMainScreenViewModel().RestoreScheduleOccurrence(s, DateOnly.FromDateTime(ScheduleItem.Date));
                                }
                                else
                                {
                                    PowerPlannerApp.Current.GetMainScreenViewModel().CancelScheduleOccurrence(s, DateOnly.FromDateTime(ScheduleItem.Date));
                                }
                            }
                        }
                    }
                }
            };
        }

        private TextBlock Text(string txt)
        {
            return new TextBlock
            {
                Text = txt,
                FontWeight = IsCancelled ? default : FontWeights.SemiBold,
                TextColor = IsCancelled ? Theme.Current.SubtleForegroundColor : Color.White,
                Strikethrough = IsCancelled,
                Margin = TextMargin,
                WrapText = false,
                FontSize = 14
            };
        }
    }
}
