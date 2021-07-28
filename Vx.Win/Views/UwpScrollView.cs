using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Microsoft.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpScrollView : UwpView<Vx.Views.ScrollView, ScrollViewer>
    {
        public UwpScrollView()
        {
            View.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                View.Content = view.CreateFrameworkElement();
            });
        }
    }
}
