using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class RecoveredUsernamesViewModel : PopupComponentViewModel
    {
        public RecoveredUsernamesViewModel(BaseViewModel parent, string[] usernames) : base(parent)
        {
            Title = PowerPlannerResources.GetString("LoginPage_TextBlockForgotUsername.Text");
            Usernames = usernames;

            var loginViewModel = parent.GetPopupViewModelHost()?.Popups.OfType<LoginViewModel>().FirstOrDefault();
            if (loginViewModel != null && usernames.Length > 0)
            {
                loginViewModel.Username = usernames[0];
            }
        }

        public string[] Usernames { get; private set; }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("ForgotUsername_String_YourUsernames"),
                            TextColor = Theme.Current.SubtleForegroundColor,
                            WrapText = true
                        },

                        new TextBlock
                        {
                            Text = string.Join("\n", Usernames),
                            WrapText = true,
                            Margin = new Thickness(0, 6, 0, 0)
                        }
                    }
                }
            };
        }
    }
}
