using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpLinearLayout : UwpView<LinearLayout, StackPanel>
    {
        protected override void ApplyProperties(LinearLayout oldView, LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            ReconcileList(oldView?.Children, newView.Children, View.Children);
        }
    }
}
