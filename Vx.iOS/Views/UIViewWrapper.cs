using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Vx.iOS.Views
{
    public struct WrapperConstraint
    {
        public WrapperConstraint(UIView otherView, NSLayoutAttribute otherViewAttribute, nfloat multiplier, nfloat constant)
        {
            OtherView = otherView;
            OtherViewAttribute = otherViewAttribute;
            Multiplier = multiplier;
            Constant = constant;
            GreaterThanOrEqual = false;
        }

        public WrapperConstraint(UIView otherView, NSLayoutAttribute otherViewAttribute)
        {
            OtherView = otherView;
            OtherViewAttribute = otherViewAttribute;
            Multiplier = 1;
            Constant = 0;
            GreaterThanOrEqual = false;
        }

        public UIView OtherView;
        public NSLayoutAttribute OtherViewAttribute;
        public nfloat Multiplier;
        public nfloat Constant;
        public bool GreaterThanOrEqual;
    }

    public class UIGuideWrapper : UIViewOrGuideWrapper
    {
        public UIGuideWrapper(UILayoutGuide guide, UIView superview)
        {
            ViewOrGuide = guide;
            Superview = superview;
        }

        public UILayoutGuide Guide => ViewOrGuide as UILayoutGuide;
        public override UIView Superview { get; }
    }

    public abstract class UIViewOrGuideWrapper
    {
        public NSObject ViewOrGuide { get; protected set; }
        public abstract UIView Superview { get; }

        private float _width = float.NaN;
        public float Width
        {
            get => _width;
            set => SetValue(ref _width, value);
        }

        public void SetConstraints(
            WrapperConstraint? leftConstraint,
            WrapperConstraint? topConstraint,
            WrapperConstraint? rightConstraint,
            WrapperConstraint? bottomConstraint)
        {
            LeftConstraint = leftConstraint.HasValue ? TransformLeftConstraint(leftConstraint.Value) : null;
            TopConstraint = topConstraint.HasValue ? TransformTopConstraint(topConstraint.Value) : null;
            RightConstraint = rightConstraint.HasValue ? TransformRightConstraint(rightConstraint.Value) : null;
            BottomConstraint = bottomConstraint.HasValue ? TransformBottomConstraint(bottomConstraint.Value) : null;
        }

        private NSLayoutConstraint TransformLeftConstraint(WrapperConstraint constraint)
        {
            return NSLayoutConstraint.Create(
                ViewOrGuide,
                NSLayoutAttribute.Left,
                PinLeft && !constraint.GreaterThanOrEqual ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                constraint.OtherView,
                constraint.OtherViewAttribute,
                constraint.Multiplier,
                constraint.Constant + Margin.Left);
        }

        private NSLayoutConstraint TransformRightConstraint(WrapperConstraint constraint)
        {
            return NSLayoutConstraint.Create(
                constraint.OtherView,
                constraint.OtherViewAttribute,
                PinRight && !constraint.GreaterThanOrEqual ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                ViewOrGuide,
                NSLayoutAttribute.Right,
                constraint.Multiplier,
                Margin.Right + constraint.Constant);
        }

        private NSLayoutConstraint TransformTopConstraint(WrapperConstraint constraint)
        {
            return NSLayoutConstraint.Create(
                ViewOrGuide,
                NSLayoutAttribute.Top,
                PinTop && !constraint.GreaterThanOrEqual ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                constraint.OtherView,
                constraint.OtherViewAttribute,
                constraint.Multiplier,
                Margin.Top + constraint.Constant);
        }

        private NSLayoutConstraint TransformBottomConstraint(WrapperConstraint constraint)
        {
            return NSLayoutConstraint.Create(
                constraint.OtherView,
                constraint.OtherViewAttribute,
                PinBottom && !constraint.GreaterThanOrEqual ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                ViewOrGuide,
                NSLayoutAttribute.Bottom,
                constraint.Multiplier,
                Margin.Bottom + constraint.Constant);
        }

        private NSLayoutConstraint _topConstraint;
        protected NSLayoutConstraint TopConstraint
        {
            get => _topConstraint;
            set => SetConstraint(ref _topConstraint, value);
        }

        private NSLayoutConstraint _rightConstraint;
        protected NSLayoutConstraint RightConstraint
        {
            get => _rightConstraint;
            set => SetConstraint(ref _rightConstraint, value);
        }

        private NSLayoutConstraint _leftConstraint;
        protected NSLayoutConstraint LeftConstraint
        {
            get => _leftConstraint;
            set => SetConstraint(ref _leftConstraint, value);
        }

        private NSLayoutConstraint _bottomConstraint;
        protected NSLayoutConstraint BottomConstraint
        {
            get => _bottomConstraint;
            set => SetConstraint(ref _bottomConstraint, value);
        }

        private Vx.Views.Thickness _margin;
        public Vx.Views.Thickness Margin
        {
            get => _margin;
            set => SetValue(ref _margin, value.AsModified());
        }

        protected virtual bool PinLeft => true;
        protected virtual bool PinTop => true;
        protected virtual bool PinRight => true;
        protected virtual bool PinBottom => true;

        protected void SetConstraint(ref NSLayoutConstraint storage, NSLayoutConstraint value)
        {
            // If removing
            if (storage != null && value == null)
            {
                // Simply remove
                Superview.RemoveConstraint(storage);
                storage = null;
                return;
            }

            if (storage != null)
            {
                // If identical
                if (object.ReferenceEquals(storage.FirstItem, value.FirstItem)
                    && storage.FirstAttribute == value.FirstAttribute
                    && storage.Relation == value.Relation
                    && object.ReferenceEquals(storage.SecondItem, value.SecondItem)
                    && storage.SecondAttribute == value.SecondAttribute
                    && storage.Multiplier == value.Multiplier
                    && storage.Constant == value.Constant)
                {
                    // No need to update
                    return;
                }

                // Otherwise we need to remove existing constraint to update it
                Superview.RemoveConstraint(storage);
                storage = null;
            }

            if (value != null)
            {
                Superview.AddConstraint(value);
                storage = value;
            }
        }

        protected void SetValue<T>(ref T storage, T value)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                Invalidate();
            }
        }

        private void Invalidate()
        {
            Superview?.SetNeedsUpdateConstraints();
        }

        private Dictionary<string, object> _attachedValues;

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            if (_attachedValues == null)
            {
                return defaultValue;
            }

            return (T)_attachedValues.GetValueOrDefault(key, defaultValue);
        }

        public void SetValue(string key, object val)
        {
            if (_attachedValues == null)
            {
                _attachedValues = new Dictionary<string, object>();
            }

            _attachedValues[key] = val;
            Invalidate();
        }

        protected static nfloat MaxF(nfloat f1, nfloat f2)
        {
            if (f1 > f2)
            {
                return f1;
            }
            return f2;
        }

        protected static nfloat MinF(nfloat f1, nfloat f2)
        {
            if (f1 < f2)
            {
                return f1;
            }
            return f2;
        }
    }

    public class UIViewWrapper : UIViewOrGuideWrapper
    {
        public UIView View => ViewOrGuide as UIView;
        public override UIView Superview => View.Superview;

        public void RemoveFromSuperview()
        {
            View.RemoveFromSuperview();

            if (_centerYGuide != null)
            {
                DisposeCenterYGuide();
            }
        }

#if DEBUG
        public string DebugConstraintsString
        {
            get
            {
                return string.Join("\n", DebugDumpConstraints("Left", LeftConstraint), DebugDumpConstraints("Top", TopConstraint), DebugDumpConstraints("Right", RightConstraint), DebugDumpConstraints("Bottom", BottomConstraint), DebugDumpConstraints("Width", WidthConstraint), DebugDumpConstraints("Height", HeightConstraint), DebugDumpConstraints("CenterX", CenterXConstraint), DebugDumpConstraints("CenterY", CenterYConstraint));
            }
        }

        public static string DebugDumpConstraints(string name, NSLayoutConstraint constraint)
        {
            if (constraint == null)
            {
                return $"{name}: null";
            }

            return $"{name}: {constraint.FirstAttribute} {constraint.FirstItem} {constraint.Relation} {constraint.Multiplier}x {constraint.SecondAttribute} {constraint.SecondItem} + {constraint.Constant}";
        }
#endif

        public void SetConstraints(
            WrapperConstraint? leftConstraint,
            WrapperConstraint? topConstraint,
            WrapperConstraint? rightConstraint,
            WrapperConstraint? bottomConstraint,
            UIView centeringHorizontalView,
            UIView centeringVerticalView,
            WrapperConstraint? widthConstraint = null,
            WrapperConstraint? heightConstraint = null,
            bool centerViaLayoutGuideIfNeeded = false)
        {
            base.SetConstraints(
                leftConstraint,
                topConstraint,
                rightConstraint,
                bottomConstraint);

            if (widthConstraint != null)
            {
                if (widthConstraint.Value.OtherView != null)
                {
                    WidthConstraint = NSLayoutConstraint.Create(
                        ViewOrGuide,
                        NSLayoutAttribute.Width,
                        NSLayoutRelation.Equal,
                        widthConstraint.Value.OtherView,
                        widthConstraint.Value.OtherViewAttribute,
                        widthConstraint.Value.Multiplier,
                        widthConstraint.Value.Constant);
                }
                else
                {
                    WidthConstraint = NSLayoutConstraint.Create(
                        ViewOrGuide,
                        NSLayoutAttribute.Width,
                        NSLayoutRelation.Equal,
                        widthConstraint.Value.Multiplier,
                        widthConstraint.Value.Constant);
                }
            }
            else if (!float.IsNaN(Width))
            {
                WidthConstraint = NSLayoutConstraint.Create(
                    ViewOrGuide,
                    NSLayoutAttribute.Width,
                    NSLayoutRelation.Equal,
                    1,
                    Width);
            }
            else
            {
                WidthConstraint = null;
            }

            if (heightConstraint != null)
            {
                if (heightConstraint.Value.OtherView != null)
                {
                    HeightConstraint = NSLayoutConstraint.Create(
                        ViewOrGuide,
                        NSLayoutAttribute.Height,
                        NSLayoutRelation.Equal,
                        heightConstraint.Value.OtherView,
                        heightConstraint.Value.OtherViewAttribute,
                        heightConstraint.Value.Multiplier,
                        heightConstraint.Value.Constant);
                }
                else
                {
                    HeightConstraint = NSLayoutConstraint.Create(
                        ViewOrGuide,
                        NSLayoutAttribute.Height,
                        NSLayoutRelation.Equal,
                        heightConstraint.Value.Multiplier,
                        heightConstraint.Value.Constant);
                }
            }
            else if (!float.IsNaN(Height))
            {
                HeightConstraint = NSLayoutConstraint.Create(
                    ViewOrGuide,
                    NSLayoutAttribute.Height,
                    NSLayoutRelation.Equal,
                    1,
                    Height);
            }
            else
            {
                HeightConstraint = null;
            }


            ApplyHorizontalCenteringConstraintIfNeeded(centeringHorizontalView);
            ApplyVerticalCenteringConstraintIfNeeded(centeringVerticalView, leftConstraint, topConstraint, rightConstraint, bottomConstraint, centerViaLayoutGuideIfNeeded);
        }

        private void ApplyHorizontalCenteringConstraintIfNeeded(UIView viewForCenter)
        {
            if (viewForCenter != null && HorizontalAlignment == Vx.Views.HorizontalAlignment.Center)
            {
                CenterXConstraint = NSLayoutConstraint.Create(
                    View,
                    NSLayoutAttribute.CenterX,
                    NSLayoutRelation.Equal,
                    viewForCenter,
                    NSLayoutAttribute.CenterX,
                    1,
                    (Margin.Left - Margin.Right) / 2f);
            }
            else
            {
                CenterXConstraint = null;
            }
        }

        private void ApplyVerticalCenteringConstraintIfNeeded(UIView viewForCenter,
            WrapperConstraint? leftConstraint,
            WrapperConstraint? topConstraint,
            WrapperConstraint? rightConstraint,
            WrapperConstraint? bottomConstraint,
            bool centerViaLayoutGuideIfNeeded)
        {
            if (VerticalAlignment == Vx.Views.VerticalAlignment.Center)
            {
                NSObject objForCenter;

                if (viewForCenter != null)
                {
                    objForCenter = viewForCenter;
                    DisposeCenterYGuide();
                }
                else if (centerViaLayoutGuideIfNeeded)
                {
                    // The layout guide mostly works, doesn't work if we have a weighted view and then another view afterwards
                    if (_centerYGuide == null)
                    {
                        var guide = new UILayoutGuide();
                        _centerYGuide = new UIGuideWrapper(guide, View.Superview);
                        View.Superview.AddLayoutGuide(guide);
                    }

                    _centerYGuide.SetConstraints(leftConstraint, topConstraint, rightConstraint, bottomConstraint);
                    objForCenter = _centerYGuide.ViewOrGuide;
                }
                else
                {
                    CenterYConstraint = null;
                    DisposeCenterYGuide();
                    return;
                }

                CenterYConstraint = NSLayoutConstraint.Create(
                    View,
                    NSLayoutAttribute.CenterY,
                    NSLayoutRelation.Equal,
                    objForCenter,
                    NSLayoutAttribute.CenterY,
                    1,
                    (Margin.Top - Margin.Bottom) / 2f);
            }
            else
            {
                CenterYConstraint = null;
                DisposeCenterYGuide();
            }
        }

        private void DisposeCenterYGuide()
        {
            if (_centerYGuide != null)
            {
                _centerYGuide.Superview.RemoveLayoutGuide(_centerYGuide.Guide);
                _centerYGuide = null;
            }
        }

        private NSLayoutConstraint _centerXConstraint;
        private NSLayoutConstraint CenterXConstraint
        {
            get => _centerXConstraint;
            set => SetConstraint(ref _centerXConstraint, value);
        }

        private NSLayoutConstraint _centerYConstraint;
        private NSLayoutConstraint CenterYConstraint
        {
            get => _centerYConstraint;
            set => SetConstraint(ref _centerYConstraint, value);
        }

        private NSLayoutConstraint _widthConstraint;
        private NSLayoutConstraint WidthConstraint
        {
            get => _widthConstraint;
            set => SetConstraint(ref _widthConstraint, value);
        }

        private NSLayoutConstraint _heightConstraint;
        private NSLayoutConstraint HeightConstraint
        {
            get => _heightConstraint;
            set => SetConstraint(ref _heightConstraint, value);
        }

        private UIGuideWrapper _centerYGuide;

        private float _height = float.NaN;
        public float Height
        {
            get => _height;
            set => SetValue(ref _height, value);
        }

        private Vx.Views.HorizontalAlignment _horizontalAlignment;
        public Vx.Views.HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set => SetValue(ref _horizontalAlignment, value);
        }

        private Vx.Views.VerticalAlignment _verticalAlignment;
        public Vx.Views.VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set => SetValue(ref _verticalAlignment, value);
        }

        protected override bool PinTop => VerticalAlignment == Vx.Views.VerticalAlignment.Stretch || VerticalAlignment == Vx.Views.VerticalAlignment.Top;
        protected override bool PinLeft => HorizontalAlignment == Vx.Views.HorizontalAlignment.Left || HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch;
        protected override bool PinRight => HorizontalAlignment == Vx.Views.HorizontalAlignment.Right || HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch;
        protected override bool PinBottom => VerticalAlignment == Vx.Views.VerticalAlignment.Bottom || VerticalAlignment == Vx.Views.VerticalAlignment.Stretch;

        public UIViewWrapper(UIView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            ViewOrGuide = view;
        }
    }
}

