using PowerPlannerApp.App;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerPlannerApp
{
    public partial class AppShell : Application
    {
        public AppShell()
        {
            InitializeComponent();

            MainPage = new ContentPage()
            {
                Content = new PowerPlannerVxApp()
            };
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
