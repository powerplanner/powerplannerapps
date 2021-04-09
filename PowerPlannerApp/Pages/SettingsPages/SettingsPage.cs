using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages.SettingsPages
{
    public class SettingsPage : VxPage
    {
        protected override View Render()
        {
            return new StackLayout
            {
                Children =
                {
                    new Label { Text = "My account" },
                    new MyAccountPage()
                }
            };
        }
    }
}
