using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpListItemButton : UwpView<Vx.Views.ListItemButton, TransparentButtonWithHoverBorder>
    {
        public UwpListItemButton()
        {
            View.Click += View_Click;
            View.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.ListItemButton oldView, Vx.Views.ListItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view => View.Content = view?.CreateFrameworkElement());

            AutomationProperties.SetName(View, newView.AltText ?? "");
        }
    }

    public partial class TransparentButtonWithHoverBorder : Button
    {
        public TransparentButtonWithHoverBorder()
        {
            DefaultStyleKey = typeof(TransparentButtonWithHoverBorder);
        }
    }
}
