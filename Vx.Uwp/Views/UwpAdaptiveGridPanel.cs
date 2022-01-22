using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace Vx.Uwp.Views
{
    public class UwpAdaptiveGridPanel : UwpView<Vx.Views.AdaptiveGridPanel, MyAdaptiveGridPanel>
    {
        public UwpAdaptiveGridPanel()
        {
            View.StretchIfOnlyOneChild = true;
        }

        protected override void ApplyProperties(AdaptiveGridPanel oldView, AdaptiveGridPanel newView)
        {
            base.ApplyProperties(oldView, newView);

            View.MinColumnWidth = newView.MinColumnWidth;
            View.ColumnSpacing = newView.ColumnSpacing;

            ReconcileList(oldView?.Children, newView.Children, View.Children);
        }
    }
}
