using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.CheckBox;
using InterfacesDroid.Helpers;
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

            UpdateCheckedTint();
            View.ViewAttachedToWindow += (s, e) => Vx.Views.Theme.ThemeChanged += ThemeChanged;
            View.ViewDetachedFromWindow += (s, e) => Vx.Views.Theme.ThemeChanged -= ThemeChanged;
        }

        private void ThemeChanged(object sender, EventArgs e)
        {
            UpdateCheckedTint();
        }

        private void UpdateCheckedTint()
        {
            var accentColor = Vx.Views.Theme.Current.AccentColor.ToDroid();
            View.ButtonTintList = new ColorStateList(
                new int[][] {
                    new int[] { Android.Resource.Attribute.StateChecked },
                    new int[] { }
                },
                new int[] {
                    accentColor,
                    View.CurrentTextColor  // unchecked state keeps default
                });
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