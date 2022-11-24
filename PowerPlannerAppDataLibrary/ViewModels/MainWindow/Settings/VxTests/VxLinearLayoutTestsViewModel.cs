using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Helpers;
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
                                    WrapText = false
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
                                    VerticalAlignment = VerticalAlignment.Stretch,
                                    WrapText = false // iOS needs non-wrapping when in horizontal auto-width layout
                                },

                                new TextBlock
                                {
                                    Text = "Top",
                                    VerticalAlignment = VerticalAlignment.Top,
                                    WrapText = false
                                },

                                new TextBlock
                                {
                                    Text = "Center",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    WrapText = false
                                },

                                new TextBlock
                                {
                                    Text = "Bottom",
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    WrapText = false
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
                                    Margin = new Thickness(6),
                                    WrapText = false // iOS needs non-wrapping when in horizontal auto-width layout
                                },

                                new TextBlock
                                {
                                    Text = "Top",
                                    VerticalAlignment = VerticalAlignment.Top,
                                    Margin = new Thickness(6),
                                    WrapText = false
                                },

                                new TextBlock
                                {
                                    Text = "Center",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(6),
                                    WrapText = false
                                },

                                new TextBlock
                                {
                                    Text = "Bottom",
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Margin = new Thickness(6),
                                    WrapText = false
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
                                                        Text = "+",
                                                        WrapText = false
                                                    }
                                                }
                                            },

                                            new LinearLayout
                                            {
                                                BackgroundColor = Color.Blue,
                                                Orientation = Orientation.Horizontal,
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
                                                    }.LinearLayoutWeight(1)
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
                                                        Text = "+",
                                                        WrapText = false
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
                        },

                        new TextBlock
                        {
                            Text = "Vertical completion bar (65%)",
                            Margin = new Thickness(Theme.Current.PageMargin)
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            BackgroundColor = Color.BurlyWood,
                            Children =
                            {
                                new LinearLayout
                                {
                                    BackgroundColor = Theme.Current.SubtleForegroundColor.Opacity(0.3),
                                    Width = 8,
                                    Children =
                                    {
                                        new Border().LinearLayoutWeight(0.175f),

                                        new Border
                                        {
                                            BackgroundColor = Color.Red
                                        }.LinearLayoutWeight(0.65f),

                                        new Border().LinearLayoutWeight(0.175f)
                                    }
                                },

                                new LinearLayout
                                {
                                    Margin = new Thickness(6, 3, 0, 3),
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = "Item 1"
                                        },

                                        new TextBlock
                                        {
                                            Text = "Caption",
                                            TextColor = Theme.Current.SubtleForegroundColor
                                        }
                                    }
                                }.LinearLayoutWeight(1)
                            }
                        },

                        new TextBlock
                        {
                            Text = "Default grade scales (text boxes should stretch width)",
                            Margin = new Thickness(Theme.Current.PageMargin)
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(Theme.Current.PageMargin),
                            Children =
                            {
                                new NumberTextBox
                                {
                                    Margin = new Thickness(0, 0, 6, 0),
                                }.LinearLayoutWeight(1),

                                new NumberTextBox
                                {
                                    Margin = new Thickness(6, 0, 0, 0)
                                }.LinearLayoutWeight(1),

                                new TransparentContentButton
                                {
                                    Content = new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Close,
                                        FontSize = 20,
                                        Color = System.Drawing.Color.Red
                                    }
                                }
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
