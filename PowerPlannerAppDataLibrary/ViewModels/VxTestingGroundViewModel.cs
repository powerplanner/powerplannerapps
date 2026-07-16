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
            //return RenderComplexYearsLayout();
            return RenderLayoutGallery();
        }

        /// <summary>
        /// A gallery of layout primitives and combinations used to verify the cross-platform
        /// layout engine (especially the iOS manual LinearLayout implementation). Add new
        /// scenarios here as we find layout bugs so we can reproduce them in isolation.
        /// </summary>
        private View RenderLayoutGallery()
        {
            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                BackgroundColor = Color.White,
                Children =
                {
                    // Toolbar at the top + weighted scrolling content (the calendar/classes pattern)
                    new Toolbar
                    {
                        Title = "Vx Layout Tests"
                    },

                    new ScrollView
                    {
                        Content = new LinearLayout
                        {
                            Orientation = Orientation.Vertical,
                            Children =
                            {
                                Section("1. Vertical: auto top, weighted fill, auto bottom",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Vertical,
                                        Children =
                                        {
                                            Box("Top (auto height)", Color.LightBlue),
                                            Box("Weighted fill (weight 1)", Color.LightGreen).LinearLayoutWeight(1),
                                            Box("Bottom (auto height)", Color.LightBlue)
                                        }
                                    }, 170),

                                Section("2. Horizontal: fixed-width sidebar + weighted content",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            Box("80px", Color.LightPink, width: 80),
                                            Box("weight 1", Color.LightGreen).LinearLayoutWeight(1)
                                        }
                                    }, 100),

                                Section("3. Horizontal: weights 1 : 2",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            Box("weight 1", Color.Khaki).LinearLayoutWeight(1),
                                            Box("weight 2", Color.LightSalmon).LinearLayoutWeight(2)
                                        }
                                    }, 100),

                                Section("4. Vertical: horizontal alignment Left / Center / Right / Stretch",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Vertical,
                                        Children =
                                        {
                                            Box("Left", Color.LightBlue, h: HorizontalAlignment.Left),
                                            Box("Center", Color.LightGreen, h: HorizontalAlignment.Center),
                                            Box("Right", Color.LightPink, h: HorizontalAlignment.Right),
                                            Box("Stretch", Color.Khaki, h: HorizontalAlignment.Stretch)
                                        }
                                    }, 200),

                                Section("5. Horizontal: vertical alignment Top / Center / Bottom / Stretch",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            Box("Top", Color.LightBlue, width: 70, v: VerticalAlignment.Top),
                                            Box("Center", Color.LightGreen, width: 70, v: VerticalAlignment.Center),
                                            Box("Bottom", Color.LightPink, width: 70, v: VerticalAlignment.Bottom),
                                            Box("Stretch", Color.Khaki, width: 70, v: VerticalAlignment.Stretch)
                                        }
                                    }, 140),

                                Section("6. Nested: horizontal of two top-aligned vertical stacks",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            new LinearLayout
                                            {
                                                Orientation = Orientation.Vertical,
                                                VerticalAlignment = VerticalAlignment.Top,
                                                Children =
                                                {
                                                    Box("A1", Color.LightBlue),
                                                    Box("A2", Color.LightBlue),
                                                    Box("A3", Color.LightBlue)
                                                }
                                            }.LinearLayoutWeight(1),

                                            Box("spacer", Color.White, width: 12),

                                            new LinearLayout
                                            {
                                                Orientation = Orientation.Vertical,
                                                VerticalAlignment = VerticalAlignment.Top,
                                                Children =
                                                {
                                                    Box("B1", Color.LightGreen)
                                                }
                                            }.LinearLayoutWeight(1)
                                        }
                                    }, 180),

                                Section("7. Border padding around content",
                                    new Border
                                    {
                                        BackgroundColor = Color.LightSteelBlue,
                                        Padding = new Thickness(20),
                                        Content = Box("Padded 20 all around", Color.White)
                                    }, 120),

                                Section("8. Wrapping text inside a weighted fill",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Vertical,
                                        Children =
                                        {
                                            new TextBlock
                                            {
                                                Text = "This is a long wrapping paragraph of text used to verify that width-constrained labels compute the correct height when laid out by the manual layout engine, and that the surrounding layout reacts to the measured height correctly.",
                                                WrapText = true,
                                                Margin = new Thickness(8)
                                            },
                                            Box("Below the text", Color.LightGreen)
                                        }
                                    }, 200),

                                Section("9. Margins between horizontal boxes",
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            Box("m8", Color.LightBlue, margin: 8).LinearLayoutWeight(1),
                                            Box("m8", Color.LightGreen, margin: 8).LinearLayoutWeight(1),
                                            Box("m8", Color.LightPink, margin: 8).LinearLayoutWeight(1)
                                        }
                                    }, 100)
                            }
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }

        private View Section(string title, View demo, float height)
        {
            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(12, 12, 12, 0),
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontWeight = FontWeights.Bold,
                        WrapText = true,
                        Margin = new Thickness(0, 0, 0, 4)
                    },

                    new FrameLayout
                    {
                        Height = height,
                        BackgroundColor = Color.FromArgb(255, 235, 235, 235),
                        Children = { demo }
                    }
                }
            };
        }

        private View Box(string text, Color color, float? width = null, float? height = null, float margin = 2, VerticalAlignment v = VerticalAlignment.Stretch, HorizontalAlignment h = HorizontalAlignment.Stretch)
        {
            var border = new Border
            {
                BackgroundColor = color,
                Margin = new Thickness(margin),
                VerticalAlignment = v,
                HorizontalAlignment = h,
                Content = new TextBlock
                {
                    Text = text,
                    Margin = new Thickness(6),
                    WrapText = false,
                    TextAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            if (width.HasValue)
            {
                border.Width = width.Value;
            }

            if (height.HasValue)
            {
                border.Height = height.Value;
            }

            return border;
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