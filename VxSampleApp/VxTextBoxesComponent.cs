using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace VxSampleApp
{
    public class VxTextBoxesComponent : VxComponent
    {
        private VxState<string> _username = new VxState<string>();
        private VxState<string> _email = new VxState<string>();

        protected override View Render()
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
