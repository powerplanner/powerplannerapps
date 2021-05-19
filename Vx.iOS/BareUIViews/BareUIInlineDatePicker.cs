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
    public class BareUIInlineDatePicker : BareUIInlineEditView
    {
        public event EventHandler<DateTime?> DateChanged;

        private DateTime? _date;
        public DateTime? Date
        {
            get { return _date; }
            set
            {
                _date = value;
                UpdateDisplayValue();
            }
        }

        private bool _allowsNull = true;
        public bool AllowsNull
        {
            get { return _allowsNull; }
            set
            {
                if (value == false && Date == null)
                {
                    Date = DateTime.Today;
                }

                _allowsNull = value;
            }
        }

        public BareUIInlineDatePicker(UIViewController controller, int left = 0, int right = 0)
            : base(controller, left, right)
        {
            HeaderText = "Date";
            UpdateDisplayValue();
        }

        private void UpdateDisplayValue()
        {
            DisplayValue = Date == null ? "None" : Date.Value.ToString("MMM d, yyyy");
        }

        protected override ModalEditViewController CreateModalEditViewController(UIViewController parent)
        {
            var answer = new ModalDatePickerViewController(HeaderText, parent);
            answer.DatePicker.Mode = UIDatePickerMode.Date;
            return answer;
        }

        protected new ModalDatePickerViewController ModalController => base.ModalController as ModalDatePickerViewController;

        protected override void PrepareModalControllerValues()
        {
            ModalController.DatePicker.Date = BareUIHelper.DateTimeToNSDate(Date != null ? Date.Value : DateTime.Today);
        }

        protected override void UpdateValuesFromModalController()
        {
            var newDate = BareUIHelper.NSDateToDateTime(ModalController.DatePicker.Date).Date;

            if (newDate != Date)
            {
                Date = newDate;
                DateChanged?.Invoke(this, Date);
            }
        }
    }
}