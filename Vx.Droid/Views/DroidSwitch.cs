﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.SwitchMaterial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidSwitch : DroidView<Vx.Views.Switch, SwitchMaterial>
    {
        public DroidSwitch() : base(new SwitchMaterial(VxDroidExtensions.ApplicationContext))
        {
            View.CheckedChange += View_CheckedChange;
        }

        private void View_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (VxView.IsOnChanged != null && VxView.IsOn != View.Checked)
            {
                VxView.IsOnChanged(View.Checked);
            }
        }

        protected override void ApplyProperties(Vx.Views.Switch oldView, Vx.Views.Switch newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Enabled = newView.IsEnabled;
            View.Text = newView.Title;
            View.Checked = newView.IsOn;
        }
    }
}