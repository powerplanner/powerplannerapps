using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.Views.View;
using Android.Graphics;
using Android.Util;
using Android.Views.Animations;
using Android.Animation;

namespace InterfacesDroid.Views
{
    public class MyZoomAndPanView : FrameLayout
    {
        /// <summary>
        /// Occurs when the view is about to change
        /// </summary>
        public event EventHandler ViewChanging;

        private GestureDetector _gestureDetector;
        private ScaleGestureDetector _scaleDetector;

        public MyZoomAndPanView(Context context) : base(context)
        {
            Initialize();
        }

        public MyZoomAndPanView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _gestureDetector = new GestureDetector(Context, new MyGestureListener(this));
            _scaleDetector = new ScaleGestureDetector(Context, new MyScaleListener(this));
        }

        public float ViewportWidth => MeasuredWidth;
        public float ViewportHeight => MeasuredHeight;

        public float HorizontalOffset
        {
            get
            {
                if (base.ChildCount > 0)
                {
                    return ConvertTranslationXToHorizontalOffset(base.GetChildAt(0).TranslationX);
                }

                return 0;
            }
        }

        public float VerticalOffset
        {
            get
            {
                if (base.ChildCount > 0)
                {
                    return ConvertTranslationYToVerticalOffset(base.GetChildAt(0).TranslationY);
                }

                return 0;
            }
        }

        private float _unscaledExtentWidth;
        private float _unscaledExtentHeight;

        /// <summary>
        /// The total width of all the scrollable content.
        /// </summary>
        public float ExtentWidth => _unscaledExtentWidth * ZoomFactor;

        /// <summary>
        /// The total height of all the scrollable content.
        /// </summary>
        public float ExtentHeight => _unscaledExtentHeight * ZoomFactor;

        public float ScrollableWidth
        {
            get { return Math.Max(0, ExtentWidth - ViewportWidth); }
        }

        /// <summary>
        /// The vertical size that can be scrolled. ExtentHeight - ViewportHeight.
        /// </summary>
        public float ScrollableHeight
        {
            get { return Math.Max(0, ExtentHeight - ViewportHeight); }
        }

        /// <summary>
        /// Gets a value that indicates the current zoom factor engaged for content scaling. Default value is 1.0, where 1.0 indicates no additional scaling.
        /// </summary>
        public float ZoomFactor { get; private set; } = 1;

        public float MinZoomFactor { get; private set; } = 0.3f;
        public float MaxZoomFactor { get; private set; } = 1.5f;

        public void ChangeView(float? horizontalOffset, float? verticalOffset, float? zoomFactor)
        {
            CancelAnimation();

            ViewChanging?.Invoke(this, new EventArgs());

            if (zoomFactor != null)
            {
                ZoomFactor = Math.Max(MinZoomFactor, Math.Min(MaxZoomFactor, zoomFactor.Value));
            }

            float finalHorizontalOffset = HorizontalOffset;
            if (horizontalOffset != null)
            {
                finalHorizontalOffset = horizontalOffset.Value;
            }
            finalHorizontalOffset = SanitizeHorizontalOffset(finalHorizontalOffset);

            float finalVerticalOffset = VerticalOffset;
            if (verticalOffset != null)
            {
                finalVerticalOffset = verticalOffset.Value;
            }
            finalVerticalOffset = SanitizeVerticalOffset(finalVerticalOffset);

            for (int i = 0; i < base.ChildCount; i++)
            {
                var view = GetChildAt(i);
                view.TranslationX = CalculateTranslationX(finalHorizontalOffset);
                view.TranslationY = CalculateTranslationY(finalVerticalOffset);
                view.ScaleX = ZoomFactor;
                view.ScaleY = ZoomFactor;
            }
        }

        private float CalculateTranslationX(float horizontalOffset)
        {
            return -horizontalOffset - (_unscaledExtentWidth - ZoomFactor * _unscaledExtentWidth) / 2f;
        }

        private float CalculateTranslationY(float verticalOffset)
        {
            return -verticalOffset - (_unscaledExtentHeight - ZoomFactor * _unscaledExtentHeight) / 2f;
        }

        private float ConvertTranslationXToHorizontalOffset(float translationX)
        {
            return -translationX - (_unscaledExtentWidth - ZoomFactor * _unscaledExtentWidth) / 2f;
        }

        private float ConvertTranslationYToVerticalOffset(float translationY)
        {
            return -translationY - (_unscaledExtentHeight - ZoomFactor * _unscaledExtentHeight) / 2f;
        }

