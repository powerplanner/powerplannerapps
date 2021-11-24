using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public class PopupComponentViewController : PopupViewController<PopupComponentViewModel>
    {
        private iOSNativeComponent _nativeComponent;
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var backOverride = ViewModel.BackOverride;
            if (backOverride != null)
            {
                BackButtonText = backOverride.Item1;
            }

            UpdateRightBarButtonItems();
            UpdateNookInsets();

            _nativeComponent = ViewModel.Render(AfterViewChanged);
            _nativeComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.Add(_nativeComponent);
            _nativeComponent.StretchWidthAndHeight(ContentView);

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
        }

        private void UpdateRightBarButtonItems()
        {
            NavItem.RightBarButtonItems = GetRightBarButtonItems().ToArray();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Title):
                    Title = ViewModel.Title;
                    break;

                case nameof(ViewModel.Commands):
                case nameof(ViewModel.SecondaryCommands):
                    UpdateRightBarButtonItems();
                    break;
            }
        }

        private IEnumerable<UIBarButtonItem> GetRightBarButtonItems()
        {
            if (ViewModel.SecondaryCommands != null && ViewModel.SecondaryCommands.Length > 0)
            {
                yield return new UIBarButtonItem(UIImage.FromBundle("MenuVerticalIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIBarButtonItemStyle.Plain, new WeakEventHandler(ButtonMore_Clicked).Handler);
            }

            if (ViewModel.Commands != null)
            {
                foreach (var command in ViewModel.Commands)
                {
                    yield return new UIBarButtonItem(command.Glyph.ToUIBarButtonSystemItem(), (obj, args) => command.Action());
                }
            }
        }

        private void ButtonMore_Clicked(object sender, EventArgs e)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetMoreOptions = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            foreach (var option in ViewModel.SecondaryCommands)
            {
                actionSheetMoreOptions.AddAction(UIAlertAction.Create(option.Text, option.Style == PopupCommandStyle.Destructive ? UIAlertActionStyle.Destructive : UIAlertActionStyle.Default, delegate
                {
                    if (option.UseQuickConfirmDelete)
                    {
                        ConfirmDelete(option.Action);
                    }
                    else
                    {
                        option.Action();
                    }
                }));
            }

            actionSheetMoreOptions.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetMoreOptions.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = NavItem.RightBarButtonItems.First();
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            this.PresentViewController(actionSheetMoreOptions, true, null);
        }

        private void ConfirmDelete(Action actualDeleteAction)
        {
            PowerPlannerUIHelper.ConfirmDeleteQuick(this, NavItem.RightBarButtonItems.First(), actualDeleteAction, PowerPlannerResources.GetString("String_YesDelete"));
        }

        /// <summary>
        /// This method only exists on iOS 11+, not sure if this class will still work on iOS 10/9, but not sure how to dynamically overload a method...
        /// </summary>
        public override void ViewSafeAreaInsetsDidChange()
        {
            if (ViewModel != null)
            {
                UpdateNookInsets();
            }
        }

        private void UpdateNookInsets()
        {
            if (SdkSupportHelper.IsSafeAreaInsetsSupported)
            {
                ViewModel.UpdateNookInsets(new Vx.Views.Thickness((float)View.SafeAreaInsets.Left, 0, (float)View.SafeAreaInsets.Right, (float)View.SafeAreaInsets.Bottom));
            }
        }

        private void AfterViewChanged(UIView view)
        {
            if (view is UIScrollView scrollView)
            {
                EnableKeyboardScrollOffsetHandling(scrollView, 0);
            }
        }

        private void PrimaryButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.PrimaryCommand.Action?.Invoke();
        }

        protected override void BackButtonClicked()
        {
            if (ViewModel.BackOverride != null)
            {
                if (ViewModel.BackOverride.Item2 != null)
                {
                    ViewModel.BackOverride.Item2();
                    return;
                }
            }

            base.BackButtonClicked();
        }
    }
}
