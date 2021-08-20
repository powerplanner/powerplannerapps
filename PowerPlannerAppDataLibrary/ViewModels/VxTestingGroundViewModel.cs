#if DEBUG
using System;
using System.Drawing;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewItems;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels
{
    public class VxTestingGroundViewModel : ComponentViewModel
    {
        public static readonly bool ShowTestingGround = false;
        protected override bool InitialAllowLightDismissValue => false;

        public VxTestingGroundViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override View Render()
        {
            //return RenderHorizontalWithVerticals();
            return RenderComplexYearsLayout();
        }

        private View RenderHorizontalWithVerticals()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Horizontal rows with vertical items of different heights, with vertical alignment = top"
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.LightBlue,
                            Children =
                            {
                                new LinearLayout
                                {
                                    BackgroundColor = Color.LightPink,
                                    Orientation = Orientation.Horizontal,
                                    Children =
                                    {
                                        new LinearLayout
                                        {
                                            Margin = new Thickness(12),
                                            VerticalAlignment = VerticalAlignment.Top,
                                            BackgroundColor = Color.LightBlue,
                                            Children =
                                            {
                                                new TextBlock { Text = "Item 1" },
                                                new TextBlock { Text = "Item 2" },
                                                new TextBlock { Text = "Item 3" }
                                            }
                                        }.LinearLayoutWeight(1),

                                        new Border
                                        {
                                            Width = 24
                                        },

                                        new LinearLayout
                                        {
                                            Margin = new Thickness(12),
                                            VerticalAlignment = VerticalAlignment.Top,
                                            BackgroundColor = Color.LightBlue,
                                            Children =
                                            {
                                                new TextBlock { Text = "Item 1" },
                                            }
                                        }.LinearLayoutWeight(1)
                                    }
                                },

                                new LinearLayout
                                {
                                    BackgroundColor = Color.LightPink,
                                    Orientation = Orientation.Horizontal,
                                    Margin = new Thickness(0, 12, 0, 0),
                                    Children =
                                    {
                                        new LinearLayout
                                        {
                                            Margin = new Thickness(12),
                                            VerticalAlignment = VerticalAlignment.Top,
                                            BackgroundColor = Color.LightBlue,
                                            Children =
                                            {
                                                new TextBlock { Text = "Item 1" },
                                                new TextBlock { Text = "Item 2" }
                                            }
                                        }.LinearLayoutWeight(1),

                                        new Border
                                        {
                                            Width = 24
                                        },

                                        new LinearLayout
                                        {
                                            Margin = new Thickness(12),
                                            VerticalAlignment = VerticalAlignment.Top,
                                            BackgroundColor = Color.LightBlue,
                                            Children =
                                            {
                                                new TextBlock { Text = "Item 1" },
                                                new TextBlock { Text = "Item 2" },
                                                new TextBlock { Text = "Item 3" }
                                            }
                                        }.LinearLayoutWeight(1)
                                    }
                                }
                            }
                        },

                        new Button { Text = "Bottom button" }
                    }
                }
            };
        }

        private View RenderComplexYearsLayout()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = PowerPlannerResources.GetString("YearsPage_TextBlockOverall.Text"),
                                    Margin = new Thickness(0, 0, 12, 0),
                                    FontSize = Theme.Current.TitleFontSize,
                                    VerticalAlignment = VerticalAlignment.Center
                                },

                                new LinearLayout
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = "3.95 GPA",
                                            FontWeight = FontWeights.Bold
                                        },

                                        new TextBlock
                                        {
                                            Text = "15 credits"
                                        }
                                    }
                                }.LinearLayoutWeight(1)
                            }
                        },

                        new LinearLayout
                        {
                            Children =
                            {
                                new LinearLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Children =
                                    {
                                        RenderYear("Freshman").LinearLayoutWeight(1),

                                        new Border
                                        {
                                            Width = 24
                                        },

                                        RenderYear("Sophomore").LinearLayoutWeight(1)
                                    }
                                },

                                new LinearLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Children =
                                    {
                                        RenderYear("Junior").LinearLayoutWeight(1),

                                        new Border
                                        {
                                            Width = 24
                                        },

                                        RenderYear("Senior").LinearLayoutWeight(1)
                                    }
                                }
                            }
                        },

                        new Button
                        {
                            Text = "+ add year"
                        }
                    }
                }
            };
        }

        private View RenderYear(string name)
        {
            var linearLayout = new LinearLayout
            {
                Children =
                {
                    new TransparentContentButton
                    {
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(12),
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = name,
                                    FontSize = Theme.Current.TitleFontSize
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = GpaToStringConverter.Convert(3.9),
                                    FontSize = Theme.Current.TitleFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }
                            }
                        }
                    }
                }
            };

            for (int i = 0; i < 2; i++)
            {
                linearLayout.Children.Add(RenderSemester(i == 0 ? "Fall" : "Spring"));
            }

            linearLayout.Children.Add(new Button
            {
                Text = "+ add semester",
                Margin = new Thickness(12, 0, 12, 12)
            });

            return new Border
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundColor,
                Content = linearLayout,
                Margin = new Thickness(0, 24, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
        }

        private Random _random = new Random(523);
        private View RenderSemester(string name)
        {
            var linearLayout = new LinearLayout
            {
                Children =
                {
                    new TransparentContentButton
                    {
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(12),
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = name,
                                    FontSize = 22
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = "TODO date span",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    TextAlignment = HorizontalAlignment.Right
                                }
                            }
                        }
                    }
                }
            };

            // TODO: Localize
            linearLayout.Children.Add(RenderClassRow("Class", "Credits", "GPA", isSubtle: true));

            int classes = _random.Next(3, 9);
            for (int i = 0; i < classes; i++)
            {
                linearLayout.Children.Add(RenderClassRow("Class " + i, CreditsToStringConverter.Convert(3), GpaToStringConverter.Convert(3.8)));
            }

            // TODO: Localize
            linearLayout.Children.Add(RenderClassRow("Total", CreditsToStringConverter.Convert(15), GpaToStringConverter.Convert(3.85), isBig: true));

            linearLayout.Children.Add(new AccentButton
            {
                Text = "Open semester",
                Margin = new Thickness(12, 12, 12, 12)
            });

            return new Border
            {
                Margin = new Thickness(12, 0, 12, 12),
                BackgroundColor = Theme.Current.PopupPageBackgroundAltColor,
                Content = linearLayout
            };
        }

        private View RenderClassRow(string str1, string str2, string str3, bool isSubtle = false, bool isBig = false)
        {
            var textColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor;
            var fontSize = isBig ? 16 : Theme.Current.CaptionFontSize;

            return new LinearLayout
            {
                Margin = new Thickness(12, 0, 12, 0),
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = str1,
                        FontSize = fontSize,
                        TextColor = textColor,
                        WrapText = false,
                        FontWeight = FontWeights.SemiBold
                    }.LinearLayoutWeight(2),

                    new TextBlock
                    {
                        Text = str2,
                        FontSize = fontSize,
                        TextColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor,
                        WrapText = false,
                        TextAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.SemiBold
                    }.LinearLayoutWeight(1),

                    new TextBlock
                    {
                        Text = str3,
                        FontSize = fontSize,
                        TextColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor,
                        WrapText = false,
                        TextAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.SemiBold
                    }.LinearLayoutWeight(1)
                }
            };
        }
    }
}

#endif