﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.CheckBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidCheckBox : DroidView<Vx.Views.CheckBox, MaterialCheckBox>
    {
        public DroidCheckBox() : base(new MaterialCheckBox(VxDroidExtensions.ApplicationContext))
        {
            View.CheckedChange += View_CheckedChange;
            View.SetMinimumHeight(0);
        }

        private void View_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (VxView.IsChecked != null && VxView.IsChecked.Value != View.Checked)
            {
                VxView.IsChecked.ValueChanged?.Invoke(View.Checked);
            }
        }

        protected override void ApplyProperties(Vx.Views.CheckBox oldView, Vx.Views.CheckBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Enabled = newView.IsEnabled;
            View.Text = newView.Text;
            View.Checked = newView.IsChecked?.Value ?? false;
        }
    }
}