using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Views
{
    public class UIInlineMultiDayPickerView : BareUIInlineEditView
    {
        public event EventHandler<DayOfWeek[]> SelectionsChanged;

        private DayOfWeek[] _selectedDays = new DayOfWeek[0];
        public DayOfWeek[] SelectedDays
        {
            get { return _selectedDays; }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException(nameof(SelectedDays));
                }

                _selectedDays = value.OrderBy(i => i).ToArray();
                UpdateDisplayValue();
            }
        }

        public UIInlineMultiDayPickerView(UIViewController controller, int left = 0, int right = 0)
            : base(controller, left, right)
        {
            HeaderText = "Days";
            UpdateDisplayValue();
        }

        private void UpdateDisplayValue()
        {
            if (SelectedDays.Length == 0)
            {
                DisplayValue = "None";
            }
            else if (SelectedDays.Length == 1)
            {
                DisplayValue = DateTools.ToLocalizedString(SelectedDays[0]);
            }
            else if (SelectedDays.Length == 2)
            {
                DisplayValue = string.Join(", ", GetDaysSummarized(SelectedDays, 3));
            }
            else
            {
                DisplayValue = string.Join(", ", GetDaysSummarized(SelectedDays, 2));
            }
        }

        private static string GetDaysSummarized(DayOfWeek[] days, int lengthOfEachDay)
        {
            return string.Join(", ", days.Select(i => StringTools.TrimLength(DateTools.ToLocalizedString(i), lengthOfEachDay)));
        }

        private Tuple<DayOfWeek, BareUISwitch>[] _switches;

        protected override ModalEditViewController CreateModalEditViewController(UIViewController parent)
        {
            var scrollView = new UIScrollView()
            {
                // Note that only height matters, the width values will be overwritten
                Frame = new CoreGraphics.CGRect(0, 0, 0, 340)
            };

            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical
            };
            scrollView.AddSubview(stackView);
            stackView.ConfigureForVerticalScrolling(scrollView, top: 8, bottom: 16);

            _switches = new Tuple<DayOfWeek, BareUISwitch>[7];

            DayOfWeek day = DayOfWeek.Monday;
            for (int i = 0; i < _switches.Length; i++, day = (DayOfWeek)(((int)day + 1) % 7))
            {
                var daySwitch = new BareUISwitch()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Header = DateTools.ToLocalizedString(day)
                };
                _switches[i] = new Tuple<DayOfWeek, BareUISwitch>(day, daySwitch);
                stackView.AddArrangedSubview(daySwitch);
                daySwitch.StretchWidth(stackView);
                daySwitch.SetHeight(44);

                if (i != _switches.Length - 1)
                {
                    stackView.AddDivider();
                }
            }

            return new ModalEditViewController(scrollView, HeaderText, parent);
        }

        protected override void PrepareModalControllerValues()
        {
            foreach (var pair in _switches)
            {
                pair.Item2.Switch.On = SelectedDays.Contains(pair.Item1);
            }
        }

        protected override void UpdateValuesFromModalController()
        {
            List<DayOfWeek> newSelectedDays = new List<DayOfWeek>();
            foreach (var pair in _switches)
            {
                if (pair.Item2.Switch.On)
                {
                    newSelectedDays.Add(pair.Item1);
                }
            }

            newSelectedDays.Sort();

            if (!newSelectedDays.SequenceEqual(SelectedDays))
            {
                SelectedDays = newSelectedDays.ToArray();
                SelectionsChanged?.Invoke(this, SelectedDays);
            }
        }
    }
}