using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using System.Threading.Tasks;
using Vx.Droid;
using Vx.Views;

namespace VxSampleApp.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            VxDroidNative.Initialize(this);

            var loginView = new LoginView();
            loginView.SetOnTopNativeViewChanged(newNativeView =>
            {
                SetContentView((View)newNativeView);
            });
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
            return new VxTextBlock("Hello " + Username.Value);

            //return new VxStackPanel()
            //    .Children(
            //        new VxTextBox()
            //            .Header("Username")
            //            .Text(Username),

            //        UsernameError.Value != null ? new VxTextBlock(UsernameError.Value) : null,

            //        new VxTextBox()
            //            .Header("Password")
            //            .Text(Password),

            //        new VxButton(SigningIn.Value ? "Logging in..." : "Log in")
            //            .Click(Login)
            //    );

            //var content = new VxStackPanel()
            //    .Children(
            //        new VxTextBox()
            //            .Header("Username")
            //            .Error(UsernameError.Value)
            //            .Text(Username),

            //        new VxTextBox()
            //            .Header("Password")
            //            .Text(Password)
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

                SigningIn.Value = !SigningIn.Value;
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

            SigningIn.Value = false;
        }
    }
}