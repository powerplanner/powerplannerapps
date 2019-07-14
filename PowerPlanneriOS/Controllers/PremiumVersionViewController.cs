using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using ToolsPortable;
using PowerPlanneriOS.Helpers;
using InterfacesiOS.App;
using PowerPlannerAppDataLibrary.App;
using StoreKit;
using PowerPlanneriOS.Extensions;

namespace PowerPlanneriOS.Controllers
{
    public class PremiumVersionViewController : PopupViewControllerWithScrolling<PremiumVersionViewModel>
    {
        private UIButton _buttonUpgrade;
        private UILabel _labelError;
        private BareUIVisibilityContainer _errorVisibilityContainer;

        public PremiumVersionViewController()
        {
            Title = "Premium Version";
        }

        private void IOSInAppPurchaseExtension_ResponseReceived(object sender, InAppPurchaseHelper.PurchaseResponse e)
        {
            if (e.Success)
            {
                _errorVisibilityContainer.IsVisible = false;
            }
            else
            {
                _errorVisibilityContainer.IsVisible = true;
                _labelError.Text = e.Error;
            }
        }

        public override async void OnViewModelLoadedOverride()
        {
            if (ViewModel.ContextualMessage != null)
            {
                var labelContextualMessage = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = ViewModel.ContextualMessage,
                    Font = UIFont.PreferredBody.Bold(),
                    Lines = 0
                };
                StackView.AddArrangedSubview(labelContextualMessage);
                labelContextualMessage.StretchWidth(StackView);

                AddSpacing(16);
            }

            var labelDescription = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = @"The premium version of Power Planner unlocks a number of useful features.

The free version is limited to...
 - 1 semester
 - 5 grades per class
 - 1 repeating bulk entry of a task/event

Once you purchase the premium version, you own the premium version on all your devices.",
                Lines = 0
            };
            StackView.AddArrangedSubview(labelDescription);
            labelDescription.StretchWidth(StackView);

            AddSpacing(16);

            _errorVisibilityContainer = new BareUIVisibilityContainer()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                IsVisible = false
            };
            {
                _labelError = new UILabel()
                {
                    TextColor = UIColor.Red,
                    Font = UIFont.PreferredBody.Bold(),
                    Lines = 0
                };
                _errorVisibilityContainer.Child = _labelError;
            }
            StackView.AddArrangedSubview(_errorVisibilityContainer);
            _errorVisibilityContainer.StretchWidth(StackView);

            _buttonUpgrade = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _buttonUpgrade.SetTitle("UPGRADE TO PREMIUM", UIControlState.Normal);
            _buttonUpgrade.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
            _buttonUpgrade.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
            _buttonUpgrade.TouchUpInside += new WeakEventHandler(ButtonUpgrade_TouchUpInside).Handler;
            StackView.AddArrangedSubview(_buttonUpgrade);
            _buttonUpgrade.StretchWidth(StackView);

            AddSpacing(16);

            var buttonRestore = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonRestore.SetTitle("Restore Previous Purchase", UIControlState.Normal);
            buttonRestore.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
            buttonRestore.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
            buttonRestore.TouchUpInside += new WeakEventHandler(ButtonRestore_TouchUpInside).Handler;
            StackView.AddArrangedSubview(buttonRestore);
            buttonRestore.StretchWidth(StackView);

            if (!InAppPurchaseHelper.CanMakePayments)
            {
                _buttonUpgrade.Enabled = false;
                buttonRestore.Enabled = false;
                _labelError.Text = "App Store purchases not supported.";
                _errorVisibilityContainer.IsVisible = true;
            }

            else
            {
                iOSInAppPurchaseExtension.ResponseReceived += new WeakEventHandler<InAppPurchaseHelper.PurchaseResponse>(IOSInAppPurchaseExtension_ResponseReceived).Handler;

                string price = await InAppPurchaseHelper.GetPriceAsync();
                if (price != null)
                {
                    _buttonUpgrade.SetTitle("UPGRADE TO PREMIUM - " + price, UIControlState.Normal);
                }
            }

            base.OnViewModelLoadedOverride();
        }

        private void ButtonRestore_TouchUpInside(object sender, EventArgs e)
        {
            iOSInAppPurchaseExtension.IsNewPurchaseOperation = false;
            _errorVisibilityContainer.IsVisible = false;

            ViewModel.PromptPurchase();
        }

        private void ButtonUpgrade_TouchUpInside(object sender, EventArgs e)
        {
            iOSInAppPurchaseExtension.IsNewPurchaseOperation = true;
            _errorVisibilityContainer.IsVisible = false;

            ViewModel.PromptPurchase();
        }

        protected override int AdditionalTopPadding => 16;
        protected override int LeftPadding => 16;
        protected override int RightPadding => 16;
    }
}