using System;
using System.Collections.Generic;
using CoreGraphics;
using InterfacesiOS.Helpers;
using UIKit;

namespace Vx.iOS.Controllers
{
    public abstract class ImprovedModalEditViewController<V> : UIViewController where V : UIView
    {
        private UIVisualEffectView _blurEffectView;
        private UIView _parentView;
        private V _control;
        private UIView _controlContainer;
        private UIView _replicatedParentView;

        protected virtual nfloat MinControlWidth => 300;

        public ImprovedModalEditViewController(UIView parentView)
        {
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
            _blurEffectView.AddGestureRecognizer(new UITapGestureRecognizer(Close));

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
                Pri(3, _controlContainer.LeftAnchor.ConstraintEqualTo(_replicatedParentView.LeftAnchor, 0)),
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

        protected void Close()
        {
            DismissViewController(true, null);
        }

        public void Show()
        {
            _parentView.GetViewController().PresentViewController(this, true, null);
        }

        protected abstract V GenerateControl();
    }

    public class ImprovedModalDatePickerViewController : ImprovedModalEditViewController<UIDatePicker>
    {
        public ImprovedModalDatePickerViewController(UIView parentView) : base(parentView) { }

        protected override UIDatePicker GenerateControl()
        {
            var datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date,
                PreferredDatePickerStyle = UIDatePickerStyle.Inline
            };

            datePicker.ValueChanged += DatePicker_ValueChanged;

            return datePicker;
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            Close();
        }
    }
}
