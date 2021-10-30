using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Google.Android.Material.TimePicker;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidTimePicker : DroidView<Vx.Views.TimePicker, FrameLayout>
    {
        private TextInputLayout _textInputLayout;
        private MaterialButton _button;
        public DroidTimePicker() : base(new FrameLayout(VxDroidExtensions.ApplicationContext))
        {
            _textInputLayout = new TextInputLayout(View.Context, null, Vx.Droid.Resource.Attribute.materialOutlinedTextBoxStyle);

            var editText = new TextInputEditText(_textInputLayout.Context)
            {
                LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Text = " "
            };

            _textInputLayout.AddView(editText);

            View.AddView(_textInputLayout);

            _button = new MaterialButton(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialTextButtonStyle)
            {
                Gravity = GravityFlags.Left | GravityFlags.CenterVertical
            };
            _button.SetPadding(AsPx(16), AsPx(4), 0, 0);

            _button.Click += View_Click;

            View.AddView(_button, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }

        private void View_Click(object sender, EventArgs e)
        {
            new TimePickerDialog(
                context: View.Context,
                callBack: OnTimePicked,
                hourOfDay: VxView.Value?.Value.Hours ?? 9,
                minute: VxView.Value?.Value.Minutes ?? 0,
                is24HourView: Android.Text.Format.DateFormat.Is24HourFormat(View.Context)).Show();
        }

        private void OnTimePicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            VxView.Value?.ValueChanged?.Invoke(new TimeSpan(e.HourOfDay, e.Minute, 0));
        }

        protected override void ApplyProperties(Vx.Views.TimePicker oldView, Vx.Views.TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Value != null)
            {
                _button.Text = DateHelper.ToShortTimeString(DateTime.Today.Add(newView.Value.Value));
            }

            _textInputLayout.Hint = newView.Header;
        }
    }
}