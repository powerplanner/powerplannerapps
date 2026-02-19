using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;
using InterfacesiOS.Controllers;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Helpers;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Helpers
{
    public static class PowerPlannerUIHelper
    {
        public static void ConfirmDeleteQuick(UIViewController viewController, UIBarButtonItem barButton, Action deleteAction, string deleteText = null)
        {
            if (deleteText == null)
            {
                deleteText = PowerPlannerResources.GetString("MenuItemDelete");
            }

            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            actionSheetAlert.AddAction(UIAlertAction.Create(deleteText, UIAlertActionStyle.Destructive, delegate { deleteAction(); }));
            actionSheetAlert.AddAction(UIAlertAction.Create(PowerPlannerResources.GetStringCancel(), UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                if (OperatingSystem.IsIOSVersionAtLeast(16))
                {
                    presentationPopover.SourceItem = barButton;
                }
                else
                {
#pragma warning disable CA1422 // BarButtonItem is obsoleted on iOS 16.0 but needed for iOS 14-15
                    presentationPopover.BarButtonItem = barButton;
#pragma warning restore CA1422
                }
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            viewController.PresentViewController(actionSheetAlert, true, null);
        }

        public static UIButton CreatePowerPlannerBlueButton(string title)
        {
            var button = new UIButton(UIButtonType.System);

            if (OperatingSystem.IsIOSVersionAtLeast(15))
            {
                var config = UIButtonConfiguration.PlainButtonConfiguration;
                config.ContentInsets = new NSDirectionalEdgeInsets(8, 8, 8, 8);
                config.Background.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
                config.Background.CornerRadius = 10;
                config.BaseForegroundColor = new UIColor(1, 1);
                config.Title = title;
                button.Configuration = config;
            }
            else
            {
                button.SetTitle(title, UIControlState.Normal);
                button.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
                button.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
                button.Layer.CornerRadius = 10;
                button.ContentEdgeInsets = new UIEdgeInsets(8, 8, 8, 8);
            }

            return button;
        }

        /// <summary>
        /// Styles the background grey in preperation for section dividers and inputs to be added
        /// </summary>
        public static void ConfigureForInputsStyle<T>(BareMvvmUIViewControllerWithScrolling<T> controller) where T : BaseViewModel
        {
            controller.View.BackgroundColor = ColorResources.InputSectionDividers;
            controller.StackViewContainer.BackgroundColor = UIColorCompat.SecondarySystemGroupedBackgroundColor;
        }
    }
}