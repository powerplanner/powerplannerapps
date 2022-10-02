using System;
using System.Collections.Generic;
using System.Linq;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSToolbar : iOSView<Vx.Views.Toolbar, UIView>
    {
        private UINavigationBar NavBar;

        public iOSToolbar()
        {
            var statusBarView = UIStatusBarView.CreateAndAddTo(View);
            statusBarView.StretchWidth(View);

            NavBar = new UINavigationBar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Translucent = false,
                Items = new UINavigationItem[]
                {
                    new UINavigationItem()
                }
            };
            View.AddSubview(NavBar);
            NavBar.StretchWidth(View);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[statusBar][navBar]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "statusBar", statusBarView,
                "navBar", NavBar));
        }

        protected override void ApplyProperties(Toolbar oldView, Toolbar newView)
        {
            base.ApplyProperties(oldView, newView);

            View.BackgroundColor = newView.BackgroundColor.ToUI();
            NavBar.BarTintColor = newView.BackgroundColor.ToUI();

            if (oldView?.ForegroundColor != newView.ForegroundColor)
            {
                NavBar.TintColor = newView.ForegroundColor.ToUI();

                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    NavBar.TitleTextAttributes = new UIStringAttributes()
                    {
                        ForegroundColor = newView.ForegroundColor.ToUI()
                    };
                }
            }

            if (oldView?.BackgroundColor != newView.BackgroundColor || oldView?.ForegroundColor != newView.ForegroundColor)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    var appearance = new UINavigationBarAppearance();
                    appearance.ConfigureWithOpaqueBackground();
                    appearance.BackgroundColor = View.BackgroundColor;
                    appearance.TitleTextAttributes = new UIStringAttributes()
                    {
                        ForegroundColor = NavBar.TintColor
                    };

                    NavBar.StandardAppearance = appearance;
                    NavBar.ScrollEdgeAppearance = appearance;
                }
            }

            NavBar.TopItem.Title = newView.Title;

            if (!Vx.Views.ToolbarCommand.AreSame(oldView?.PrimaryCommands, newView.PrimaryCommands)
                || !Vx.Views.ToolbarCommand.AreSame(oldView?.SecondaryCommands, newView.SecondaryCommands))
            {
                NavBar.TopItem.RightBarButtonItems = GetRightBarButtonItems(newView).ToArray();
            }
        }

        private IEnumerable<UIBarButtonItem> GetRightBarButtonItems(Toolbar toolbar)
        {
            if (toolbar.SecondaryCommands != null && toolbar.SecondaryCommands.Any())
            {
                yield return new UIBarButtonItem(UIImage.FromBundle("MenuVerticalIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIBarButtonItemStyle.Plain, new WeakEventHandler(ButtonMore_Clicked).Handler);
            }

            if (toolbar.PrimaryCommands != null)
            {
                foreach (var command in toolbar.PrimaryCommands)
                {
                    var button = command.Glyph.ToUIBarButtonItem();
                    button.Clicked += delegate
                    {
                        if (command.Action != null)
                        {
                            command.Action();
                        }
                        else if (command.SubCommands != null && command.SubCommands.Any())
                        {
                            ShowSubCommands(command.SubCommands, button);
                        }
                    };
                    yield return button;
                }
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private class VxUIBarButtonItem : UIBarButtonItem
        {
            private ToolbarCommand _command;
            private iOSToolbar _toolbar;
            public VxUIBarButtonItem(iOSToolbar toolbar, ToolbarCommand command) : base(command.Glyph.ToUIBarButtonSystemItem())
            {
                _command = command;
                _toolbar = toolbar;
                base.Clicked += VxUIBarButtonItem_Clicked;
            }

            private void VxUIBarButtonItem_Clicked(object sender, EventArgs e)
            {
                InvokeCommand();
            }

            private void InvokeCommand()
            {
                if (_command.Action != null)
                {
                    _command.Action();
                }
                else if (_command.SubCommands != null && _command.SubCommands.Any())
                {
                    _toolbar.ShowSubCommands(_command.SubCommands, this);
                }
            }
        }

        private void ShowSubCommands(IEnumerable<ToolbarCommand> commands, UIBarButtonItem source)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetMoreOptions = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            foreach (var option in commands)
            {
                actionSheetMoreOptions.AddAction(UIAlertAction.Create(option.Text, option.Style == ToolbarCommandStyle.Destructive ? UIAlertActionStyle.Destructive : UIAlertActionStyle.Default, delegate
                {
                    option.Action?.Invoke();
                }));
            }

            actionSheetMoreOptions.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetMoreOptions.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = source;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            View.GetViewController().PresentViewController(actionSheetMoreOptions, true, null);
        }

        private void ButtonMore_Clicked(object sender, EventArgs e)
        {
            ShowSubCommands(VxView.SecondaryCommands, NavBar.TopItem.RightBarButtonItems.First());
        }
    }
}

