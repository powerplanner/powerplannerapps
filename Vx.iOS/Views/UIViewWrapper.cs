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
        public CGSize MeasuredSize { get; private set; }

        public CGSize Measure(CGSize availableSize)
        {
            if (!float.IsNaN(Width) && !float.IsNaN(Height))
            {
                MeasuredSize = new CGSize(Width + Margin.Width, Height + Margin.Height);
                return MeasuredSize;
            }

            if (!float.IsNaN(Width))
            {
                availableSize = new CGSize(Width + Margin.Width, availableSize.Height);
            }
            else if (!float.IsNaN(Height))
            {
                availableSize = new CGSize(availableSize.Width, Height + Margin.Height);
            }

            var contentSize = View.SystemLayoutSizeFittingSize(new CGSize(availableSize.Width - Margin.Width, availableSize.Height - Margin.Height));
            MeasuredSize = new CGSize(contentSize.Width + Margin.Width, contentSize.Height + Margin.Height);

            if (!float.IsNaN(Width))
            {
                MeasuredSize = new CGSize(Width + Margin.Width, MeasuredSize.Height);
            }
            else if (!float.IsNaN(Height))
            {
                MeasuredSize = new CGSize(MeasuredSize.Width, Height + Margin.Height);
            }

            return MeasuredSize;
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
                        if (MeasuredSize.Width > finalSize.Width)
                        {
                            width = finalSize.Width - margin.Width;
                        }
                        else
                        {
                            width = MeasuredSize.Width - margin.Width;
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
                        if (MeasuredSize.Height > finalSize.Height)
                        {
                            height = finalSize.Height - margin.Height;
                        }
                        else
                        {
                            height = MeasuredSize.Height - Margin.Height;
                        }
                        break;
                }
            }

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
    }
}

