using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;

namespace PowerPlannerAndroid.Views
{
    public class WelcomeView : InterfacesDroid.Views.PopupViewHost<WelcomeViewModel>, AndroidX.Core.View.IOnApplyWindowInsetsListener
    {
        public WelcomeView(ViewGroup root) : base(Resource.Layout.Welcome, root)
        {
        }

        protected override async void OnViewCreated()
        {
            FindViewById<TextView>(Resource.Id.WelcomePage_TextViewSubtitle).Text = R.S("WelcomePage_TextBlockSubtitle.Text");

            FindViewById<Button>(Resource.Id.ButtonLogin).Text = R.S("WelcomePage_ButtonLogin.Content");
            FindViewById<Button>(Resource.Id.ButtonLogin).Click += ButtonLogin_Click;
            FindViewById<Button>(Resource.Id.ButtonCreateAccount).Text = R.S("WelcomePage_ButtonCreateAccount.Content");
            FindViewById<Button>(Resource.Id.ButtonCreateAccount).Click += ButtonCreateAccount_Click;

            ViewCompat.SetOnApplyWindowInsetsListener(this, this);

            // For some reason we need this for the welcome view
            await System.Threading.Tasks.Task.Delay(1);
            ViewCompat.RequestApplyInsets(this);
        }

        private void ButtonCreateAccount_Click(object sender, EventArgs e)
        {
            ViewModel.CreateAccount();
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            ViewModel.Login();
        }

        public virtual WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat windowInsets)
        {
            var insets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());

            var bottomInsets = FindViewById(Resource.Id.BottomInsets);
            var lp = bottomInsets.LayoutParameters;
            lp.Height = insets.Bottom;
            bottomInsets.LayoutParameters = lp;

            // Return CONSUMED if you don't want want the window insets to keep being
            // passed down to descendant views.
            return WindowInsetsCompat.Consumed;
        }
    }
}