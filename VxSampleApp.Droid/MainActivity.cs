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

            var loginView = new WelcomeView();
            loginView.SetOnTopNativeViewChanged(newNativeView =>
            {
                (newNativeView as View).LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                SetContentView((View)newNativeView);
            });
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class WelcomeView : VxComponent
    {
        protected override VxView Render()
        {
            return new VxGrid()
                .RowDefinitions(
                    new VxRowDefinition(VxGridLength.Star(1)),
                    new VxRowDefinition(VxGridLength.Auto)
                )
                .Children(
                    new VxStackPanel()
                        .GridRow(0)
                        .VerticalAlignment(VxVerticalAlignment.Center)
                        .Children(
                            new VxTextBlock("Power Planner")
                                .HorizontalAlignment(VxHorizontalAlignment.Center)
                                .Margin(0, 0, 0, 12),
                            new VxTextBlock("The ultimate homework planner")
                                .HorizontalAlignment(VxHorizontalAlignment.Center)
                        ),

                    new VxGrid()
                        .GridRow(1)
                        .ColumnDefinitions(
                            new VxColumnDefinition(VxGridLength.Star(1)),
                            new VxColumnDefinition(VxGridLength.Star(1))
                        )
                        .Children(
                            new VxButton("Log in")
                                .HorizontalAlignment(VxHorizontalAlignment.Stretch)
                                .Margin(12), // The margins make it go wacky for some reason
                            new VxButton("Create account")
                                .HorizontalAlignment(VxHorizontalAlignment.Stretch)
                                .GridColumn(1)
                                .Margin(12)
                        )
                );
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
            //Updater();
        }

        protected override VxView Render()
        {
            //return new VxTextBlock("Hello " + Username.Value);

            return new VxGrid()
                .RowDefinitions(
                    new VxRowDefinition(VxGridLength.Star(1)),
                    new VxRowDefinition(VxGridLength.Auto)
                )
                .Children(
                    new VxTextBlock("Power Planner"),
                    new VxGrid()
                        .GridRow(1)
                        .ColumnDefinitions(
                            new VxColumnDefinition(VxGridLength.Star(1)),
                            new VxColumnDefinition(VxGridLength.Star(1))
                        )
                        .Children(
                            new VxTextBlock("Log in"),
                            new VxTextBlock("Create account")
                                .GridColumn(1)
                        )
                );

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