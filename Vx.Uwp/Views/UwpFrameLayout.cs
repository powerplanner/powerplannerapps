using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpFrameLayout : UwpView<Vx.Views.FrameLayout, Grid>
    {
        protected override void ApplyProperties(Vx.Views.FrameLayout oldView, Vx.Views.FrameLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Background = newView.BackgroundColor.ToUwpBrush();

            ReconcileList(oldView?.Children, newView.Children, View.Children);
        }
    }
}
