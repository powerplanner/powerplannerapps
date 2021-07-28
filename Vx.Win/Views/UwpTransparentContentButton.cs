using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpTransparentContentButton : UwpView<Vx.Views.TransparentContentButton, TransparentButton>
    {
        public UwpTransparentContentButton()
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.TransparentContentButton oldView, Vx.Views.TransparentContentButton newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view => View.Content = view.CreateFrameworkElement());
        }
    }

    public class TransparentButton : Button
    {
        public TransparentButton()
        {
            DefaultStyleKey = typeof(TransparentButton);
        }
    }
}
