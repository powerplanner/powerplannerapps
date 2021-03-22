using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vx.Uwp;
using Vx.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VxSampleApp.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            VxNative.Initialize();

            var loginView = new LoginView();
            loginView.SetOnTopNativeViewChanged(newNativeView =>
            {
                Content.Child = (UIElement)newNativeView;
            });
        }
    }

    public class LoginView : VxComponent
    {
        public VxState<string> Username { get; set; } = new VxState<string>("");
        public VxState<string> Password { get; set; } = new VxState<string>("");

        public VxState<string> UsernameError { get; set; } = new VxState<string>(null);

        public VxState<bool> SigningIn { get; set; } = new VxState<bool>(false);

        public LoginView()
        {
            Updater();
        }

        protected override VxView Render()
        {
            return new VxTextBlock()
            {
                Text = "Username: " + Username.Value
            };

            //var content = new VxStackPanel()
            //    .Children(
            //        new VxTextBox()
            //            .Header("Username")
            //            .Error(UsernameError.Value)
            //            .TextBinding(Username),

            //        new VxTextBox()
            //            .Header("Password")
            //            .TextBinding(Password),

            //        new VxButton("Login")
            //            .Click(Login)
            //    );

            //if (SigningIn.Value)
            //{
            //    return new VxGrid()
            //        .Children(
            //            content,
            //            new VxTextBlock("Logging in as " + Username.Value + "..."));
            //}

            //return content;
        }

        private async void Updater()
        {
            while (true)
            {
                await Task.Delay(1000);

                Username.Value += "a";
            }
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
