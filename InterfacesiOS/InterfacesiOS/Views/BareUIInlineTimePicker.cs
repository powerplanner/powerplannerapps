using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Controllers;
using ToolsPortable;

namespace InterfacesiOS.Views
{
    public class BareUIInlineTimePicker : BareUIInlineEditView
    {
        public event EventHandler<TimeSpan?> TimeChanged;

        private TimeSpan? _time;
        public TimeSpan? Time
        {
            get { return _time; }
            set
            {
                _time = value;
                UpdateDisplayValue();
            }
        }

        private bool _allowsNull = true;
        public bool AllowsNull
        {
            get { return _allowsNull; }
            set
            {
                if (value == false && Time == null)
                {
                    Time = new TimeSpan(9, 0, 0);
                }

                _allowsNull = value;
            }
        }

        public BareUIInlineTimePicker(UIViewController controller, int left = 0, int right = 0)
            : base (controller, left, right)
        {
            HeaderText = "Time";
            UpdateDisplayValue();
        }

        private void UpdateDisplayValue()
        {
            DisplayValue = Time == null ? "None" : DateTime.Today.Add(Time.Value).ToString("t");
        }

        protected override ModalEditViewController CreateModalEditViewController(UIViewController parent)
        {
            var answer = new ModalDatePickerViewController(HeaderText, parent);
            answer.DatePicker.Mode = UIDatePickerMode.Time;
            return answer;
        }

        protected new ModalDatePickerViewController ModalController => base.ModalController as ModalDatePickerViewController;

        protected override void PrepareModalControllerValues()
        {
            ModalController.DatePicker.Date = BareUIHelper.DateTimeToNSDate(Time != null ? DateTime.Today.Add(Time.Value) : DateTime.Today.AddHours(9));
        }

        protected override void UpdateValuesFromModalController()
        {
            var newTime = BareUIHelper.NSDateToDateTime(ModalController.DatePicker.Date).TimeOfDay;

            if (newTime != Time)
            {
                Time = newTime;
                TimeChanged?.Invoke(this, Time);
            }
        }
    }
}