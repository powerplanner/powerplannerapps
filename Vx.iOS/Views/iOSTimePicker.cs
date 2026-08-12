using System;
using CoreGraphics;
using InterfacesiOS.Helpers;
using UIKit;
using Vx.Extensions;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTimePicker : iOSView<Vx.Views.TimePicker, ManualLayoutView>
    {
        private UILabel _header;
        private ManualLayoutControl _valueContainer;
        private UILabel _value;

        public iOSTimePicker()
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

            var minTime = VxView is EndTimePicker endTimePicker ? endTimePicker.StartTime : new TimeSpan();
            var resp = await new Controllers.ImprovedModalTimePickerViewController(_valueContainer, VxView.Value?.Value ?? new TimeSpan(DateTime.Today.Hour, 0, 0), minTime).ShowAsync();

            if (resp != null)
            {
                var newTime = resp.Value;

                if (_value != null)
                {
                    UpdateText(newTime);
                }

                if (VxView.Value != null && newTime != VxView.Value.Value)
                {
                    VxView.Value.ValueChanged?.Invoke(newTime);
                }
            }
        }

        protected override void ApplyProperties(TimePicker oldView, TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;
            _valueContainer.Alpha = newView.IsEnabled ? 1f : 0.5f;
            View.InvalidateIntrinsicContentSize();
            View.SetNeedsLayout();

            if (newView.Value?.Value != null)
            {
                UpdateText(newView.Value.Value);
            }
        }

        private void UpdateText(TimeSpan timeSpan)
        {
            _value.Text = DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(timeSpan));
        }
    }
}