        private float SanitizeHorizontalOffset(float desiredHorizontalOffset)
        {
            return Math.Max(0, Math.Min(desiredHorizontalOffset, ScrollableWidth));
        }

        private float SanitizeVerticalOffset(float desiredVerticalOffset)
        {
            return Math.Max(0, Math.Min(desiredVerticalOffset, ScrollableHeight));
        }

        /// <summary>
        /// zoomCenterX and Y are relative to the viewport.
        /// </summary>
        /// <param name="zoomFactor"></param>
        /// <param name="zoomCenterX"></param>
        /// <param name="zoomCenterY"></param>
        public void ChangeZoom(float zoomFactor, float zoomCenterX, float zoomCenterY)
        {
            zoomFactor = Math.Max(MinZoomFactor, Math.Min(MaxZoomFactor, zoomFactor));

            if (ZoomFactor == zoomFactor)
            {
                return;
            }

            float zoomChangeRatio = zoomFactor / ZoomFactor;

            // Make the zoomCenter be relative to the entire extent rather than just the viewport
            zoomCenterX = HorizontalOffset + zoomCenterX;
            zoomCenterY = VerticalOffset + zoomCenterY;

            // First we need to calculate where the new center would be after the scaling
            float untransformedCenterX = zoomCenterX * zoomChangeRatio;
            float untransformedCenterY = zoomCenterY * zoomChangeRatio;

            // Now we need to calculate how far the center moved from where we originally were
            float changeInX = untransformedCenterX - zoomCenterX;
            float changeInY = untransformedCenterY - zoomCenterY;

            // And now we'll change view and zoom to the correct position
            ChangeView(
                horizontalOffset: HorizontalOffset + changeInX,
                verticalOffset: VerticalOffset + changeInY,
                zoomFactor: zoomFactor);
        }

        private List<ObjectAnimator> _currentAnimators = new List<ObjectAnimator>();
        private void CancelAnimation()
        {
            foreach (var a in _currentAnimators)
            {
                a.Cancel();
            }
            _currentAnimators.Clear();
        }

        public void Fling(float velocityX, float velocityY)
        {
            CancelAnimation();

            ViewChanging?.Invoke(this, new EventArgs());

            // Velocity is pixels per second, but it comes in extremely powerful, so we'll downgrade it some
            velocityX = SanitizeFlingVelocity(velocityX);
            velocityY = SanitizeFlingVelocity(velocityY);

            if (velocityX == 0 && velocityY == 0)
            {
                return;
            }

            float calculatedHorizontalOffset = HorizontalOffset - velocityX;
            float calculatedVerticalOffset = VerticalOffset - velocityY;

            float safeHorizontalOffset = SanitizeHorizontalOffset(calculatedHorizontalOffset);
            float safeVerticalOffset = SanitizeVerticalOffset(calculatedVerticalOffset);

            float calculatedTranslationX = CalculateTranslationX(calculatedHorizontalOffset);
            float calculatedTranslationY = CalculateTranslationY(calculatedVerticalOffset);

            float safeTranslationX = CalculateTranslationX(safeHorizontalOffset);
            float safeTranslationY = CalculateTranslationY(safeVerticalOffset);

            for (int i = 0; i < base.ChildCount; i++)
            {
                var view = GetChildAt(i);
                var animator = ObjectAnimator.OfFloat(view, "translationX", calculatedTranslationX);
                animator.SetDuration(1000);
                animator.SetInterpolator(new MyInterpolator(animator, view.TranslationX, calculatedTranslationX, safeTranslationX));
                animator.AnimationCancel += (s, e) =>
                {
                    view.TranslationX = (float)(s as ObjectAnimator).AnimatedValue;
                };
                animator.AnimationEnd += (s, e) =>
                {
                    view.TranslationX = (float)(s as ObjectAnimator).AnimatedValue;
                };
                animator.Start();
                _currentAnimators.Add(animator);

                animator = ObjectAnimator.OfFloat(view, "translationY", calculatedTranslationY);
                animator.SetDuration(1000);
                animator.SetInterpolator(new MyInterpolator(animator, view.TranslationY, calculatedTranslationY, safeTranslationY));
                animator.AnimationCancel += (s, e) =>
                {
                    view.TranslationY = (float)(s as ObjectAnimator).AnimatedValue;
                };
                animator.AnimationEnd += (s, e) =>
                {
                    view.TranslationY = (float)(s as ObjectAnimator).AnimatedValue;
                };
                animator.Start();
                _currentAnimators.Add(animator);
            }
        }

