using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidTransparentContentMenuButton : DroidView<Vx.Views.TransparentContentMenuButton, Android.Widget.FrameLayout>
    {
        public DroidTransparentContentMenuButton() : base(new Android.Widget.FrameLayout(VxDroidExtensions.ApplicationContext))
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            var menuItems = VxView?.Menu;
            if (menuItems == null || menuItems.Count == 0)
            {
                return;
            }

            var popupMenu = new PopupMenu(VxDroidExtensions.ApplicationContext, View);
            var mappedItems = new List<(Vx.Views.IMenuItem Item, Android.Views.IMenuItem NativeItem)>();

            foreach (var item in menuItems)
            {
                if (item == null || item is Vx.Views.MenuSeparator)
                {
                    continue;
                }

                if (item is Vx.Views.MenuItem menuItem)
                {
                    if (menuItem.SubItems != null && menuItem.SubItems.Any(subItem => subItem != null))
                    {
                        var subMenu = popupMenu.Menu.AddSubMenu(menuItem.Text);
                        AddMenuItems(subMenu, menuItem.SubItems, mappedItems);
                    }
                    else
                    {
                        var nativeItem = popupMenu.Menu.Add(menuItem.Text);
                        mappedItems.Add((menuItem, nativeItem));
                    }
                }
                else if (item is Vx.Views.MenuRadioItem radioItem)
                {
                    var nativeItem = popupMenu.Menu.Add(radioItem.Text);
                    nativeItem.SetCheckable(true);
                    nativeItem.SetChecked(radioItem.IsChecked);
                    mappedItems.Add((radioItem, nativeItem));
                }
            }

            popupMenu.MenuItemClick += (sender, args) =>
            {
                var match = mappedItems.FirstOrDefault(x => ReferenceEquals(x.NativeItem, args.Item));

                if (match.Item is Vx.Views.MenuItem menuItem)
                {
                    menuItem.Click?.Invoke();
                }
                else if (match.Item is Vx.Views.MenuRadioItem radioItem)
                {
                    radioItem.Click?.Invoke();
                }
            };

            popupMenu.Show();
        }

        private static void AddMenuItems(ISubMenu subMenu, IEnumerable<Vx.Views.IMenuItem> menuItems, List<(Vx.Views.IMenuItem Item, Android.Views.IMenuItem NativeItem)> mappedItems)
        {
            foreach (var item in menuItems)
            {
                if (item == null || item is Vx.Views.MenuSeparator)
                {
                    continue;
                }

                if (item is Vx.Views.MenuItem menuItem)
                {
                    var nativeItem = subMenu.Add(menuItem.Text);
                    mappedItems.Add((menuItem, nativeItem));
                }
                else if (item is Vx.Views.MenuRadioItem radioItem)
                {
                    var nativeItem = subMenu.Add(radioItem.Text);
                    nativeItem.SetCheckable(true);
                    nativeItem.SetChecked(radioItem.IsChecked);
                    mappedItems.Add((radioItem, nativeItem));
                }
            }
        }

        protected override void ApplyProperties(TransparentContentMenuButton oldView, TransparentContentMenuButton newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                View.RemoveAllViews();

                if (view != null)
                {
                    View.AddView(view.CreateDroidView(VxView));
                }
            });

            View.ContentDescription = newView.AltText ?? newView.TooltipText ?? "";
        }
    }
}
