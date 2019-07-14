using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Views;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class DeleteAccountViewController : PopupViewControllerWithScrolling<DeleteAccountViewModel>
    {
        public DeleteAccountViewController()
        {
            Title = "Delete Account";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {

            AddTopSectionDivider();

            AddSpacing(8);
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Are you sure you want to delete your account?",
                Lines = 0
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);
            AddSpacing(8);

            AddDivider();

            var switchDeleteOnline = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Delete online account"
            };
            BindingHost.SetSwitchBinding(switchDeleteOnline, nameof(ViewModel.DeleteOnlineAccountToo));
            StackView.AddArrangedSubview(switchDeleteOnline);
            switchDeleteOnline.StretchWidth(StackView);
            switchDeleteOnline.SetHeight(44);

            AddSectionDivider();

            var buttonDelete = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TintColor = UIColor.Red
            };
            buttonDelete.TouchUpInside += new WeakEventHandler<EventArgs>(async delegate
            {
                try
                {
                    ShowLoadingOverlay();
                    await ViewModel.DeleteAsync();
                }
                catch { }

                HideLoadingOverlay();
            }).Handler;
            buttonDelete.SetTitle("Yes, Delete Account", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonDelete);
            buttonDelete.StretchWidth(StackView);
            buttonDelete.SetHeight(44);

            AddBottomSectionDivider();

            base.OnViewModelLoadedOverride();
        }
    }
}