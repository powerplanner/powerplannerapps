using System;
using System.Linq;
using CoreGraphics;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using ToolsPortable;
using UIKit;
using Vx.iOS.BareUIViews;
using Vx.iOS.Controllers;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSComboBox : iOSView<Vx.Views.ComboBox, ManualLayoutView>
    {
        private UILabel _header;
        private ManualLayoutView _valueContainer;
        private UILabel _value;
        private INativeComponent _valueTemplated;

        public iOSComboBox()
        {
            _header = new UILabel();

            _valueContainer = new ManualLayoutView();
            _valueContainer.BackgroundColor = UIColorCompat.TertiarySystemFillColor;
            _valueContainer.ClipsToBounds = true;
            _valueContainer.Layer.CornerRadius = 10;

            View.Add(_header);
            View.Add(_valueContainer);

            View.LayoutAction = LayoutSubviews;
            View.MeasureAction = Measure;
            _valueContainer.LayoutAction = LayoutValueContainer;

            // Handle clicks
            UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer();
            tapRecognizer.AddTarget(ShowOptions);
            View.AddGestureRecognizer(tapRecognizer);
        }

        private CGSize Measure(CGSize availableSize)
        {
            var headerSize = _header.SizeThatFits(new CGSize(availableSize.Width, UIViewWrapper.UnboundedSize));
            var valueView = _valueTemplated as UIView ?? _value;
            var valueSize = valueView?.SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, 36)) ?? CGSize.Empty;
            var width = availableSize.Width >= UIViewWrapper.UnboundedSize ? Math.Max(headerSize.Width, valueSize.Width + 20) : availableSize.Width;
            return new CGSize(width, headerSize.Height + 40);
        }

        private void LayoutSubviews()
        {
            var headerSize = _header.SizeThatFits(new CGSize(View.Bounds.Width, UIViewWrapper.UnboundedSize));
            _header.Frame = new CGRect(0, 0, View.Bounds.Width, headerSize.Height);
            _valueContainer.Frame = new CGRect(0, headerSize.Height + 4, View.Bounds.Width, 36);
        }

        private void LayoutValueContainer()
        {
            var content = _valueTemplated as UIView ?? _value;
            if (content != null)
            {
                content.Frame = _valueContainer.Bounds.Inset(10, 0);
            }
        }

        protected override void ApplyProperties(ComboBox oldView, ComboBox newView)
        {
            _header.Text = newView.Header;
            _valueContainer.Alpha = newView.IsEnabled ? 1f : 0.5f;

            if (newView.ItemTemplate != null)
            {
                if (_value != null)
                {
                    _value.RemoveFromSuperview();
                    _value = null;
                }

                if (DataTemplateHelper.ProcessAndIsNewComponent(newView.SelectedItem?.Value, newView.ItemTemplate, _valueTemplated, out VxComponent newComponent))
                {
                    _valueTemplated = newComponent.Render();
                    _valueContainer.Add(_valueTemplated as UIView);
                    _valueContainer.SetNeedsLayout();
                }
            }
            else
            {
                if (_valueTemplated != null)
                {
                    (_valueTemplated as UIView).RemoveFromSuperview();
                    _valueTemplated = null;
                }

                if (_value == null)
                {
                    _value = new UILabel { Lines = 1 };
                    _valueContainer.Add(_value);
                    _valueContainer.SetNeedsLayout();
                }

                _value.Text = newView.SelectedItem?.Value?.ToString();
            }

            View.InvalidateIntrinsicContentSize();
            View.SetNeedsLayout();

            base.ApplyProperties(oldView, newView);
        }

        private async void ShowOptions()
        {
            if (!VxView.IsEnabled || !VxView.Items.GetEnumerator().MoveNext())
            {
                return;
            }

            var response = await new ImprovedModalPickerViewController(_valueContainer, VxView.Items, VxView.SelectedItem?.Value, VxView.ItemTemplate != null ? ConvertItemToView : null as Func<object, UIView, UIView>).ShowAsync();

            if (response != null)
            {
                if (response.Value != VxView.SelectedItem?.Value && VxView.SelectedItem?.ValueChanged != null)
                {
                    VxView.SelectedItem.ValueChanged(response.Value);
                }
            }
        }

        private UIView ConvertItemToView(object item, UIView recycledView)
        {
            if (DataTemplateHelper.ProcessAndIsNewComponent(item, VxView.ItemTemplate, (recycledView as BareUICenteredView)?.Content as INativeComponent, out VxComponent newComponent))
            {
                return new BareUICenteredView(newComponent.Render());
            }

            // Otherwise recycled and was already updated
            return recycledView;
        }
    }
}
