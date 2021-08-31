using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using InterfacesiOS.Helpers;
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
                Frame = new CGRect(_parentView.ConvertPointToView(_parentView.Frame.Location, null), _parentView.Frame.Size)
            };
            Add(_replicatedParentView);

            _blurEffectView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.Light))
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            Add(_blurEffectView);
            _blurEffectView.StretchWidthAndHeight(View);
            _blurEffectView.AddGestureRecognizer(new UITapGestureRecognizer(Cancel));

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

        protected void Cancel()
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

    public class ImprovedModalDatePickerViewController : ImprovedModalEditViewController<UIView, DateTime>
    {
        public ImprovedModalDatePickerViewController(UIView parentView, DateTime currentDate) : base(parentView, currentDate) { }

        private UIDatePicker _datePicker;

        protected override UIView GenerateControl()
        {
            _datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date,
                Date = BareUIHelper.DateTimeToNSDate(InitialValue)
            };

            if (SdkSupportHelper.IsUIDatePickerInlineStyleSupported)
            {
                _datePicker.PreferredDatePickerStyle = UIDatePickerStyle.Inline;
            }

            // If calendar type (when wheels property was introduced, that's when calendar type appeared)
            if (SdkSupportHelper.IsUIDatePickerWheelsStyleSupported)
            {
                _datePicker.ValueChanged += DatePicker_ValueChanged;
                return _datePicker;
            }

            else
            {
                var container = new UIView();

                var buttonDone = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                buttonDone.SetTitle("Done", UIControlState.Normal);
                buttonDone.TouchUpInside += DatePicker_ValueChanged;

                _datePicker.TranslatesAutoresizingMaskIntoConstraints = false;
                container.Add(_datePicker);
                container.Add(buttonDone);

                _datePicker.StretchWidth(container);
                buttonDone.StretchWidth(container);

                container.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[datePicker]-12-[done]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "datePicker", _datePicker,
                    "done", buttonDone));

                return container;
            }
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            Finish(BareUIHelper.NSDateToDateTime(_datePicker.Date).Date);
        }

        public static Task<ImprovedModalResponse<DateTime>> ShowAsync(UIView parentView, DateTime currentDate)
        {
            return new ImprovedModalDatePickerViewController(parentView, currentDate).ShowAsync();
        }
    }
}
