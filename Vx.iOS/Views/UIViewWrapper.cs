using System;
using CoreGraphics;
using UIKit;

namespace Vx.iOS.Views
{
    public class UIViewWrapper
    {
        public UIView View { get; private set; }

        private float _width = float.NaN;
        public float Width
        {
            get => _width;
            set => SetValue(ref _width, value);
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
                View.Superview?.InvalidateIntrinsicContentSize();
                View.Superview?.SetNeedsLayout();
            }
        }

        public UIViewWrapper(UIView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            View = view;
        }

        public CGSize IntrinsicContentSize
        {
            get
            {
                var size = View.IntrinsicContentSize;
                var margin = Margin;
                if (size.Width != UIView.NoIntrinsicMetric)
                {
                    size.Width += margin.Width;
                }
                if (size.Height != UIView.NoIntrinsicMetric)
                {
                    size.Height += margin.Height;
                }

                return size;
            }
        }

        /// <summary>
        /// Includes margins
        /// </summary>
        public CGSize DesiredSize { get; private set; }

        public void Measure(CGSize availableSize)
        {
            var width = availableSize.Width;
            var height = availableSize.Height;

            // If width/height are explicitly set, use those (even if bigger)
            if (!float.IsNaN(Width))
            {
                width = Width;
            }
            else
            {
                // Otherwise, accomodate for margins
                width = MaxF(availableSize.Width - Margin.Width, 0);
            }

            if (!float.IsNaN(Height))
            {
                height = Height;
            }
            else
            {
                height = MaxF(availableSize.Height - Margin.Height, 0);
            }

            // Returns a size without any margins
            var size = MeasureOverride(new CGSize(width, height));

            width = size.Width;
            height = size.Height;

            if (!float.IsNaN(Width))
            {
                width = Width;
            }
            if (!float.IsNaN(Height))
            {
                height = Height;
            }

            width += Margin.Width;
            height += Margin.Height;

            DesiredSize = new CGSize(MinF(width, availableSize.Width), MinF(height, availableSize.Height));
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

        /// <summary>
        /// Extending classes can implement this to custom measure. Margins have already been accomodated for (ignore margins when calculating). Ignore the Width/Height values too.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected virtual CGSize MeasureOverride(CGSize availableSize)
        {
            return View.SystemLayoutSizeFittingSize(availableSize);
        }

        public void Arrange(CGPoint pos, CGSize finalSize)
        {
            nfloat width, height; // Width/height without margins
            var margin = Margin;

            if (!float.IsNaN(Width))
            {
                width = Width;
            }
            else
            {
                switch (HorizontalAlignment)
                {
                    case Vx.Views.HorizontalAlignment.Stretch:
                        width = finalSize.Width - margin.Width;
                        break;

                    default:
                        if (DesiredSize.Width > finalSize.Width)
                        {
                            width = finalSize.Width - margin.Width;
                        }
                        else
                        {
                            width = DesiredSize.Width - margin.Width;
                        }
                        break;
                }
            }

            if (!float.IsNaN(Height))
            {
                height = Height;
            }
            else
            {
                switch (VerticalAlignment)
                {
                    case Vx.Views.VerticalAlignment.Stretch:
                        height = finalSize.Height - margin.Height;
                        break;

                    default:
                        if (DesiredSize.Height > finalSize.Height)
                        {
                            height = finalSize.Height - margin.Height;
                        }
                        else
                        {
                            height = DesiredSize.Height - Margin.Height;
                        }
                        break;
                }
            }

            // Use final size
            var arrangedSize = ArrangeOverride(new CGSize(width, height));
            width = arrangedSize.Width;
            height = arrangedSize.Height;

            nfloat childX, childY;

            switch (HorizontalAlignment)
            {
                case Vx.Views.HorizontalAlignment.Stretch:
                case Vx.Views.HorizontalAlignment.Left:
                    childX = pos.X + margin.Left;
                    break;

                case Vx.Views.HorizontalAlignment.Center:
                    nfloat center = (finalSize.Width + margin.Left - margin.Right) / 2.0f;
                    nfloat offset = width / 2.0f;
                    childX = pos.X + center - offset;
                    break;

                case Vx.Views.HorizontalAlignment.Right:
                    childX = pos.X + (finalSize.Width - width - margin.Right);
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (VerticalAlignment)
            {
                case Vx.Views.VerticalAlignment.Stretch:
                case Vx.Views.VerticalAlignment.Top:
                    childY = pos.Y + margin.Top;
                    break;

                case Vx.Views.VerticalAlignment.Center:
                    nfloat center = (finalSize.Height + margin.Top - margin.Bottom) / 2.0f;
                    nfloat offset = height / 2.0f;
                    childY = pos.Y + center - offset;
                    break;

                case Vx.Views.VerticalAlignment.Bottom:
                    childY = pos.Y + (finalSize.Height - height - margin.Bottom);
                    break;

                default:
                    throw new NotImplementedException();
            }

            View.Frame = new CGRect(childX, childY, width, height);
        }

        protected virtual CGSize ArrangeOverride(CGSize finalSize)
        {
            return finalSize;
        }
    }
}

