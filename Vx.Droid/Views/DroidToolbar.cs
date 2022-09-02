using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidToolbar : DroidView<Vx.Views.Toolbar, AndroidX.AppCompat.Widget.Toolbar>
    {
        public DroidToolbar() : base(new AndroidX.AppCompat.Widget.Toolbar(VxDroidExtensions.ApplicationContext))
        {
            View.Context.SetTheme(Resource.Style.ThemeOverlay_MaterialComponents_Toolbar_Primary);
            View.SetTitleTextColor(System.Drawing.Color.White.ToDroid());
            View.NavigationClick += View_NavigationClick;
            View.MenuItemClick += View_MenuItemClick;
        }

        private void View_MenuItemClick(object sender, AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            int size = View.Menu.Size();
            int itemIndex = 0;
            for (int i = 0; i < size; i++)
            {
                var menuItem = View.Menu.GetItem(i);
                if (menuItem == e.Item)
                {
                    break;
                }

                if (menuItem.HasSubMenu)
                {
                    var subMenu = menuItem.SubMenu;
                    bool subFound = false;
                    for (int s = 0; s < subMenu.Size(); s++)
                    {
                        itemIndex++;

                        if (subMenu.GetItem(s) == e.Item)
                        {
                            subFound = true;
                            break;
                        }
                    }

                    if (subFound)
                    {
                        break;
                    }
                }

                itemIndex++;
            }

            VxCommands().ElementAt(itemIndex).Action?.Invoke();
        }

        private IEnumerable<Vx.Views.ToolbarCommand> VxCommands()
        {
            foreach (var c in VxView.PrimaryCommands)
            {
                yield return c;

                if (c.SubCommands != null)
                {
                    foreach (var s in c.SubCommands)
                    {
                        yield return s;
                    }
                }
            }

            foreach (var c in VxView.SecondaryCommands)
            {
                yield return c;
            }
        }

        private void View_NavigationClick(object sender, AndroidX.AppCompat.Widget.Toolbar.NavigationClickEventArgs e)
        {
            VxView?.OnBack?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.Toolbar oldView, Vx.Views.Toolbar newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Title = newView.Title;
            View.SetBackgroundColor(newView.BackgroundColor.ToDroid());

            if (newView.OnBack != null && View.NavigationIcon == null)
            {
                View.SetNavigationIcon(Resource.Drawable.arrow_back_24px);
            }
            else if (newView.OnBack == null && View.NavigationIcon != null)
            {
                View.NavigationIcon = null;
            }

            if (!Vx.Views.ToolbarCommand.AreSame(oldView?.PrimaryCommands, newView.PrimaryCommands)
                || !Vx.Views.ToolbarCommand.AreSame(oldView?.SecondaryCommands, newView.SecondaryCommands))
            {
                View.Menu.Clear();

                if (newView.PrimaryCommands.Any())
                {
                    int i = 0;
                    foreach (var c in newView.PrimaryCommands)
                    {
                        ISubMenu subMenu = null;
                        if (c.SubCommands != null && c.SubCommands.Any())
                        {
                            subMenu = View.Menu.AddSubMenu(c.Text);
                        }
                        else
                        {
                            View.Menu.Add(c.Text);
                        }

                        var item = View.Menu.GetItem(i);
                        item.SetIcon(ToDroidIconResource(c.Glyph));
                        item.SetShowAsAction(ShowAsAction.Always | ShowAsAction.WithText);

                        if (subMenu != null)
                        {
                            AddTextCommands(subMenu, c.SubCommands);
                        }

                        i++;
                    }
                }

                AddTextCommands(View.Menu, newView.SecondaryCommands);
            }
        }

        private static void AddTextCommands(IMenu menu, IEnumerable<Vx.Views.ToolbarCommand> commands)
        {
            foreach (var c in commands)
            {
                menu.Add(c.Text);
            }
        }

        private static int ToDroidIconResource(string glyph)
        {
            switch (glyph)
            {
                case MaterialDesign.MaterialDesignIcons.Add:
                    return Resource.Drawable.add_24px;

                case MaterialDesign.MaterialDesignIcons.ChevronLeft:
                case MaterialDesign.MaterialDesignIcons.NavigateBefore:
                case MaterialDesign.MaterialDesignIcons.KeyboardArrowLeft:
                    return Resource.Drawable.chevron_left_24px;

                case MaterialDesign.MaterialDesignIcons.ChevronRight:
                case MaterialDesign.MaterialDesignIcons.NavigateNext:
                case MaterialDesign.MaterialDesignIcons.KeyboardArrowRight:
                    return Resource.Drawable.chevron_right_24px;

                case MaterialDesign.MaterialDesignIcons.Today:
                    return Resource.Drawable.today_24px;
                //case MaterialDesign.MaterialDesignIcons.Check:
                //case MaterialDesign.MaterialDesignIcons.Save:
                //    return Resource.Drawable.ic_check_white_36dp;

                //case MaterialDesign.MaterialDesignIcons.Delete:
                //    throw new NotImplementedException();

                //case MaterialDesign.MaterialDesignIcons.Edit:
                //    return Resource.Drawable.ic_edit_white_24dp;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}