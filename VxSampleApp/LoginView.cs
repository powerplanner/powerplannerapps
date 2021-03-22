using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace VxSampleApp
{
    public class LoginView : VxComponent
    {
        public VxState<string> Username { get; set; } = new VxState<string>("");
        public VxState<string> Password { get; set; } = new VxState<string>("");

        public VxState<string> UsernameError { get; set; } = new VxState<string>(null);

        public VxState<bool> SigningIn { get; set; } = new VxState<bool>(false);

        public override VxView Render()
        {
            var content = new VxStackPanel()
                .Children(
                    new VxTextBox()
                        .Header("Username")
                        .Error(UsernameError.Value)
                        .TextBinding(Username),

                    new VxTextBox()
                        .Header("Password")
                        .TextBinding(Password),

                    new VxButton("Login")
                        .Click(Login)
                );

            if (SigningIn.Value)
            {
                return new VxGrid()
                    .Children(
                        content,
                        new VxTextBlock("Logging in as " + Username.Value + "..."));
            }

            return content;
        }

        private async void Login()
        {
            if (Username.Value.Length == 0)
            {
                UsernameError.Value = "You must provide your username.";
                return;
            }

            SigningIn.Value = true;

            await Task.Delay(2000);

            if (true)
            {
                // Close popup, navigate
            }
            else
            {
                SigningIn.Value = false;
            }
        }
    }
}