        private class MyInterpolator : DecelerateInterpolator
        {
            private float _maxInterpolatedFraction = 1;

            public MyInterpolator(ObjectAnimator animator, float currentTranslation, float calculatedTranslation, float safeTranslation)
            {
                if (calculatedTranslation != currentTranslation)
                {
                    _maxInterpolatedFraction = (safeTranslation - currentTranslation) / (calculatedTranslation - currentTranslation);
                }
            }

            public override float GetInterpolation(float input)
            {
                float result = base.GetInterpolation(input);

                if (result > _maxInterpolatedFraction)
                {
                    return _maxInterpolatedFraction;
                }

                return result;
            }
        }

        private static float SanitizeFlingVelocity(float velocity)
        {
            // If less than 100, we ignore it
            if (Math.Abs(velocity) < 100)
            {
                return 0;
            }

            // It comes in extremely powerful, so downgrade it some
            return velocity / 2;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            float maxWidth = 0;
            float maxHeight = 0;
            for (int i = 0; i < base.ChildCount; i++)
            {
                var view = base.GetChildAt(i);
                view.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
                maxWidth = Math.Max(maxWidth, view.MeasuredWidth);
                maxHeight = Math.Max(maxHeight, view.MeasuredHeight);
            }

            _unscaledExtentWidth = maxWidth;
            _unscaledExtentHeight = maxHeight;

            SetMeasuredDimension(widthMeasureSpec, heightMeasureSpec);
        }

        private bool _wasScrollOrZooming;
        private bool _didScrollOrZoomFromTouch;
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            _didScrollOrZoomFromTouch = false;
            bool answer = _scaleDetector.OnTouchEvent(e);
            answer = _gestureDetector.OnTouchEvent(e) || answer;

            // If there wasn't a gesture and we were previously scrolling
            if (!_didScrollOrZoomFromTouch && _wasScrollOrZooming)
            {
                switch (e.Action)
                {
                    // If pointer released, stop scrolling
                    case MotionEventActions.Cancel:
                    case MotionEventActions.Up:
                        _wasScrollOrZooming = false;
                        break;
                }
            }

            // Otherwise if there wasn't a gesture and we were NOT previously scrolling
            else if (!_didScrollOrZoomFromTouch && !_wasScrollOrZooming)
            {
                switch (e.Action)
                {
                    case MotionEventActions.Cancel:
                    case MotionEventActions.Down:
                    case MotionEventActions.Up:
                        base.DispatchTouchEvent(e);
                        break;
                }
            }

            // Otherwise if there was a gesture, make sure we're scrolling
            else if (_didScrollOrZoomFromTouch)
            {
                _wasScrollOrZooming = true;
            }

            return answer;
        }

        private class MyGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private MyZoomAndPanView _zoomAndPanView;

            public MyGestureListener(MyZoomAndPanView zoomAndPanView)
            {
                _zoomAndPanView = zoomAndPanView;
            }

            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                // https://developer.android.com/training/gestures/scale.html
                _zoomAndPanView.ChangeView(
                    _zoomAndPanView.HorizontalOffset + distanceX,
                    _zoomAndPanView.VerticalOffset + distanceY,
                    null);
                _zoomAndPanView._didScrollOrZoomFromTouch = true;

                return true;
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                _zoomAndPanView.Fling(velocityX, velocityY);
                return true;
            }
        }

        private class MyScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private MyZoomAndPanView _zoomAndPanView;

            public MyScaleListener(MyZoomAndPanView zoomAndPanView)
            {
                _zoomAndPanView = zoomAndPanView;
            }

            private PointF _viewportFocus = new PointF();
            private float _lastSpan;

            public override bool OnScaleBegin(ScaleGestureDetector detector)
            {
                _lastSpan = detector.CurrentSpan;
                return true;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                float span = detector.CurrentSpan;

                float newFactor = _zoomAndPanView.ZoomFactor * span / _lastSpan;
                _lastSpan = span;

                _zoomAndPanView.ChangeZoom(
                    zoomFactor: newFactor,
                    zoomCenterX: detector.FocusX,
                    zoomCenterY: detector.FocusY);
                _zoomAndPanView._didScrollOrZoomFromTouch = true;

                return true;
            }
        }
    }
}