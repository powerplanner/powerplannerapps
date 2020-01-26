﻿using Foundation;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using System;
using UIKit;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlanneriOS.ViewModels;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Welcome
{
    public class WelcomeViewController : BareMvvmUIViewController<WelcomeViewModel>
    {
        public WelcomeViewController()
        {
            View.BackgroundColor = new UIColor(31 / 255f, 38 / 255f, 86 / 255f, 1);
        }

        public override void OnViewModelLoadedOverride()
        {
            // [viewTextContainer]
            // [viewButtons]
            var safeView = BareUISafeView.CreateAndAddTo(View);
            {
                var viewTopSpacer = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                safeView.Add(viewTopSpacer);
                viewTopSpacer.StretchWidth(safeView);

                var viewTextContainer = new UIStackView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Axis = UILayoutConstraintAxis.Vertical,
                    Spacing = 8
                };
                {
                    var labelTitle = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Text = "Power Planner",
                        TextColor = new UIColor(1, 1),
                        Font = UIFont.PreferredTitle1,
                        TextAlignment = UITextAlignment.Center
                    };
                    viewTextContainer.AddArrangedSubview(labelTitle);
                    labelTitle.StretchWidth(viewTextContainer);

                    var labelSubtitle = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Text = "The ultimate homework planner",
                        TextColor = new UIColor(0.9f, 1),
                        Font = UIFont.PreferredCaption1,
                        TextAlignment = UITextAlignment.Center
                    };
                    viewTextContainer.AddArrangedSubview(labelSubtitle);
                    labelSubtitle.StretchWidth(viewTextContainer);
                }
                safeView.Add(viewTextContainer);
                viewTextContainer.StretchWidth(safeView, left: 16, right: 16);

                var viewLowerSpacer = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                safeView.Add(viewLowerSpacer);
                viewLowerSpacer.StretchWidth(safeView);

                var viewButtons = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                {
                    var buttonLogin = PowerPlannerUIHelper.CreatePowerPlannerBlueButton("Log In");
                    buttonLogin.TranslatesAutoresizingMaskIntoConstraints = false;
                    buttonLogin.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.Login(); }).Handler;
                    viewButtons.Add(buttonLogin);
                    buttonLogin.StretchHeight(viewButtons);

                    var buttonCreateAccount = PowerPlannerUIHelper.CreatePowerPlannerBlueButton("Create Account");
                    buttonCreateAccount.TranslatesAutoresizingMaskIntoConstraints = false;
                    buttonCreateAccount.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.CreateAccount(); }).Handler;
                    viewButtons.Add(buttonCreateAccount);
                    buttonCreateAccount.StretchHeight(viewButtons);

                    viewButtons.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|[buttonLogin(==buttonCreateAccount)]-8-[buttonCreateAccount]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                        "buttonLogin", buttonLogin,
                        "buttonCreateAccount", buttonCreateAccount)));
                }
                safeView.Add(viewButtons);
                viewButtons.StretchWidth(safeView, left: 16, right: 16);

                safeView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-16-[viewTopSpacer(==viewLowerSpacer)][viewTextContainer][viewLowerSpacer]-16-[viewButtons]-16-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary(
                    "viewTextContainer", viewTextContainer,
                    "viewButtons", viewButtons,
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
                        Text = "About"
                    };
                    buttonSettings.Add(label);
                    label.StretchHeight(buttonSettings);

                    buttonSettings.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[image(22)]-8-[label]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                        "image", settingsImage,
                        "label", label));
                }
                safeView.Add(buttonSettings);
                buttonSettings.TouchUpInside += new WeakEventHandler(delegate { OpenAboutPageAsPopup(); }).Handler;
                buttonSettings.PinToTop(safeView, top: UIDevice.CurrentDevice.CheckSystemVersion(11, 0) ? 12 : 28); // We do this comparison since on iOS 11 the safe view adds extra padding
                buttonSettings.PinToLeft(safeView, left: 12);
                buttonSettings.SetHeight(30);
            }

            base.OnViewModelLoadedOverride();
        }

        public void OpenAboutPageAsPopup()
        {
            var mainWindowViewModel = ViewModel.FindAncestor<MainWindowViewModel>();
            mainWindowViewModel.ShowPopup(new AboutViewModelAsPopup(mainWindowViewModel));
        }
    }
}