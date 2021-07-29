using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Vx.iOS.Views;
using Vx.Views;

namespace Vx.iOS
{
    public static class VxiOSExtensions
    {
        static VxiOSExtensions()
        {
            Theme.Current = new VxiOSTheme();
            VxPlatform.Current = Platform.iOS;

            NativeView.CreateNativeView = view =>
            {
                if (view is Vx.Views.VxComponent c)
                {
                    return new iOSVxComponent(c);
                }

                if (view is Vx.Views.TextBlock)
                {
                    return new iOSTextBlock();
                }

                if (view is Vx.Views.LinearLayout)
                {
                    return new iOSLinearLayout();
                }

                if (view is Vx.Views.Button)
                {
                    if (view is Vx.Views.AccentButton)
                    {
                        return new iOSAccentButton();
                    }
                    if (view is Vx.Views.TextButton)
                    {
                        return new iOSTextButton();
                    }
                    return new iOSButton();
                }

                if (view is Vx.Views.TextBox)
                {
                    return new iOSTextBox();
                }

                if (view is Vx.Views.ScrollView)
                {
                    return new iOSScrollView();
                }

                if (view is Vx.Views.FontIcon)
                {
                    return new iOSFontIcon();
                }

                if (view is Vx.Views.ListItemButton)
                {
                    return new iOSListItemButton();
                }

                if (view is Vx.Views.TransparentContentButton)
                {
                    return new iOSTransparentContentButton();
                }

                if (view is Vx.Views.NumberTextBox)
                {
                    return new iOSNumberTextBox();
                }

                if (view is Vx.Views.Switch)
                {
                    return new iOSSwitch();
                }

                if (view is Vx.Views.CheckBox)
                {
                    return new iOSCheckBox();
                }

                if (view is Vx.Views.TimePicker)
                {
                    return new iOSTimePicker();
                }

                if (view is Vx.Views.Border)
                {
                    return new iOSBorder();
                }

                if (view is Vx.Views.ComboBox)
                {
                    return new iOSComboBox();
                }

#if DEBUG
                // Control not implemented
                System.Diagnostics.Debugger.Break();
#endif
                throw new NotImplementedException();
            };

            NativeView.ShowContextMenu = ShowContextMenu;
        }

        private static void ShowContextMenu(ContextMenu contextMenu, View view)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            foreach (var item in contextMenu.Items)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(item.Text, UIAlertActionStyle.Default, delegate { item.Click(); }));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = view.NativeView.View as UIView;
                presentationPopover.SourceRect = (view.NativeView.View as UIView).Frame;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            (view.NativeView.View as UIView).GetViewController().PresentViewController(actionSheetAlert, true, null);
        }

        public static iOSNativeComponent Render(this VxComponent component, Action<UIView> afterViewChanged = null)
        {
            if (component.NativeComponent != null)
            {
                return component.NativeComponent as iOSNativeComponent;
            }

            var nativeComponent = new iOSNativeComponent()
            {
                AfterViewChanged = afterViewChanged
            };
            component.InitializeForDisplay(nativeComponent);
            return nativeComponent;
        }

        internal static UIView CreateUIView(this Vx.Views.View view, Vx.Views.View parentView)
        {
            return view.CreateNativeView(parentView).View as UIView;
        }

        internal static UIColor ToUI(this Color color)
        {
            return UIColor.FromRGBA(color.R, color.G, color.B, color.A);
        }
    }
}