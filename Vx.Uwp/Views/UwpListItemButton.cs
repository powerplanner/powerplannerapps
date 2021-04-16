using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpListItemButton : UwpView<Vx.Views.ListItemButton, ListItemButton>
    {
        public UwpListItemButton()
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.ListItemButton oldView, Vx.Views.ListItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view => View.Content = view.CreateFrameworkElement());
        }
    }

    public class ListItemButton : Button
    {
        public ListItemButton()
        {
            DefaultStyleKey = typeof(ListItemButton);
        }
    }
}
