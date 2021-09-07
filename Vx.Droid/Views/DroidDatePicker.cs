using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Lifecycle;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidDatePicker : DroidView<Vx.Views.DatePicker, TextInputLayout>
    {
        private TextInputEditText _editText;

        public DroidDatePicker() : base(new TextInputLayout(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialOutlinedTextBoxStyle))
        {
            _editText = new TextInputEditText(View.Context)
            {
                LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Focusable = false,
                FocusableInTouchMode = false
            };

            View.AddView(_editText);
            View.Click += View_Click;
            _editText.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            ShowPicker();
        }

        private void ShowPicker()
        {
            var date = VxView.Value?.Value ?? DateTime.Today;
            new DatePickerDialog(View.Context, OnDatePicked, date.Year, date.Month - 1, date.Day).Show();
        }

        private void OnDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            if (e.Date.Date != VxView.Value?.Value)
            {
                VxView.Value?.ValueChanged?.Invoke(e.Date.Date);
            }
        }

        protected override void ApplyProperties(Vx.Views.DatePicker oldView, Vx.Views.DatePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Hint = newView.Header;
            View.Enabled = newView.IsEnabled;
            _editText.Text = newView.Value?.Value?.ToShortDateString() ?? "Select a date";
        }
    }
}