using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers
{
    public class InitialSyncViewController : BareMvvmUIViewController<InitialSyncViewModel>
    {
        public InitialSyncViewController()
        {
            View.BackgroundColor = new UIColor(31 / 255f, 38 / 255f, 86 / 255f, 1);
        }

        public override void OnViewModelLoadedOverride()
        {
            var safeView = BareUISafeView.CreateAndAddTo(View);
            {
                var viewTopSpacer = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                safeView.Add(viewTopSpacer);
                viewTopSpacer.StretchWidth(safeView);

                var viewCenterContainer = new UIStackView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Axis = UILayoutConstraintAxis.Vertical,
                    Spacing = 8
                };
                {
                    var icon = new UIImageView(UIImage.FromBundle("PowerPlannerIcon"))
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        ContentMode = UIViewContentMode.ScaleAspectFit
                    };
                    icon.SetHeight(100);
                    viewCenterContainer.AddArrangedSubview(icon);

                    var progressRing = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Large)
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Color = UIColor.White
                    };
                    progressRing.StartAnimating();
                    viewCenterContainer.AddUnderVisiblity(progressRing, BindingHost, nameof(ViewModel.IsSyncing));

                    var labelSyncing = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Text = PowerPlannerResources.GetString("LoginPage_String_SyncingAccount"),
                        TextColor = new UIColor(1, 1),
                        Font = UIFont.PreferredTitle2,
                        TextAlignment = UITextAlignment.Center
                    };
                    viewCenterContainer.AddUnderVisiblity(labelSyncing, BindingHost, nameof(ViewModel.IsSyncing));

                    var labelSyncError = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Text = PowerPlannerResources.GetString("String_SyncError"),
                        TextColor = new UIColor(1, 1),
                        Font = UIFont.PreferredTitle2,
                        TextAlignment = UITextAlignment.Center
                    };
                    viewCenterContainer.AddUnderVisiblity(labelSyncError, BindingHost, nameof(ViewModel.IsSyncing), invert: true);

                    var buttonTryAgain = PowerPlannerUIHelper.CreatePowerPlannerBlueButton("Try again");
                    buttonTryAgain.TranslatesAutoresizingMaskIntoConstraints = false;
                    buttonTryAgain.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.TryAgain(); }).Handler;
                    viewCenterContainer.AddUnderVisiblity(buttonTryAgain, BindingHost, nameof(ViewModel.IsSyncing), invert: true);

                    var labelErrorDescription = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextColor = new UIColor(0.9f, 1),
                        Font = UIFont.PreferredCaption1,
                        TextAlignment = UITextAlignment.Left,
                        Lines = 0
                    };
                    BindingHost.SetLabelTextBinding(labelErrorDescription, nameof(ViewModel.Error));
                    viewCenterContainer.AddUnderVisiblity(labelErrorDescription, BindingHost, nameof(ViewModel.Error));
                }
                safeView.Add(viewCenterContainer);
                viewCenterContainer.StretchWidth(safeView, left: 16, right: 16);

                var viewLowerSpacer = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                safeView.Add(viewLowerSpacer);
                viewLowerSpacer.StretchWidth(safeView);

                safeView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-16-[viewTopSpacer(==viewLowerSpacer)][viewCenterContainer][viewLowerSpacer]-16-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary(
                    "viewCenterContainer", viewCenterContainer,
                    "viewTopSpacer", viewTopSpacer,
                    "viewLowerSpacer", viewLowerSpacer)));

                var buttonSettings = new UIControl()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                {
                    var settingsImage = new UIImageView(UIImage.FromBundle("SettingsIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate))
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TintColor = UIColor.White,
                        ContentMode = UIViewContentMode.ScaleAspectFit
                    };
                    buttonSettings.Add(settingsImage);
                    settingsImage.StretchHeight(buttonSettings, top: 4, bottom: 4);

                    var label = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = UIFont.PreferredCaption1,
                        TextColor = UIColor.White,
                        Text = PowerPlannerResources.GetString("MainMenuItem_Settings")
                    };
                    buttonSettings.Add(label);
                    label.StretchHeight(buttonSettings);

                    buttonSettings.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[image(22)]-8-[label]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                        "image", settingsImage,
                        "label", label));
                }
                BindingHost.SetVisibilityBinding(buttonSettings, nameof(ViewModel.IsSyncing), invert: true);
                safeView.Add(buttonSettings);

                buttonSettings.TouchUpInside += new WeakEventHandler(delegate { ViewModel.OpenSettings(asPopup: true); }).Handler;
                buttonSettings.PinToTop(safeView, top: 12); // The safe view adds extra padding on devices with notch

                buttonSettings.PinToLeft(safeView, left: 12);
                buttonSettings.SetHeight(30);
            }

            base.OnViewModelLoadedOverride();
        }
    }
}