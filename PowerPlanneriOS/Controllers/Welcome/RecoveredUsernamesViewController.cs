using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers.Welcome
{
    public class RecoveredUsernamesViewController : PopupViewControllerWithScrolling<RecoveredUsernamesViewModel>
    {
        public RecoveredUsernamesViewController()
        {
            Title = "Username";
        }

        public override void OnViewModelLoadedOverride()
        {
            if (ViewModel.Usernames.Length == 0)
            {
                // Shouldn't happen
                return;
            }

            AddSpacing(16);
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Your username is: " + ViewModel.Usernames.First()
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);

            if (ViewModel.Usernames.Length > 1)
            {
                AddSpacing(16);

                var additionalHeader = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Additional usernames...",
                    Font = UIFont.PreferredCaption1.Bold()
                };
                StackView.AddArrangedSubview(additionalHeader);
                additionalHeader.StretchWidth(StackView, left: 16, right: 16);

                foreach (var username in ViewModel.Usernames.Skip(1))
                {
                    var additionalUsername = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Text = username,
                        Font = UIFont.PreferredCaption1
                    };
                    StackView.AddArrangedSubview(additionalUsername);
                    additionalUsername.StretchWidth(StackView, left: 16, right: 16);
                }
            }

            base.OnViewModelLoadedOverride();
        }
    }
}