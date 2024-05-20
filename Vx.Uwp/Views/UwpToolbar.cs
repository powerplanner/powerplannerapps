using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Components.OnlyForNativeLibraries;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    internal class UwpToolbar : UwpView<Toolbar, Windows.UI.Xaml.Controls.Border>
    {
        private ToolbarComponent _toolbarComponent;
        public UwpToolbar()
        {
            _toolbarComponent = new ToolbarComponent();
            View.Child = _toolbarComponent.Render();
            View.Height = ToolbarComponent.ToolbarHeight;
        }

        protected override void ApplyProperties(Toolbar oldView, Toolbar newView)
        {
            base.ApplyProperties(oldView, newView);
            View.Height = ToolbarComponent.ToolbarHeight;

            _toolbarComponent.Toolbar = newView;
            _toolbarComponent.RenderOnDemand();
        }
    }
}
