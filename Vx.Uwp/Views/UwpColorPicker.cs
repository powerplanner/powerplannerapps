using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace Vx.Uwp.Views
{
    public class UwpColorPicker : UwpView<ColorPicker, InterfacesUWP.ColorPicker>
    {
        public UwpColorPicker()
        {
            View.SelectedColorChanged += View_SelectedColorChanged;
        }

        private void View_SelectedColorChanged(object sender, Windows.UI.Color e)
        {
            if (VxView.Color != null && VxView.Color.Value.ToUwpColor() != e)
            {
                VxView.Color.ValueChanged?.Invoke(System.Drawing.Color.FromArgb(e.A, e.R, e.G, e.B));
            }
        }

        protected override void ApplyProperties(ColorPicker oldView, ColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Header = newView.Header;
            View.IsEnabled = newView.IsEnabled;

            if (newView.Color?.Value != null)
            {
                View.SelectedColor = newView.Color.Value.ToUwpColor();
            }
        }
    }
}
