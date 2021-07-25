using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpBorder : UwpView<Vx.Views.Border, Border>
    {
        protected override void ApplyProperties(Vx.Views.Border oldView, Vx.Views.Border newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Background = newView.BackgroundColor.ToUwpBrush();
            View.Padding = newView.Padding.ToUwp();
            View.BorderThickness = newView.BorderThickness.ToUwp();
            View.BorderBrush = newView.BorderColor.ToUwpBrush();

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                View.Child = view.CreateFrameworkElement();
            });
        }
    }
}
