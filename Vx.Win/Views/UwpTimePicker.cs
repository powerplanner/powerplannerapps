using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Controls.TimePickers;
using Vx.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Vx.Uwp.Views
{
    public class UwpTimePicker : UwpView<Vx.Views.TimePicker, EnhancedTimePicker>
    {
        public UwpTimePicker()
        {
            View.SelectedTimeChanged += View_SelectedTimeChanged;
        }

        private void View_SelectedTimeChanged(object sender, TimeSpan e)
        {
            if (VxView.Value != e)
            {
                VxView.ValueChanged?.Invoke(e);
            }
        }

        protected override void ApplyProperties(TimePicker oldView, TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            if (View.SelectedTime != newView.Value)
            {
                View.SelectedTime = newView.Value;
            }

            View.IsEnabled = newView.IsEnabled;
            View.Header = newView.Header;
        }
    }

    public class UwpEndTimePicker : UwpView<Vx.Views.EndTimePicker, EnhancedEndTimePicker>
    {
        public UwpEndTimePicker()
        {
            View.SelectedTimeChanged += View_SelectedTimeChanged;
        }

        private void View_SelectedTimeChanged(object sender, TimeSpan e)
        {
            if (VxView.Value != e)
            {
                VxView.ValueChanged?.Invoke(e);
            }
        }

        protected override void ApplyProperties(Vx.Views.EndTimePicker oldView, Vx.Views.EndTimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            if (View.SelectedTime != newView.Value)
            {
                View.SelectedTime = newView.Value;
            }

            if (View.StartTime != newView.StartTime)
            {
                View.StartTime = newView.StartTime;
            }

            View.IsEnabled = newView.IsEnabled;
            View.Header = newView.Header;
        }
    }
}
