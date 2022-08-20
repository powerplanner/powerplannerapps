using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.VxTests
{
    public class VxLinearLayoutTestsViewModel : PopupComponentViewModel
    {
        public VxLinearLayoutTestsViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(NookInsets.Left, 0, NookInsets.Right, NookInsets.Bottom),
                    Children =
                    {
                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            BackgroundColor = Color.Blue,
                            Children =
                            {
                                new Border
                                {
                                    BackgroundColor = Color.White,
                                    Height = 4,
                                    VerticalAlignment = VerticalAlignment.Center
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = "What If? (6px margins)",
                                    TextColor = Color.White,
                                    FontSize = Theme.Current.TitleFontSize,
                                    Margin = new Thickness(6),
                                },

                                new Border
                                {
                                    BackgroundColor = Color.White,
                                    Height = 4,
                                    VerticalAlignment = VerticalAlignment.Center
                                }.LinearLayoutWeight(1)
                            }
                        },

                        new TextBlock
                        {
                            Text = "Vertical text alignments in horizontal layout...",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Height = 50,
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "Stretch",
                                    VerticalAlignment = VerticalAlignment.Stretch
                                },

                                new TextBlock
                                {
                                    Text = "Top",
                                    VerticalAlignment = VerticalAlignment.Top
                                },

                                new TextBlock
                                {
                                    Text = "Center",
                                    VerticalAlignment = VerticalAlignment.Center
                                },

                                new TextBlock
                                {
                                    Text = "Bottom",
                                    VerticalAlignment = VerticalAlignment.Bottom
                                }
                            }
                        },

                        new TextBlock
                        {
                            Text = "Vertical button alignments in horizontal layout...",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Height = 60,
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new AccentButton
                                {
                                    Text = "Stretch",
                                    VerticalAlignment = VerticalAlignment.Stretch
                                },

                                new AccentButton
                                {
                                    Text = "Top",
                                    VerticalAlignment = VerticalAlignment.Top
                                },

                                new AccentButton
                                {
                                    Text = "Center",
                                    VerticalAlignment = VerticalAlignment.Center
                                },

                                new AccentButton
                                {
                                    Text = "Bottom",
                                    VerticalAlignment = VerticalAlignment.Bottom
                                }
                            }
                        },

                        new TextBlock
                        {
                            Text = "Vertical text alignments with 6px margins in horizontal layout...",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Height = 50,
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "Stretch",
                                    VerticalAlignment = VerticalAlignment.Stretch,
                                    Margin = new Thickness(6)
                                },

                                new TextBlock
                                {
                                    Text = "Top",
                                    VerticalAlignment = VerticalAlignment.Top,
                                    Margin = new Thickness(6)
                                },

                                new TextBlock
                                {
                                    Text = "Center",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(6)
                                },

                                new TextBlock
                                {
                                    Text = "Bottom",
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Margin = new Thickness(6)
                                }
                            }
                        },

                        new TextBlock
                        {
                            Text = "Vertical border alignments with 6px margins in horizontal layout...",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Height = 50,
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new Border
                                {
                                    BackgroundColor = Color.Blue,
                                    Width = 30,
                                    VerticalAlignment = VerticalAlignment.Stretch,
                                    Margin = new Thickness(6)
                                },

                                new Border
                                {
                                    BackgroundColor = Color.Blue,
                                    Height = 6,
                                    Width = 30,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    Margin = new Thickness(6)
                                },

                                new Border
                                {
                                    BackgroundColor = Color.Blue,
                                    Height = 6,
                                    Width = 30,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(6)
                                },

                                new Border
                                {
                                    BackgroundColor = Color.Blue,
                                    Height = 6,
                                    Width = 30,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Margin = new Thickness(6)
                                }
                            }
                        },

                        new TextBlock
                        {
                            Text = "Horizontal with nested vertical (settings list)",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.AccountCircle,
                                    FontSize = 40
                                },

                                new LinearLayout
                                {
                                    Margin = new Thickness(6, 0, 0, 0),
                                    BackgroundColor = Color.Beige,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = "My Account",
                                            FontWeight = FontWeights.Bold,
                                            WrapText = false
                                        },

                                        new TextBlock
                                        {
                                            Text = "View my account",
                                            TextColor = Theme.Current.SubtleForegroundColor,
                                            WrapText = false
                                        }
                                    }
                                }.LinearLayoutWeight(1)
                            }
                        },

                        new TextBlock
                        {
                            Text = "Calendar with nested vertical stretching (background should stretch to bottom)",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Orientation = Orientation.Horizontal,
                            Height = 80,
                            Children =
                            {
                                new LinearLayout
                                {
                                    BackgroundColor = Color.Beige,
                                    Margin = new Thickness(6),
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = "One"
                                        },
                                        new TextBlock
                                        {
                                            Text = "Two"
                                        },
                                        new TextBlock
                                        {
                                            Text = "Three"
                                        }
                                    }
                                }.LinearLayoutWeight(1),

                                new LinearLayout
                                {
                                    BackgroundColor = Color.Beige,
                                    Margin = new Thickness(6),
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = "One"
                                        }
                                    }
                                }.LinearLayoutWeight(1)
                            }
                        },

                        new TextBlock
                        {
                            Text = "Truly complicated calendar (border with the linear layouts)",
                            Margin = new Thickness(6)
                        },

                        new LinearLayout
                        {
                            BackgroundColor = Color.BurlyWood,
                            Orientation = Orientation.Horizontal,
                            Height = 100,
                            Children =
                            {
                                // Day 1
                                new Border
                                {
                                    BorderThickness = new Thickness(1),
                                    BorderColor = Color.Black,
                                    Content = new LinearLayout
                                    {
                                        BackgroundColor = Color.Beige,
                                        Margin = new Thickness(6),
                                        Children =
                                        {
                                            new LinearLayout
                                            {
                                                Orientation = Orientation.Horizontal,
                                                Children =
                                                {
                                                    new TextBlock
                                                    {
                                                        Text = "26",
                                                        FontSize = 30
                                                    }.LinearLayoutWeight(1),

                                                    new TextBlock
                                                    {
                                                        Text = "+"
                                                    }
                                                }
                                            },

                                            new LinearLayout
                                            {
                                                BackgroundColor = Color.Blue,
                                                Children =
                                                {
                                                    new Border
                                                    {
                                                        Width = 8,
                                                        BackgroundColor = Color.DarkBlue
                                                    },

                                                    new TextBlock
                                                    {
                                                        Text = "One item",
                                                        TextColor = Color.White
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }.LinearLayoutWeight(1),

                                // Day 2
                                new Border
                                {
                                    BorderThickness = new Thickness(1),
                                    BorderColor = Color.Black,
                                    Content = new LinearLayout
                                    {
                                        BackgroundColor = Color.Beige,
                                        Margin = new Thickness(6),
                                        Children =
                                        {
                                            new LinearLayout
                                            {
                                                Orientation = Orientation.Horizontal,
                                                Children =
                                                {
                                                    new TextBlock
                                                    {
                                                        Text = "26",
                                                        FontSize = 30
                                                    }.LinearLayoutWeight(1),

                                                    new TextBlock
                                                    {
                                                        Text = "+"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }.LinearLayoutWeight(1),
                            }
                        },

                        new TextBlock
                        {
                            Text = "Fixed size items (small calendar circles, should be on bottom)",
                            Margin = new Thickness(Theme.Current.PageMargin)
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            BackgroundColor = Color.BurlyWood,
                            Height = 20,
                            Children =
                            {
                                CalendarCircle(Color.Red),
                                CalendarCircle(Color.Blue),
                                CalendarCircle(Color.Green)
                            }
                        }
                    }
                }
            };
        }

        private View CalendarCircle(Color color)
        {
            const float size = 4;
            return new Border
            {
                Width = size,
                Height = size,
                CornerRadius = size,
                BackgroundColor = color,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, size, 0)
            };
        }
    }
}
