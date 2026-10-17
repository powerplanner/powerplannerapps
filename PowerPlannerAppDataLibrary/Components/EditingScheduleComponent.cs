using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public partial class EditingScheduleComponent : VxComponent
    {
        public ScheduleViewModel ViewModel { get; set; }

        public Thickness NookInsets { get; set; }

        protected override View Render()
        {
            if (ViewModel == null)
            {
                return null;
            }

            var classes = ViewModel.SemesterViewItemsGroup.Classes;
            SubscribeToCollection(classes);

            var layout = new LinearLayout
            {
                Margin = new Thickness(
                    Theme.Current.PageMargin + NookInsets.Left - Theme.Current.SmallPageMargin,
                    Theme.Current.PageMargin - Theme.Current.SmallPageMargin,
                    Theme.Current.PageMargin + NookInsets.Right - Theme.Current.SmallPageMargin,
                    Theme.Current.PageMargin + NookInsets.Bottom - Theme.Current.SmallPageMargin)
            };

            foreach (var c in classes)
            {
                layout.Children.Add(RenderClass(c));
            }

            layout.Children.Add(new AccentButton
            {
                Text = PowerPlannerResources.GetString("SchedulePage_ButtonAddClass.Content"),
                Click = ViewModel.AddClass
            });

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundColor,
                CornerRadius = Theme.Current.CornerRadius,
                Content = new ScrollView(layout)
            };

            return new ScrollView(layout);
        }

        private View RenderClass(ViewItemClass c)
        {
            Subscribe(c);
            SubscribeToCollection(c.Schedules);

            const float CIRCLE_SIZE = 14;

            var content = new LinearLayout
            {
                Children =
                {
                    new TransparentContentButton
                    {
                        Click = () => ViewModel.EditClass(c),
                        Content = new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(8),
                            Children =
                            {
                                new Border
                                {
                                    BackgroundColor = c.Color.ToColor(),
                                    Width = CIRCLE_SIZE,
                                    Height = CIRCLE_SIZE,
                                    CornerRadius = CIRCLE_SIZE / 2f,
                                    VerticalAlignment = VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = c.Name,
                                    FontSize = Theme.Current.SubtitleFontSize,
                                    WrapText = false,
                                    Margin = new Thickness(8, 0, 0, 1),
                                    VerticalAlignment = VerticalAlignment.Center
                                }
                            }
                        }
                    }
                }
            };

            foreach (var group in c.GetSchedulesGroupedBySharedEditingValues())
            {
                content.Children.Add(RenderTimes(group));
            }

            content.Children.Add(new Button
            {
                Text = PowerPlannerResources.GetString("String_AddTime"),
                Margin = new Thickness(8, 8, 8, 8),
                Click = () => ViewModel.AddTime(c, true)
            });

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                CornerRadius = Theme.Current.CornerRadius,
                Margin = new Thickness(0, 0, 0, 16),
                Content = content
            };
        }

        private View RenderTimes(ViewItemSchedule[] schedules)
        {
            var first = schedules.First();

            var layout = new LinearLayout
            {
                Margin = new Thickness(8, 4, 8, 4),
                Children =
                {
                    new TextBlock
                    {
                        Text = string.Join(", ", schedules.Select(i => i.DayOfWeek).Distinct().OrderBy(i => i).Select(i => DateTools.ToLocalizedString(i))),
                        FontWeight = FontWeights.SemiBold
                    },

                    // Editing view, so we use School Time
                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetStringTimeToTime(
                            DateTimeFormatterExtension.Current.FormatAsShortTime(first.StartTimeInSchoolTime),
                            DateTimeFormatterExtension.Current.FormatAsShortTime(first.EndTimeInSchoolTime))
                    }.CaptionStyle()
                }
            };

            if (!string.IsNullOrWhiteSpace(first.Room))
            {
                layout.Children.Add(new TextBlock
                {
                    Text = first.Room
                }.CaptionStyle());
            }

            if (first.ScheduleWeek != PowerPlannerSending.Schedule.Week.BothWeeks)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = PowerPlannerResources.GetLocalizedWeek(first.ScheduleWeek)
                }.CaptionStyle());
            }

            return new TransparentContentButton
            {
                Content = layout,
                Click = () => ViewModel.EditTimes(schedules, true)
            };
        }
    }
}
