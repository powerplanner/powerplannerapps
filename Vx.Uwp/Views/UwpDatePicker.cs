using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpDatePicker : UwpView<Vx.Views.DatePicker, CalendarDatePicker>
    {
        public UwpDatePicker()
        {
            View.DateChanged += View_DateChanged;
        }

        private void View_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (VxView.Value?.ValueChanged != null && VxView.Value?.Value != args.NewDate?.DateTime)
            {
                VxView.Value.ValueChanged(args.NewDate?.DateTime);
            }
        }

        protected override void ApplyProperties(Vx.Views.DatePicker oldView, Vx.Views.DatePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Value?.Value != View.Date?.DateTime)
            {
                View.Date = newView?.Value?.Value;
            }

            View.IsEnabled = newView.IsEnabled;
            View.Header = newView.Header;
        }
    }
}
