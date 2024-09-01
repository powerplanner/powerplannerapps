using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    internal class UwpWrapGrid : UwpView<Vx.Views.WrapGrid, VariableSizedWrapGrid>
    {
        public UwpWrapGrid()
        {
            View.Orientation = Orientation.Horizontal;
        }

        protected override void ApplyProperties(Vx.Views.WrapGrid oldView, Vx.Views.WrapGrid newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Background = newView.BackgroundColor.ToUwpBrush();
            View.ItemWidth = newView.ItemWidth;
            View.ItemHeight = newView.ItemHeight;

            ReconcileList(oldView?.Children, newView.Children, View.Children);
        }
    }
}
