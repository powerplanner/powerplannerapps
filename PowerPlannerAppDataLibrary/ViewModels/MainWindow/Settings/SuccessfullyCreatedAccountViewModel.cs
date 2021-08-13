using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SuccessfullyCreatedAccountViewModel : PopupComponentViewModel
    {
        public SuccessfullyCreatedAccountViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_SuccessfullyCreatedAccountPage.Title");
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_SuccessfullyCreatedAccountPage_Message.Text")
                },

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("TextBox_Username.Header"),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new TextBlock
                {
                    Text = Username
                },

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("CreateAccountPage_TextBoxEmail.Header"),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new TextBlock
                {
                    Text = Email
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_ButtonContinue.Content"),
                    Margin = new Thickness(0, 24, 0, 0),
                    Click = Continue
                }

            );
        }

        public string Username { get; set; }

        public string Email { get; set; }

        public void Continue()
        {
            RemoveViewModel();
        }
    }
}
