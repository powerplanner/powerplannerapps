using CoreGraphics;
using InterfacesiOS.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using UIKit;

namespace InterfacesiOS.Views
{
    /// <summary>
    /// A custom color picker control.
    /// </summary>
    public class BareUICustomColorPicker : BareUIView
    {
        private BindingHost m_internalBinding = new BindingHost();
        public BareUICustomColorPicker()
        {
            Initialize();
        }

        public BareUICustomColorPicker(CGRect frame) : base(frame)
        {
            Initialize();
        }

        private void Initialize()
        {
            m_internalBinding.DataContext = this;

            var rectanglePreview = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            m_internalBinding.SetBackgroundColorBinding(rectanglePreview, nameof(Color));
            AddSubview(rectanglePreview);
            rectanglePreview.StretchWidth(this);

            m_channelR = CreateChannelView("R", (float)Color.Components[0] * 255, new UIColor(200 / 255f, 0, 0, 1));
            m_channelG = CreateChannelView("G", (float)Color.Components[1] * 255, new UIColor(0, 200 / 255f, 0, 1));
            m_channelB = CreateChannelView("B", (float)Color.Components[2] * 255, new UIColor(0, 0, 200 / 255f, 1));

            AddSubview(m_channelR);
            AddSubview(m_channelG);
            AddSubview(m_channelB);
            m_channelR.StretchWidth(this, left: 16, right: 16);
            m_channelG.StretchWidth(this, left: 16, right: 16);
            m_channelB.StretchWidth(this, left: 16, right: 16);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[preview(>=130)]-8-[channelR]-8-[channelG]-8-[channelB]-8-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "preview", rectanglePreview,
                "channelR", m_channelR,
                "channelG", m_channelG,
                "channelB", m_channelB));
        }

        private UIChannelView m_channelR;
        private UIChannelView m_channelG;
        private UIChannelView m_channelB;

        private UIChannelView CreateChannelView(string channelName, float value, UIColor trackColor)
        {
            var answer = new UIChannelView(channelName, value, trackColor)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            answer.ValueChanged += new WeakEventHandler(Slider_ValueChanged).Handler;
            return answer;
        }

        private class UIChannelView : UIView
        {
            private UISlider m_slider;
            private UILabel m_labelValue;
            public event EventHandler ValueChanged;

            public UIChannelView(string channelName, float initialValue, UIColor trackColor)
            {
                var channelLabel = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = channelName
                };
                AddSubview(channelLabel);
                channelLabel.StretchHeight(this);

                m_value = initialValue;
                m_slider = new UISlider()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    MinValue = 0,
                    MaxValue = 255,
                    Value = initialValue,
                    MinimumTrackTintColor = trackColor,
                    ThumbTintColor = trackColor
                };
                m_slider.ValueChanged += new WeakEventHandler(Slider_ValueChanged).Handler;
                AddSubview(m_slider);
                m_slider.StretchHeight(this);

                m_labelValue = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                AddSubview(m_labelValue);
                m_labelValue.StretchHeight(this);
                UpdateTextValue();

                this.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[channel]->=0-[value]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "channel", channelLabel,
                    "value", m_labelValue));

                m_slider.StretchWidth(this, left: 35, right: 40);
            }

            private float m_value;
            public float Value
            {
                get { return m_value; }
                set
                {
                    if (m_value != value)
                    {
                        m_value = value;
                        m_slider.Value = value;
                        UpdateTextValue();
                    }
                }
            }

            private void UpdateTextValue()
            {
                m_labelValue.Text = m_value.ToString();
            }

            private void Slider_ValueChanged(object sender, EventArgs e)
            {
                var newValue = Math.Round(m_slider.Value);
                if (m_value != newValue)
                {
                    m_value = (float)newValue;
                    UpdateTextValue();
                    ValueChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private void Slider_ValueChanged(object sender, EventArgs e)
        {
            var newColor = new CGColor(m_channelR.Value / 255, m_channelG.Value / 255, m_channelB.Value / 255);
            if (!m_color.Components.SequenceEqual(newColor.Components))
            {
                m_color = newColor;
                OnPropertyChanged(nameof(Color));
            }
        }

        private CGColor m_color = new CGColor(207 / 255f, 13 / 255f, 217 / 255f);
        public CGColor Color
        {
            get { return m_color; }
            set
            {
                if (!m_color.Components.SequenceEqual(value.Components))
                {
                    m_color = value;
                    m_channelR.Value = (float)value.Components[0] * 255;
                    m_channelG.Value = (float)value.Components[1] * 255;
                    m_channelB.Value = (float)value.Components[2] * 255;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }
    }
}
