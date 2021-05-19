using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Views
{
    public class InlineDatePicker : EditText
    {
        public event EventHandler<DateTime> DateChanged;

        public InlineDatePicker(Context context) : base(context)
        {
            Initialize();
        }

        public InlineDatePicker(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            Focusable = false;
            Date = DateTime.Today;
            Click += InlineDatePicker_Click;
        }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value.Date)
                {
                    _date = value.Date;

                    Text = _date.ToString("dddd, MMM d");

                    DateChanged?.Invoke(this, _date);
                }
            }
        }

        private void InlineDatePicker_Click(object sender, EventArgs e)
        {
            new DatePickerDialog(Context, OnDatePicked, Date.Year, Date.Month - 1, Date.Day).Show();
        }

        private void OnDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            Date = new DateTime(e.Year, e.Month + 1, e.DayOfMonth);
        }
    }
}