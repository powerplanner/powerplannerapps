using System;
using System.Collections.Generic;
using CoreGraphics;
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

    public class UIViewWrapper
    {
        public UIView View { get; private set; }

        private float _width = float.NaN;
        public float Width
        {
            get => _width;
            set => SetValue(ref _width, value);
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
            WrapperConstraint? heightConstraint = null)
        {
            if (widthConstraint != null)
            {
                WidthConstraint = NSLayoutConstraint.Create(
                    View,
                    NSLayoutAttribute.Width,
                    NSLayoutRelation.Equal,
                    widthConstraint.Value.OtherView,
                    widthConstraint.Value.OtherViewAttribute,
                    widthConstraint.Value.Multiplier,
                    widthConstraint.Value.Constant);
            }
            else if (!float.IsNaN(Width))
            {
                WidthConstraint = NSLayoutConstraint.Create(
                    View,
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
                HeightConstraint = NSLayoutConstraint.Create(
                    View,
                    NSLayoutAttribute.Height,
                    NSLayoutRelation.Equal,
                    heightConstraint.Value.OtherView,
                    heightConstraint.Value.OtherViewAttribute,
                    heightConstraint.Value.Multiplier,
                    heightConstraint.Value.Constant);
            }
            else if (!float.IsNaN(Height))
            {
                HeightConstraint = NSLayoutConstraint.Create(
                    View,
                    NSLayoutAttribute.Height,
                    NSLayoutRelation.Equal,
                    1,
                    Height);
            }
            else
            {
                HeightConstraint = null;
            }

            LeftConstraint = leftConstraint.HasValue ? TransformLeftConstraint(leftConstraint.Value) : null;
            TopConstraint = topConstraint.HasValue ? TransformTopConstraint(topConstraint.Value) : null;
            RightConstraint = rightConstraint.HasValue ? TransformRightConstraint(rightConstraint.Value) : null;
            BottomConstraint = bottomConstraint.HasValue ? TransformBottomConstraint(bottomConstraint.Value) : null;

            ApplyHorizontalCenteringConstraintIfNeeded(centeringHorizontalView);
            ApplyVerticalCenteringConstraintIfNeeded(centeringVerticalView);
        }

        private NSLayoutConstraint TransformLeftConstraint(WrapperConstraint constraint)
        {
            bool pinLeft = HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch || HorizontalAlignment == Vx.Views.HorizontalAlignment.Left;

            return NSLayoutConstraint.Create(
                View,
                NSLayoutAttribute.Left,
                pinLeft ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                constraint.OtherView,
                constraint.OtherViewAttribute,
                constraint.Multiplier,
                constraint.Constant + Margin.Left);
        }

        private NSLayoutConstraint TransformRightConstraint(WrapperConstraint constraint)
        {
            bool pinRight = !constraint.GreaterThanOrEqual && (HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch || HorizontalAlignment == Vx.Views.HorizontalAlignment.Right);

            return NSLayoutConstraint.Create(
                constraint.OtherView,
                constraint.OtherViewAttribute,
                pinRight ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                View,
                NSLayoutAttribute.Right,
                constraint.Multiplier,
                Margin.Right + constraint.Constant);
        }

        private NSLayoutConstraint TransformTopConstraint(WrapperConstraint constraint)
        {
            bool pinTop = VerticalAlignment == Vx.Views.VerticalAlignment.Stretch || VerticalAlignment == Vx.Views.VerticalAlignment.Top;

            return NSLayoutConstraint.Create(
                View,
                NSLayoutAttribute.Top,
                pinTop ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                constraint.OtherView,
                constraint.OtherViewAttribute,
                constraint.Multiplier,
                Margin.Top + constraint.Constant);
        }

        private NSLayoutConstraint TransformBottomConstraint(WrapperConstraint constraint)
        {
            bool pinBottom = !constraint.GreaterThanOrEqual && (VerticalAlignment == Vx.Views.VerticalAlignment.Stretch || VerticalAlignment == Vx.Views.VerticalAlignment.Bottom);

            return NSLayoutConstraint.Create(
                constraint.OtherView,
                constraint.OtherViewAttribute,
                pinBottom ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                View,
                NSLayoutAttribute.Bottom,
                constraint.Multiplier,
                Margin.Bottom + constraint.Constant);
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

        private void ApplyVerticalCenteringConstraintIfNeeded(UIView viewForCenter)
        {
            if (viewForCenter != null && VerticalAlignment == Vx.Views.VerticalAlignment.Center)
            {
                CenterYConstraint = NSLayoutConstraint.Create(
                    View,
                    NSLayoutAttribute.CenterY,
                    NSLayoutRelation.Equal,
                    viewForCenter,
                    NSLayoutAttribute.CenterY,
                    1,
                    (Margin.Top - Margin.Bottom) / 2f);
            }
            else
            {
                CenterYConstraint = null;
            }
        }

        private NSLayoutConstraint _topConstraint;
        private NSLayoutConstraint TopConstraint
        {
            get => _topConstraint;
            set => SetConstraint(ref _topConstraint, value);
        }

        private NSLayoutConstraint _rightConstraint;
        private NSLayoutConstraint RightConstraint
        {
            get => _rightConstraint;
            set => SetConstraint(ref _rightConstraint, value);
        }

        private NSLayoutConstraint _leftConstraint;
        private NSLayoutConstraint LeftConstraint
        {
            get => _leftConstraint;
            set => SetConstraint(ref _leftConstraint, value);
        }

        private NSLayoutConstraint _bottomConstraint;
        private NSLayoutConstraint BottomConstraint
        {
            get => _bottomConstraint;
            set => SetConstraint(ref _bottomConstraint, value);
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

        private void SetConstraint(ref NSLayoutConstraint storage, NSLayoutConstraint value)
        {
            // If removing
            if (storage != null && value == null)
            {
                // Simply remove
                View.Superview.RemoveConstraint(storage);
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
                View.Superview.RemoveConstraint(storage);
                storage = null;
            }

            if (value != null)
            {
                View.Superview.AddConstraint(value);
                storage = value;
            }
        }

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

        private Vx.Views.Thickness _margin;
        public Vx.Views.Thickness Margin
        {
            get => _margin;
            set => SetValue(ref _margin, value.AsModified());
        }

        private void SetValue<T>(ref T storage, T value)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                Invalidate();
            }
        }

        private void Invalidate()
        {
            View.Superview?.SetNeedsUpdateConstraints();
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

        public UIViewWrapper(UIView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            View = view;
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
}

