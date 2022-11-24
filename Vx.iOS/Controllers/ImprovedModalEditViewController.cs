using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using UIKit;

namespace Vx.iOS.Controllers
{
    public class ImprovedModalResponse<A>
    {
        public A Value { get; set; }

        public ImprovedModalResponse(A answer)
        {
            Value = answer;
        }
    }

    public abstract class ImprovedModalEditViewController<V, A> : UIViewController where V : UIView
    {
        private UIVisualEffectView _blurEffectView;
        private UIView _parentView;
        private V _control;
        protected V Control => _control;
        private UIView _controlContainer;
        private UIView _replicatedParentView;
        private TaskCompletionSource<ImprovedModalResponse<A>> _taskCompletionSource = new TaskCompletionSource<ImprovedModalResponse<A>>();
        protected A InitialValue { get; private set; }

        protected virtual nfloat MinControlWidth => 300;

        public ImprovedModalEditViewController(UIView parentView, A initialValue)
        {
            InitialValue = initialValue;
            _parentView = parentView;
            ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            ModalPresentationStyle = UIModalPresentationStyle.Custom;
        }

        public override void ViewDidLoad()
        {
            _replicatedParentView = new UIView
            {
                Hidden = true,
                Frame = _parentView.ConvertRectToView(_parentView.Bounds, null)
            };
            Add(_replicatedParentView);

            _blurEffectView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.Light))
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            Add(_blurEffectView);
            _blurEffectView.StretchWidthAndHeight(View);
            _blurEffectView.AddGestureRecognizer(new UITapGestureRecognizer(Dismiss));

