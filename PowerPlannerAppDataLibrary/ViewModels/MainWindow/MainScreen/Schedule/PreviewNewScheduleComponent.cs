using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Extensions;
using Vx.Views;
using static PowerPlannerAppDataLibrary.Services.AiService;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    internal class PreviewNewScheduleComponent : VxComponent
    {
        public GenerateClassesResponse GenerateClassesResponse { get; set; }

        public Thickness NookInsets { get; set; }

        public Action GoBack { get; set; }

        private VxState<ProposedClass[]> _proposedClasses = new VxState<ProposedClass[]>();

        protected override void Initialize()
        {
            _proposedClasses.Value = GenerateClassesResponse.Classes.ToArray();
        }

        protected override View Render()
        {
            if (GenerateClassesResponse == null)
            {
                return null;
            }

            var classes = RenderClasses();
            classes.Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin);

            return new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = RenderClasses()
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Children =
                        {
                            new Button
                            {
                                Text = "Back",
                                Click = GoBack
                            },
                            new AccentButton
                            {
                                Text = "Save changes",
                                Margin = new Thickness(12, 0, 0, 0)
                                
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };
        }

        private View RenderClasses()
        {
            var gridPanel = new AdaptiveGridPanel()
            {
                MinColumnWidth = 290,
                ColumnSpacing = 12,
                Margin = new Thickness(Theme.Current.PageMargin).Combine(NookInsets)
            };

            foreach (var c in GenerateClassesResponse.Classes)
            {
                gridPanel.Children.Add(RenderClass(c));
            }

            return new LinearLayout
            {
                Children =
                {
                    gridPanel,

                    new Button
                    {
                        Height = 48,
                        Text = "+ Add class",
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 0, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin)
                    }
                }
            };
        }

        private View RenderClass(ProposedClass c)
        {
            // VERTICAL LAYOUT
            var verticalLayout = new LinearLayout
            {
                BackgroundColor = Theme.Current.BackgroundColor,
                Margin = new Thickness(0, 12, 0, 0),
                Children =
                {
                    // TOP ROW - Color, name, edit button, delete button
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            // COLOR, NAME, EDIT BUTTON
                            new TransparentContentButton
                            {
                                Content = new LinearLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Margin = new Thickness(12, 6, 6, 6),
                                    Children =
                                    {
                                        // COLOR
                                        new Border
                                        {
                                            BackgroundColor = ColorBytesHelper.ToColor(c.Color),
                                            Width = 24,
                                            Height = 24,
                                            VerticalAlignment = VerticalAlignment.Center
                                        },

                                        // NAME
                                        new TextBlock
                                        {
                                            Text = c.Name,
                                            WrapText = false,
                                            FontSize = 16,
                                            FontWeight = FontWeights.Bold,
                                            VerticalAlignment = VerticalAlignment.Center,
                                            Margin = new Thickness(6, 0, 0, 0)
                                        }.LinearLayoutWeight(1),

                                        // EDIT ICON
                                        new FontIcon
                                        {
                                            Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                                            FontSize = 20,
                                            VerticalAlignment = VerticalAlignment.Center,
                                            Margin = new Thickness(6, 0, 0, 0)
                                        }
                                    }
                                }
                            }.LinearLayoutWeight(1),

                            // DELETE BUTTON
                            new TransparentContentButton
                            {
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                    FontSize = 20,
                                    Margin = new Thickness(6, 6, 12, 6)
                                }
                            }
                        }
                    }
                }
            };

            // RENDER SCHEDULES
            foreach (var s in c.Schedules)
            {
                verticalLayout.Children.Add(RenderSchedulePreview(s));
            }

            // ADD SCHEDULE BUTTON
            verticalLayout.Children.Add(new Button
            {
                Text = "+ Add time",
                Margin = new Thickness(12, 0, 12, 12)
            });

            return verticalLayout;
        }

        private View RenderSchedule(ProposedSchedule s)
        {
            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                Margin = new Thickness(6, 6, 6, 0),
                Padding = new Thickness(6),
                Content = new LinearLayout
                {
                    Children =
                    {
                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TimePicker
                                {
                                    Header = "Start time",
                                    Value = VxValue.Create(s.StartTime, v =>
                                    {
                                        s.StartTime = v;
                                        MarkDirty();
                                    })
                                }
                            }
                        }
                    }
                }
            };
        }

        private string[] _availableScheduleWeekStrings;
        /// <summary>
        /// The localized strings
        /// </summary>
        public string[] AvailableScheduleWeekStrings
        {
            get
            {
                if (_availableScheduleWeekStrings == null)
                {
                    _availableScheduleWeekStrings = new string[]
                    {
                            PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.BothWeeks),
                            PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.WeekOne),
                            PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.WeekTwo)
                    };
                }

                return _availableScheduleWeekStrings;
            }
        }

        private string LocalizedScheduleWeekString(PowerPlannerSending.Schedule.Week week)
        {
            switch (week)
            {
                case PowerPlannerSending.Schedule.Week.BothWeeks:
                    return AvailableScheduleWeekStrings[0];

                case PowerPlannerSending.Schedule.Week.WeekOne:
                    return AvailableScheduleWeekStrings[1];

                case PowerPlannerSending.Schedule.Week.WeekTwo:
                    return AvailableScheduleWeekStrings[2];

                default:
                    throw new NotImplementedException();
            }
        }

        private View RenderSchedulePreview(ProposedSchedule s)
        {
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    // DETAILS AND EDIT
                    new TransparentContentButton
                    {
                        Content = new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(12, 12, 0, 12),
                            Children =
                            {
                                // DAYS, TIME, WEEK, ROOM
                                new LinearLayout
                                {
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = string.Join(", ", s.DayOfWeeks.Distinct().OrderBy(i => i).Select(i => DateTools.ToLocalizedString(i)))
                                        },

                                        new TextBlock
                                        {
                                            Text = PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(s.StartTime)), DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(s.EndTime))),
                                            FontSize = Theme.Current.CaptionFontSize
                                        },

                                        s.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks ? null : new TextBlock
                                        {
                                            Text = LocalizedScheduleWeekString(s.ScheduleWeek)
                                        }.CaptionStyle(),

                                        string.IsNullOrWhiteSpace(s.Room) ? null : new TextBlock
                                        {
                                            Text = s.Room
                                        }.CaptionStyle()
                                    }
                                }.LinearLayoutWeight(1),

                                // EDIT ICON
                                new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                                    FontSize = 20,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(6, 0, 6, 0)
                                }
                            }
                        }
                    }.LinearLayoutWeight(1),

                    // DELETE BUTTON
                    new TransparentContentButton
                    {
                        Content = new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                            Margin = new Thickness(6, 6, 12, 6),
                            FontSize = 20
                        }
                    }
                }
            };
        }

        private class PreviewClass
        {
            public string Name { get; set; }
        }
    }
}
