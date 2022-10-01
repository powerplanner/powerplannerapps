using System;
using System.Collections.Generic;
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
                Invalidate();
            }
        }

        private void Invalidate()
        {
            View.InvalidateIntrinsicContentSize();
            View.Superview?.SetNeedsLayout();
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

        private nfloat _preferredMaxLayoutWidth = -1;
        public nfloat PreferredMaxLayoutWidth
        {
            get => _preferredMaxLayoutWidth;
            set
            {
                if (_preferredMaxLayoutWidth != value)
                {
                    if (View is UIPanel panel)
                    {
                        panel.PreferredMaxLayoutWidth = value;
                    }
                    else if (View is UILabel label)
                    {
                        label.PreferredMaxLayoutWidth = value;
                    }

                    _preferredMaxLayoutWidth = value;
                }
            }
        }

        /// <summary>
        /// Automatically includes Margins and Height/Width values
        /// </summary>
        public CGSize IntrinsicContentSize => SizeThatFits(new CGSize(0, 0));

        /// <summary>
        /// Automatically includes Margins and Height/Width values
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public CGSize SizeThatFits(CGSize size)
        {
            // If absolute size
            if (!float.IsNaN(Width) && !float.IsNaN(Height))
            {
                // We're done
                return new CGSize(Width + Margin.Width, Height + Margin.Height);
            }

            // If we're fitting to parent size and parent size isn't auto, just return that, nothing to measure
            if (HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch && VerticalAlignment == Vx.Views.VerticalAlignment.Stretch
                && size.Width != 0 && size.Height != 0)
            {
                return size;
            }

            size = new CGSize(size.Width, size.Height);

            if (HorizontalAlignment != Vx.Views.HorizontalAlignment.Stretch)
            {
                // Auto width
                size.Width = 0;
            }

            if (VerticalAlignment != Vx.Views.VerticalAlignment.Stretch)
            {
                // Auto height
                size.Height = 0;
            }

            // This will include margins
            var innerSize = new CGSize(size.Width, size.Height);

            if (!float.IsNaN(Width))
            {
                innerSize.Width = Width;
            }
            else if (innerSize.Width != 0)
            {
                innerSize.Width = MaxF(0, size.Width - Margin.Width);
            }

            if (!float.IsNaN(Height))
            {
                innerSize.Height = Height;
            }
            else if (innerSize.Height != 0)
            {
                innerSize.Height = MaxF(0, size.Height - Margin.Height);
            }

            innerSize = View.SystemLayoutSizeFittingSize(innerSize);

            // Reset any fixed width/height values
            if (!float.IsNaN(Width))
            {
                innerSize.Width = Width;
            }
            if (!float.IsNaN(Height))
            {
                innerSize.Height = Height;
            }

            return new CGSize(innerSize.Width + Margin.Width, innerSize.Height + Margin.Height);
        }

        private CGRect _frame;
        public CGRect Frame
        {
            get => _frame;
            set
            {
                var contentSize = new CGSize(value.Size.Width, value.Size.Height);

                CGSize innerSize = new CGSize(contentSize);

                if (!float.IsNaN(Width))
                {
                    innerSize.Width = Width;
                }
                else
                {
                    innerSize.Width = MaxF(0, contentSize.Width - Margin.Width);
                }
                if (!float.IsNaN(Height))
                {
                    innerSize.Height = Height;
                }
                else
                {
                    innerSize.Height = MaxF(0, contentSize.Height - Margin.Height);
                }

                bool isWidthKnown = !float.IsNaN(Width) || HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch;
                bool isHeightKnown = !float.IsNaN(Height) || VerticalAlignment == Vx.Views.VerticalAlignment.Stretch;

                if (!isWidthKnown || !isHeightKnown)
                {
                    innerSize = View.SystemLayoutSizeFittingSize(new CGSize(
                        isWidthKnown ? innerSize.Width : 0,
                        isHeightKnown ? innerSize.Height : 0));

                    if (!float.IsNaN(Width))
                    {
                        innerSize.Width = Width;
                    }
                    else if (HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch)
                    {
                        innerSize.Width = MaxF(0, contentSize.Width - Margin.Width);
                    }
                    if (!float.IsNaN(Height))
                    {
                        innerSize.Height = Height;
                    }
                    else if (VerticalAlignment == Vx.Views.VerticalAlignment.Stretch)
                    {
                        innerSize.Height = MaxF(0, contentSize.Height - Margin.Height);
                    }
                }

                nfloat childX, childY;

                switch (HorizontalAlignment)
                {
                    case Vx.Views.HorizontalAlignment.Stretch:
                    case Vx.Views.HorizontalAlignment.Left:
                        childX = value.X + Margin.Left;
                        break;

                    case Vx.Views.HorizontalAlignment.Center:
                        nfloat center = contentSize.Width / 2.0f;
                        nfloat offset = (innerSize.Width + Margin.Left - Margin.Right) / 2.0f;
                        childX = value.X + center - offset;
                        break;

                    case Vx.Views.HorizontalAlignment.Right:
                        childX = value.X + (contentSize.Width - innerSize.Width - Margin.Right);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                switch (VerticalAlignment)
                {
                    case Vx.Views.VerticalAlignment.Stretch:
                    case Vx.Views.VerticalAlignment.Top:
                        childY = value.Y + Margin.Top;
                        break;

                    case Vx.Views.VerticalAlignment.Center:
                        nfloat center = contentSize.Height / 2.0f;
                        nfloat offset = (innerSize.Height + Margin.Top - Margin.Bottom) / 2.0f;
                        childY = value.Y + center - offset;
                        break;

                    case Vx.Views.VerticalAlignment.Bottom:
                        childY = value.Y + (contentSize.Height - innerSize.Height - Margin.Bottom);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                View.Frame = new CGRect(childX, childY, innerSize.Width, innerSize.Height);
            }
        }
    }
}