            _controlContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColorCompat.SystemBackgroundColor
            };
            _controlContainer.Layer.CornerRadius = 10;

            _control = GenerateControl();
            _control.TranslatesAutoresizingMaskIntoConstraints = false;
            _controlContainer.Add(_control);
            _control.StretchWidthAndHeight(_controlContainer, 10, 10, 10, 10);

            Add(_controlContainer);

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
            {
                // Required must fit within screen
                _controlContainer.TopAnchor.ConstraintGreaterThanOrEqualTo(View.LayoutMarginsGuide.TopAnchor),
                _controlContainer.BottomAnchor.ConstraintLessThanOrEqualTo(View.LayoutMarginsGuide.BottomAnchor),
                _controlContainer.LeftAnchor.ConstraintGreaterThanOrEqualTo(View.LayoutMarginsGuide.LeftAnchor),
                _controlContainer.RightAnchor.ConstraintLessThanOrEqualTo(View.LayoutMarginsGuide.RightAnchor),
                    
                // Should be at least min width
                Pri(4, NSLayoutConstraint.Create(
                    _controlContainer,
                    NSLayoutAttribute.Width,
                    NSLayoutRelation.GreaterThanOrEqual,
                    1, MinControlWidth)),

                // Optionally pin to left and right, with left being more important to pin to
                Pri(3, _controlContainer.LeftAnchor.ConstraintEqualTo(_replicatedParentView.LeftAnchor)),
                Pri(2, _controlContainer.RightAnchor.ConstraintEqualTo(_replicatedParentView.RightAnchor)),

                // Optionally pin to top, with top being more important to pin to
                Pri(3, _controlContainer.TopAnchor.ConstraintEqualTo(_replicatedParentView.TopAnchor))
            });
        }

        private NSLayoutConstraint Pri(float pri, NSLayoutConstraint c)
        {
            c.Priority = pri;
            return c;
        }

        protected virtual void Dismiss()
        {
            _taskCompletionSource.SetResult(null);
            Close();
        }

        private void Close()
        {
            DismissViewController(true, null);
        }

        protected void Finish(A answer)
        {
            _taskCompletionSource.SetResult(new ImprovedModalResponse<A>(answer));
            Close();
        }

        public Task<ImprovedModalResponse<A>> ShowAsync()
        {
            _parentView.GetViewController().PresentViewController(this, true, null);
            return _taskCompletionSource.Task;
        }

        protected abstract V GenerateControl();
    }

    public class ImprovedModalDatePickerViewController : ImprovedModalEditViewController<UIDatePicker, DateTime>
    {
        public ImprovedModalDatePickerViewController(UIView parentView, DateTime currentDate) : base(parentView, currentDate) { }

        protected override UIDatePicker GenerateControl()
        {
            var datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date,
                Date = BareUIHelper.DateTimeToNSDate(InitialValue)
            };

            if (SdkSupportHelper.IsUIDatePickerInlineStyleSupported)
            {
                datePicker.PreferredDatePickerStyle = UIDatePickerStyle.Inline;
            }

            // If calendar type (when wheels property was introduced, that's when calendar type appeared)
            if (SdkSupportHelper.IsUIDatePickerWheelsStyleSupported)
            {
                datePicker.ValueChanged += DatePicker_ValueChanged;
            }

            return datePicker;
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            Finish(BareUIHelper.NSDateToDateTime(Control.Date).Date);
        }

        protected override void Dismiss()
        {
            Finish(BareUIHelper.NSDateToDateTime(Control.Date).Date);
        }

        public static Task<ImprovedModalResponse<DateTime>> ShowAsync(UIView parentView, DateTime currentDate)
        {
            return new ImprovedModalDatePickerViewController(parentView, currentDate).ShowAsync();
        }
    }

    public class ImprovedModalTimePickerViewController : ImprovedModalEditViewController<UIDatePicker, TimeSpan>
    {
        private TimeSpan _minTime;
        public ImprovedModalTimePickerViewController(UIView parentView, TimeSpan currentTime, TimeSpan minTime) : base(parentView, currentTime)
        {
            _minTime = minTime;
        }

        protected override UIDatePicker GenerateControl()
        {
            var today = DateTime.Today;

            var datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Time,
                Date = BareUIHelper.DateTimeToNSDate(today.Add(InitialValue)),
                MinimumDate = BareUIHelper.DateTimeToNSDate(today.Add(_minTime))
            };

            datePicker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;

            return datePicker;
        }

        protected override void Dismiss()
        {
            Finish(BareUIHelper.NSDateToDateTime(Control.Date).TimeOfDay);
        }

        public static Task<ImprovedModalResponse<DateTime>> ShowAsync(UIView parentView, DateTime currentDate)
        {
            return new ImprovedModalDatePickerViewController(parentView, currentDate).ShowAsync();
        }
    }

    public class ImprovedModalPickerViewController : ImprovedModalEditViewController<UIPickerView, object>
    {
        private IEnumerable _items;
        private Func<object, UIView, UIView> _itemToViewConverter;
        private object _initialValue;
        public ImprovedModalPickerViewController(UIView parentView, IEnumerable items, object initialValue, Func<object, UIView, UIView> itemToViewConverter) : base(parentView, initialValue)
        {
            _items = items;
            _itemToViewConverter = itemToViewConverter;
            _initialValue = initialValue;
        }

        protected override UIPickerView GenerateControl()
        {
            var pickerView = new UIPickerView
            {
            };

            if (_itemToViewConverter != null)
            {
                pickerView.Model = new BareUICustomPickerViewModel(pickerView)
                {
                    ItemsSource = _items,
                    ItemToViewConverter = _itemToViewConverter
                };
            }
            else
            {
                pickerView.Model = new BareUISimplePickerViewModel(pickerView)
                {
                    ItemsSource = _items
                };
            }

            pickerView.Select(Array.FindIndex(_items.OfType<object>().ToArray(), i => i == _initialValue), 0, false);

            return pickerView;
        }

        protected override void Dismiss()
        {
            int selectedIndex = (int)Control.SelectedRowInComponent(0);

            var selectedItem = _items.OfType<object>().ElementAtOrDefault(selectedIndex);
            if (selectedItem != null)
            {
                Finish(selectedItem);
            }
            else
            {
                base.Dismiss();
            }
        }
    }
}
