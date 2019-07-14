using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class AboutViewController : BareMvvmUIViewController<AboutViewModel>
    {
        private object _tabBarHeightListener;

        public AboutViewController()
        {
            Title = "About";

            var scrollView = new UIScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(scrollView);
            scrollView.StretchWidthAndHeight(View);

            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical
            };
            scrollView.Add(stackView);
            stackView.ConfigureForVerticalScrolling(scrollView, top: 16, left: 16, right: 16, bottom: 16);

            ConfigureUI(stackView);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                scrollView.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });
        }

        private static void ButtonFacebook_TouchUpInside(object sender, EventArgs e)
        {
            NSUrl url = new NSUrl("https://facebook.com/powerplanner");

            UIApplication.SharedApplication.OpenUrl(url);
        }

        private static void ButtonContact_TouchUpInside(object sender, EventArgs e)
        {
            EmailDeveloper();
        }

        public static void EmailDeveloper()
        {
            NSUrl url = new NSUrl("mailto:?to=barebonesdev@live.com&subject=Power%20Planner%20for%20iOS%20-%20Contact%20Developer%20-%20" + Variables.VERSION);

            if (!UIApplication.SharedApplication.OpenUrl(url))
            {
                var dontWait = new PortableMessageDialog("Looks like you don't have email configured on your phone. You'll need to set up email before you can send an email.", "Email app not configured").ShowAsync();
            }
        }

        public static void ConfigureUI(UIStackView stackView)
        {
            AddHeader(stackView, "Version");
            AddText(stackView, Variables.VERSION.ToString());

            AddHeader(stackView, "Developer");
            AddText(stackView, "BareBones Dev, owned by Andrew Bares");

            AddHeader(stackView, "About the App");
            AddText(stackView, PowerPlannerResources.GetString("Settings_AboutPage_AboutAppValue.Text"));

            AddHeader(stackView, "Contact");

            var buttonContact = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left
            };
            buttonContact.SetTitle("BareBonesDev@live.com", UIControlState.Normal);
            buttonContact.TouchUpInside += ButtonContact_TouchUpInside;
            stackView.AddArrangedSubview(buttonContact);
            buttonContact.StretchWidth(stackView);

            var buttonFacebook = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left
            };
            buttonFacebook.SetTitle("Facebook page", UIControlState.Normal);
            buttonFacebook.TouchUpInside += ButtonFacebook_TouchUpInside;
            stackView.AddArrangedSubview(buttonFacebook);
            buttonFacebook.StretchWidth(stackView);
        }

        private static void AddHeader(UIStackView stackView, string text)
        {
            if (stackView.ArrangedSubviews.Length > 0)
            {
                AddSpacer(stackView);
            }

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = text,
                Font = UIFont.PreferredBody.Bold()
            };
            stackView.AddArrangedSubview(label);
            label.StretchWidth(stackView);
        }

        private static void AddText(UIStackView stackView, string text)
        {
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = text,
                Lines = 0
            };
            stackView.AddArrangedSubview(label);
            label.StretchWidth(stackView);
        }

        private static void AddSpacer(UIStackView stackView)
        {
            stackView.AddArrangedSubview(new UIView() { TranslatesAutoresizingMaskIntoConstraints = false }.SetHeight(16));
        }
    }
}