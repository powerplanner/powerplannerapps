using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TimePicker;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidTimePicker : DroidView<Vx.Views.TimePicker, MaterialButton>
    {
        public DroidTimePicker() : base(new MaterialButton(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialTextButtonStyle))
        {
            View.SetPadding(0, 0, 0, 0);

            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            new TimePickerDialog(
                context: View.Context,
                callBack: OnTimePicked,
                hourOfDay: VxView.Value.Hours,
                minute: VxView.Value.Minutes,
                is24HourView: Android.Text.Format.DateFormat.Is24HourFormat(View.Context)).Show();
        }

        private void OnTimePicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            if (VxView.ValueChanged != null)
            {
                VxView.ValueChanged(new TimeSpan(e.HourOfDay, e.Minute, 0));
            }
        }

        protected override void ApplyProperties(Vx.Views.TimePicker oldView, Vx.Views.TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = DateHelper.ToShortTimeString(DateTime.Today.Add(newView.Value));
        }
    }
}