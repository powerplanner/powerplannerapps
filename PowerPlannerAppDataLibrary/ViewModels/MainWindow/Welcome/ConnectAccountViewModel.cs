using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class ConnectAccountViewModel : PopupComponentViewModel
    {
        public ConnectAccountViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Welcome_ConnectAccountPage.Title");
        }

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
                            Text = PowerPlannerResources.GetString("Welcome_ConnectAccountPage_Message.Text"),
                            WrapText = true
                        },

                        new AccentButton
                        {
                            Text = PowerPlannerResources.GetString("WelcomePage_ButtonLogin.Content"),
                            Click = LogIn,
                            Margin = new Thickness(0, 24, 0, 0)
                        },

                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("Welcome_ConnectAccountPage_NeedHelp.Text"),
                            WrapText = true,
                            FontSize = Theme.Current.CaptionFontSize,
                            Margin = new Thickness(0, 12, 0, 0)
                        }
                    }
                }
            };
        }

        public void LogIn()
        {
            ShowPopup(new LoginViewModel(this));
        }
    }
}
