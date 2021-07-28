using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.Views
{
    public static class PopupMenuConfirmDelete
    {
        public static void Show(FrameworkElement el, Action onDelete)
        {
            var menuFlyout = new MenuFlyout();
            var itemYesDelete = new MenuFlyoutItem()
            {
                Text = LocalizedResources.GetString("String_YesDelete")
            };
            itemYesDelete.Click += delegate
            {
                onDelete();
            };
            menuFlyout.Items.Add(itemYesDelete);

            try
            {
                menuFlyout.ShowAt(el);
            }

            catch { }
        }
    }
}
