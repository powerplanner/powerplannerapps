using System;
using System.Collections.Generic;
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
                    new Button
                    {
                        Text = "Times clicked",
                        Click = () => _page.Value = "timesClicked"
                    },

                    new Button
                    {
                        Text = "Adding",
                        Click = () => _page.Value = "adding",
                        Margin = new Thickness(0, 12, 0, 0)
                    },

                    new Button
                    {
                        Text = "Text boxes",
                        Click = () => _page.Value = "textBoxes",
                        Margin = new Thickness(0, 12, 0, 0)
                    }
                }
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

                    subpage
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
