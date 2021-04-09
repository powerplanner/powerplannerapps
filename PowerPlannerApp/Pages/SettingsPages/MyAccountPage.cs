using PowerPlannerApp.App;
using PowerPlannerApp.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages.SettingsPages
{
    public class MyAccountPage : VxPage
    {
        protected override View Render()
        {
            return new Button
            {
                Text = "Log out",
                Command = CreateCommand(LogOut)
            };
        }

        private void LogOut()
        {
            AccountsManager.SetLastLoginIdentifier(Guid.Empty);
            _ = PowerPlannerVxApp.Current.SetCurrentAccount(null);
        }
    }
}
