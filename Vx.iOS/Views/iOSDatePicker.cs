using System;
using System.Threading.Tasks;
using CoreGraphics;
using InterfacesiOS.Helpers;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSDatePicker : iOSView<DatePicker, ManualLayoutView>
    {
        private UILabel _header;
        private ManualLayoutControl _valueContainer;
        private UILabel _value;

        public iOSDatePicker()
        {
            _header = new UILabel();

            View.AddSubview(_header);

            _valueContainer = new ManualLayoutControl
            {
                ClipsToBounds = true,
                BackgroundColor = UIColorCompat.TertiarySystemFillColor
            };
            _valueContainer.Layer.CornerRadius = 10;
            _valueContainer.TouchUpInside += _valueContainer_TouchUpInside;

            _value = new UILabel { Lines = 1 };
            _valueContainer.Add(_value);

            View.AddSubview(_valueContainer);

            View.LayoutAction = LayoutSubviews;
            View.MeasureAction = Measure;
            _valueContainer.LayoutAction = () => _value.Frame = _valueContainer.Bounds.Inset(10, 0);
        }

        private CGSize Measure(CGSize availableSize)
        {
            var headerSize = _header.SizeThatFits(new CGSize(availableSize.Width, UIViewWrapper.UnboundedSize));
            var valueSize = _value.SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, 36));
            var width = availableSize.Width >= UIViewWrapper.UnboundedSize ? Math.Max(headerSize.Width, valueSize.Width + 20) : availableSize.Width;
            return new CGSize(width, headerSize.Height + 40);
        }

        private void LayoutSubviews()
        {
            var headerSize = _header.SizeThatFits(new CGSize(View.Bounds.Width, UIViewWrapper.UnboundedSize));
            _header.Frame = new CGRect(0, 0, View.Bounds.Width, headerSize.Height);
            _valueContainer.Frame = new CGRect(0, headerSize.Height + 4, View.Bounds.Width, 36);
        }

        private async void _valueContainer_TouchUpInside(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            var resp = await new Controllers.ImprovedModalDatePickerViewController(_valueContainer, VxView.Value?.Value ?? DateTime.Today).ShowAsync();

            if (resp != null)
            {
                var newDate = resp.Value;

                if (_value != null)
                {
                    _value.Text = newDate.ToShortDateString();
                }

                if (VxView.Value != null && newDate != VxView.Value.Value)
                {
                    VxView.Value.ValueChanged?.Invoke(newDate);
                }
            }
        }

        protected override void ApplyProperties(DatePicker oldView, DatePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;
            _valueContainer.Alpha = newView.IsEnabled ? 1f : 0.5f;
            View.InvalidateIntrinsicContentSize();
            View.SetNeedsLayout();

            if (newView.Value?.Value != null)
            {
                _value.Text = newView.Value.Value.Value.ToShortDateString();
            }
        }
    }
}
