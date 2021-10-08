using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Dialogs;
using InterfacesDroid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidColorPicker : DroidView<ColorPicker, MyOutlinedColorPicker>
    {
        public DroidColorPicker() : base(new MyOutlinedColorPicker(VxDroidExtensions.ApplicationContext))
        {
            View.SelectionChanged += View_SelectionChanged;
        }

        private void View_SelectionChanged(object sender, Android.Graphics.Color e)
        {
            var c = e;
            var color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

            if (VxView.Color?.Value != color)
            {
                VxView.Color?.ValueChanged?.Invoke(color);
            }
        }

        protected override void ApplyProperties(ColorPicker oldView, ColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Color != null)
            {
                View.SelectedColor = newView.Color.Value.ToDroid();
            }

            View.Header = newView.Header;
        }
    }
}