using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using ToolsPortable;

namespace InterfacesDroid.Views
{
    public class CustomColorPickerView : InflatedViewWithBinding
    {
        private Channel m_channelR = new Channel()
        {
            Name = "R"
        };

        private Channel m_channelG = new Channel()
        {
            Name = "G"
        };

        private Channel m_channelB = new Channel()
        {
            Name = "B"
        };

        private View m_colorView;

        public CustomColorPickerView(ViewGroup root) : base(Resource.Layout.CustomColorPickerView, root)
        {
            Initialize();
        }

        public CustomColorPickerView(Context context, IAttributeSet attrs) : base(Resource.Layout.CustomColorPickerView, context, attrs)
        {
            Initialize();
        }

        public CustomColorPickerView(Context context) : base(Resource.Layout.CustomColorPickerView, context)
        {
            Initialize();
        }

        private Color m_color;

        public Color Color
        {
            get { return m_color; }
            set
            {
                if (object.Equals(value, m_color))
                    return;

                m_color = value;
                m_channelR.Value = value.R;
                m_channelG.Value = value.G;
                m_channelB.Value = value.B;
                NotifyPropertyChanged();
            }
        }

        private void Initialize()
        {
            m_channelR.PropertyChanged += Channel_PropertyChanged;
            m_channelG.PropertyChanged += Channel_PropertyChanged;
            m_channelB.PropertyChanged += Channel_PropertyChanged;

            m_colorView = FindViewById(Resource.Id.ColorView);

            var channelContainer = FindViewById<LinearLayout>(Resource.Id.channel_container);
            CreateAndAddChannelView(channelContainer, m_channelR);
            CreateAndAddChannelView(channelContainer, m_channelG);
            CreateAndAddChannelView(channelContainer, m_channelB);

            PropertyChanged += CustomColorPickerView_PropertyChanged;

            // Start off with a default color
            Color = Color.DarkRed;
        }

        private void CreateAndAddChannelView(LinearLayout channelContainer, Channel c)
        {
            var view = new CustomColorPickerChannelView(channelContainer, c)
            {
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    LinearLayout.LayoutParams.WrapContent)
                {
                    BottomMargin = ThemeHelper.AsPx(Context, 12)
                }
            };

            channelContainer.AddView(view);
        }

        private void CustomColorPickerView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Color):
                    m_colorView.SetBackgroundColor(Color);
                    break;
            }
        }

        private void Channel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetProperty(ref m_color, new Color(m_channelR.Value, m_channelG.Value, m_channelB.Value), nameof(Color));
        }

        private class CustomColorPickerChannelView : InflatedView
        {
            private Channel m_channel;

            public CustomColorPickerChannelView(ViewGroup root, Channel c) : base(root.Context, Resource.Layout.CustomColorPickerChannelView)
            {
                // Apparently the XML bindings don't work when it's running inside of Power Planner, so changing to programmatic
                m_channel = c;
                FindViewById<TextView>(Resource.Id.Label).Text = c.Name;
                var valueText = FindViewById<TextView>(Resource.Id.ColorValue);
                var seekBar = FindViewById<SeekBar>(Resource.Id.ColorSeekBar);
                Bindings.Programmatic.Binding.SetBinding(c, nameof(c.Value), delegate
                {
                    valueText.Text = c.Value.ToString();
                    seekBar.Progress = c.Value;
                });
                seekBar.ProgressChanged += new WeakEventHandler<SeekBar.ProgressChangedEventArgs>(SeekBar_ProgressChanged).Handler;
            }

            private void SeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
            {
                m_channel.Value = e.Progress;
            }
        }

        private class Channel : BindableBase
        {
            /// <summary>
            /// The name, like "R", "G", or "B"
            /// </summary>
            public string Name { get; set; }

            private int m_value;
            /// <summary>
            /// Value between 0 and 255. Supports binding.
            /// </summary>
            public int Value
            {
                get { return m_value; }
                set { SetProperty(ref m_value, value, nameof(Value)); }
            }
        }
    }
}