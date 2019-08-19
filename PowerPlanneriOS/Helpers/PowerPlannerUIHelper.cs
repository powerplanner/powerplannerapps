using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;
using InterfacesiOS.Controllers;
using BareMvvm.Core.ViewModels;

namespace PowerPlanneriOS.Helpers
{
    public static class PowerPlannerUIHelper
    {
        public static void ConfirmDeleteQuick(UIViewController viewController, UIBarButtonItem barButton, Action deleteAction)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            actionSheetAlert.AddAction(UIAlertAction.Create("Delete", UIAlertActionStyle.Destructive, delegate { deleteAction(); }));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = barButton;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            viewController.PresentViewController(actionSheetAlert, true, null);
        }

        public static void ConfirmDelete(string message, string title, Action deleteAction)
        {
            WeakReference<Action> weakAction = new WeakReference<Action>(deleteAction);
            IUIAlertViewDelegate del = null;
            var alertView = new UIAlertView(title, message, del, "Cancel", "Delete");
            alertView.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == 1)
                {
                    // Use a weak reference otherwise the reference to the view model's delete action
                    // gets persisted and the view model doesn't dispose
                    if (weakAction.TryGetTarget(out Action delAction))
                    {
                        delAction();
                    }
                }
            };
            alertView.Show();
        }

        public static UIButton CreatePowerPlannerBlueButton(string title)
        {
            var button = new UIButton(UIButtonType.System);
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
            button.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
            button.ContentEdgeInsets = new UIEdgeInsets()
            {
                Left = 8,
                Right = 8,
                Top = 6,
                Bottom = 6
            };

            return button;
        }

        /// <summary>
        /// Styles the background grey in preperation for section dividers and inputs to be added
        /// </summary>
        public static void ConfigureForInputsStyle<T>(BareMvvmUIViewControllerWithScrolling<T> controller) where T : BaseViewModel
        {
            controller.View.BackgroundColor = ColorResources.InputSectionDividers;
            controller.StackViewContainer.BackgroundColor = UIColor.White;
        }
    }
}