using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Uwp.Views
{
    internal class UwpCustomColorPicker : UwpView<Vx.Views.CustomColorPicker, CustomColorPicker>
    {
        public UwpCustomColorPicker()
        {
            View.ColorChanged += View_ColorChanged;
        }

        private void View_ColorChanged(object sender, Windows.UI.Color e)
        {
            var systemColor = System.Drawing.Color.FromArgb(e.A, e.R, e.G, e.B);
            if (VxView.Color != null && VxView.Color.Value != systemColor)
            {
                VxView.Color.ValueChanged?.Invoke(systemColor);
            }
        }

        protected override void ApplyProperties(Vx.Views.CustomColorPicker oldView, Vx.Views.CustomColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Header = newView.Header;
            if (newView.Color != null)
            {
                View.Color = newView.Color.Value.ToUwpColor();
            }
        }
    }
}
