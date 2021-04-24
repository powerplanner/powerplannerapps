using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace VxSampleApp
{
    public class VxCombinedComponent : VxComponent
    {
        private VxState<int> _timesClicked = new VxState<int>(0);
        private VxState<string[]> _items = new VxState<string[]>(new string[0]);
        private VxState<string> _page = new VxState<string>("home");

        protected override View Render()
        {
            switch (_page.Value)
            {
                case "timesClicked":
                    return RenderSubpage(RenderTimesClicked());

                case "adding":
                    return RenderSubpage(RenderAdding());

                case "textBoxes":
                    return RenderSubpage(RenderTextBoxes());

                case "margins":
                    return RenderSubpage(RenderMargins());

                case "verticalStretched":
                    return RenderSubpage(RenderVerticalStretchedLayout());

                case "fontIcons":
                    return RenderSubpage(RenderFontIcons());

                case "scrollView":
                    return RenderSubpage(RenderScrollView());

                case "horizontalWeights":
                    return RenderSubpage(RenderHorizontalWeights());

                default:
                    return RenderHome();
            }
        }

        private View RenderHome()
        {
            return new LinearLayout
            {
                Children =
                {
                    RenderButton("Times clicked", "timesClicked"),
                    RenderButton("Adding", "adding"),
                    RenderButton("Text boxes", "textBoxes"),
                    RenderButton("Margins", "margins"),
                    RenderButton("Vertical stretched", "verticalStretched"),
                    RenderButton("Font icons", "fontIcons"),
                    RenderButton("Scroll view", "scrollView"),
                    RenderButton("Horizontal weights", "horizontalWeights")
                }
            };
        }

        private Button RenderButton(string text, string page)
        {
            return new Button
            {
                Text = text,
                Click = () => _page.Value = page,
                Margin = new Thickness(0, 12, 0, 0)
            };
        }

        private View RenderSubpage(View subpage)
        {
            return new LinearLayout
            {
                Children =
                {
                    new Button
                    {
                        Text = "Back home",
                        Click = () => _page.Value = "home",
                        Margin = new Thickness(0, 0, 0, 12)
                    },

                    subpage.LinearLayoutWeight(1)
                }
            };
        }

        private View RenderTimesClicked()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Clicked {_timesClicked.Value} times"
                    },

                    new Button
                    {
                        Text = "Click me",
                        Click = delegate { _timesClicked.Value++; },
                        Margin = new Thickness(0, 12, 0, 0)
                    }
                }
            };
        }

        private View RenderAdding()
        {
            var sl = new LinearLayout();

            foreach (var item in _items.Value)
            {
                sl.Children.Add(new TextBlock { Text = item });
            }

            sl.Children.Add(new Button
            {
                Text = "Add item",
                Click = () => _items.Value = _items.Value.Concat(new string[] { "Item " + (_items.Value.Length + 1) }).ToArray(),
                Margin = new Thickness(0, 12, 0, 0)
            });

            return sl;
        }

        private View RenderMargins()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "This item has 12px margin below",
                        Margin = new Thickness(0, 0, 0, 12)
                    },

                    new TextBlock
                    {
                        Text = "Left margin 12px",
                        Margin = new Thickness(12, 0, 0, 0)
                    },

                    new TextBlock
                    {
                        Text = "No margins"
                    },

                    new TextBlock
                    {
                        Text = "12px margin above",
                        Margin = new Thickness(0, 12, 0, 0)
                    }
                }
            };
        }

        private View RenderVerticalStretchedLayout()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "Weight = 1"
                    }.LinearLayoutWeight(1),

                    new TextBlock
                    {
                        Text = "Weight = 1"
                    }.LinearLayoutWeight(1),

                    new TextBlock
                    {
                        Text = "Weight = 2"
                    }.LinearLayoutWeight(2),
                }
            };
        }

        private VxState<string> _username = new VxState<string>("");
        private VxState<string> _email = new VxState<string>("");

        private View RenderTextBoxes()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBox
                    {
                        Header = "Username",
                        Text = _username
                    },

                    new TextBlock
                    {
                        Text = GetUsernameStatusText()
                    },

                    new TextBox
                    {
                        Header = "Email",
                        Text = _email
                    },

                    new TextBlock
                    {
                        Text = "Your email: " + _email.Value
                    }
                }
            };
        }

        private View RenderFontIcons()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "Save icon"
                    },

                    new FontIcon
                    {
                        Glyph = MaterialDesign.MaterialDesignIcons.Save,
                        FontSize = 20,
                        Color = Color.Blue
                    }
                }
            };
        }

        private View RenderScrollView()
        {
            return new ScrollView
            {
                Content = new TextBlock
                {
                    Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed aliquet urna in tempor molestie. Aliquam bibendum quam diam. Cras quis feugiat magna. Mauris pellentesque ex vitae nulla euismod, a pretium neque auctor. Fusce at enim varius, blandit sapien ac, viverra diam. Proin ultrices a ligula eu congue. Nulla quis tellus tristique, lobortis augue non, rhoncus ipsum. Aenean consequat accumsan libero et luctus. Donec dignissim mollis risus a semper. Morbi maximus elit vel urna maximus rhoncus. Suspendisse consequat ullamcorper urna a auctor. In sed tortor justo. Vivamus consectetur eu leo in volutpat. Praesent accumsan, ipsum nec pharetra vulputate, nunc arcu accumsan elit, non luctus dui diam a felis. Phasellus iaculis arcu aliquam cursus elementum. Vivamus lobortis urna id mollis consequat.\n\nNunc sed lectus sollicitudin nisl rutrum dapibus.Aliquam quis sapien magna.In posuere luctus enim,\n\nNunc sed lectus sollicitudin nisl rutrum dapibus. Aliquam quis sapien magna. In posuere luctus enim, ac dapibus orci fringilla id. Sed lacinia lectus eget condimentum venenatis. Ut tincidunt pretium turpis, eget porta diam tristique nec. Sed lacinia efficitur justo. Duis gravida gravida velit, semper dapibus erat vestibulum sit amet. Aliquam erat volutpat. Morbi consectetur odio convallis, tempor orci ac, ultrices ipsum. Curabitur maximus faucibus mauris ac aliquam. Fusce facilisis, felis a dignissim pretium, est libero tincidunt erat, ut tincidunt urna elit et enim. In ultricies leo ante. Nunc ac sagittis ex, a congue purus. In in dui a orci mollis dignissim et eget ipsum. Maecenas id libero in tellus scelerisque pellentesque.\n\nNulla porta sodales luctus. Sed dapibus tellus sit amet nibh sagittis, pretium rhoncus turpis vestibulum. Nunc eros ipsum, elementum a libero vel, ullamcorper scelerisque dui. Donec fringilla molestie ipsum. Quisque a diam odio. Proin a magna urna. Phasellus ultricies condimentum finibus. Cras egestas eget felis in lobortis. Nam vel gravida eros. Donec dictum, erat a facilisis efficitur, nulla ante viverra sem, non tincidunt eros orci a enim. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Fusce porttitor gravida risus, at volutpat ante cursus pulvinar.",
                    Margin = new Thickness(12),
                    WrapText = true
                }
            };
        }

        private View RenderHorizontalWeights()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "Split 50/50, no margin between"
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBox().LinearLayoutWeight(1),
                            new TextBox().LinearLayoutWeight(1)
                        }
                    },

                    new TextBlock
                    {
                        Text = "Split 50/50, 12px margin between"
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBox()
                            {
                                Margin = new Thickness(0, 0, 6, 0)
                            }.LinearLayoutWeight(1),

                            new TextBox()
                            {
                                Margin = new Thickness(6, 0, 0, 0)
                            }.LinearLayoutWeight(1)
                        }
                    },

                    new TextBlock
                    {
                        Text = "Split 50/50 and then auto, 12px margin between"
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBox()
                            {
                                Margin = new Thickness(0, 0, 6, 0)
                            }.LinearLayoutWeight(1),

                            new TextBox()
                            {
                                Margin = new Thickness(6, 0, 6, 0)
                            }.LinearLayoutWeight(1),

                            new TextBlock()
                            {
                                Text = "Close",
                                Margin = new Thickness(6, 0, 0, 0)
                            }
                        }
                    },

                    new TextBlock
                    {
                        Text = "Auto and then split 50/50, 12px margin between",
                        Margin = new Thickness(0, 12, 0, 0)
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock()
                            {
                                Text = "Icon",
                                Margin = new Thickness(0, 0, 6, 0)
                            },

                            new TextBox()
                            {
                                Margin = new Thickness(6, 0, 6, 0)
                            }.LinearLayoutWeight(1),

                            new TextBox()
                            {
                                Margin = new Thickness(0, 0, 6, 0)
                            }.LinearLayoutWeight(1)
                        }
                    },

                    new TextBlock
                    {
                        Text = "Auto and then 100%, 12px margin between",
                        Margin = new Thickness(0, 12, 0, 0)
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock()
                            {
                                Text = "Icon",
                                Margin = new Thickness(0, 0, 6, 0)
                            },

                            new TextBox()
                            {
                                Margin = new Thickness(6, 0, 0, 0)
                            }.LinearLayoutWeight(1)
                        }
                    },

                    new TextBlock
                    {
                        Text = "Auto and then 100% text, 12px margin between",
                        Margin = new Thickness(0, 12, 0, 0)
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock()
                            {
                                Text = "Icon",
                                Margin = new Thickness(0, 0, 6, 0)
                            },

                            new TextBlock()
                            {
                                Text = "100% text Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed aliquet urna in tempor molestie. Aliquam bibendum quam diam. Cras quis feugiat magna.",
                                Margin = new Thickness(6, 0, 0, 0),
                                WrapText = true
                            }.LinearLayoutWeight(1)
                        }
                    },

                    new TextBlock
                    {
                        Text = "Auto and then stretch nested vertical, 12px margin between",
                        Margin = new Thickness(0, 12, 0, 0)
                    },

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock()
                            {
                                Text = "Icon",
                                Margin = new Thickness(0, 0, 6, 0)
                            },

                            new LinearLayout()
                            {
                                Margin = new Thickness(6, 0, 0, 0),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = "Hello", FontWeight = FontWeights.Bold
                                    },
                                    new TextBlock { Text = "World" }
                                }
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };
        }

        private string GetUsernameStatusText()
        {
            if (_username.Value.Length == 0)
            {
                return "You must enter a username";
            }

            if (_username.Value.Length < 5)
            {
                return "Username must be at least 5 characters";
            }

            if (_username.Value.Contains("@"))
            {
                return "Username cannot contain @";
            }

            return "Looking good!";
        }
    }
}
