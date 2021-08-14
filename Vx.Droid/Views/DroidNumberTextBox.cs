using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidNumberTextBox : DroidView<Vx.Views.NumberTextBox, BareMaterialEditDecimalNumber>
    {
        public DroidNumberTextBox() : base(new BareMaterialEditDecimalNumber(VxDroidExtensions.ApplicationContext))
        {
            View.ValueChanged += View_ValueChanged;
        }

        private void View_ValueChanged(object sender, double? e)
        {
            VxView.Number?.ValueChanged(View.Value);
        }

        protected override void ApplyProperties(NumberTextBox oldView, NumberTextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Number != null)
            {
                View.Value = newView.Number.Value;
            }

            View.Hint = newView.PlaceholderText;
            View.Enabled = newView.IsEnabled;
        }
    }
}