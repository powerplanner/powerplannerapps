using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Graphics;
using InterfacesDroid.Themes;
using Android.Graphics.Drawables;
using InterfacesDroid.Dialogs;
using ToolsPortable;

namespace InterfacesDroid.Views
{
    public class MyColorPicker : Spinner
    {
        public static readonly List<ColorItem> DefaultColors = new List<ColorItem>()
        {
            new ColorItem("blue", new Color(27,161,226)),
            new ColorItem("red", new Color(229,20,0)),
            new ColorItem("green", new Color(51,153,51)),
            new ColorItem("purple", new Color(162,0,255)),
            new ColorItem("pink", new Color(230,113,184)),
            new ColorItem("mango", new Color(240,150,9)),
            new ColorItem("teal", new Color(0,171,169)),
            new ColorItem("lime", new Color(140,191,38)),
            new ColorItem("magenta", new Color(255,0,151)),
            new ColorItem("brown", new Color(160,80,0)),
            new ColorItem("gray", new Color(75,75,75)),
            new ColorItem("nokia", new Color(16,128,221)),
            new ColorItem("htc", new Color(105,180,15))
        };

        private static ColorItem CreateCustomItem(Color color)
        {
            return new ColorItem("custom", color);
        }

        public MyColorPicker(Context context) : base(context)
        {
            Initialize();
        }

        public MyColorPicker(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            AvailableColors = DefaultColors.ToArray();
        }

        private ColorItem[] _availableColors;
        public ColorItem[] AvailableColors
        {
            get { return _availableColors; }
            set
            {
                _availableColors = value;

                if (value == null)
                {
                    Adapter = null;
                }

                else
                {
                    Adapter = new ColorSpinnerAdapter(value);
                }
            }
        }

        public override void SetSelection(int position)
        {
            if (position >= AvailableColors.Length)
            {
                ShowCustomColorPicker();
                return;
            }

            base.SetSelection(position);
        }

        public Activity ActivityForDialog { get; set; }

        private CustomColorPickerDialog m_customColorPickerDialog;

        public void ShowCustomColorPicker()
        {
            if (m_customColorPickerDialog == null)
            {
                if (ActivityForDialog == null)
                {
                    ActivityForDialog = (BareMvvm.Core.App.PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow as Windows.NativeDroidAppWindow).Activity;
                    if (ActivityForDialog == null)
                    {
                        throw new InvalidOperationException("ActivityForDialog must be assigned");
                    }
                }

                m_customColorPickerDialog = new CustomColorPickerDialog(ActivityForDialog);
                m_customColorPickerDialog.ColorChosen += M_customColorPickerDialog_ColorChosen;
            }

            m_customColorPickerDialog.Show(SelectedColor);
        }

        private void M_customColorPickerDialog_ColorChosen(object sender, Color e)
        {
            SelectedColor = e;
        }

        private ColorItem m_customColor;

        public Color SelectedColor
        {
            get
            {
                if (AvailableColors == null)
                    return default(Color);

                return AvailableColors[this.SelectedItemPosition].Color;
            }

            set
            {
                if (AvailableColors == null)
                {
                    throw new NullReferenceException("AvailableColors was null. Set AvailableColors first.");
                }

                int pos = GetPosition(value);
                if (pos == -1)
                {
                    if (m_customColor == null)
                    {
                        m_customColor = CreateCustomItem(value);
                        AvailableColors = AvailableColors.Concat(new ColorItem[] { m_customColor }).ToArray();
                    }
                    else
                    {
                        m_customColor.Color = value;
                    }
                    pos = AvailableColors.Length - 1;
                }

                base.SetSelection(pos);
            }
        }

        private int GetPosition(Color color)
        {
            for (int i = 0; i < AvailableColors.Length; i++)
            {
                if (AvailableColors[i].Color.ToArgb() == color.ToArgb())
                {
                    return i;
                }
            }

            return -1;
        }

        public class ColorSpinnerAdapter : BaseAdapter
        {
            public ColorItem[] Items;

            public ColorSpinnerAdapter(ColorItem[] items)
            {
                Items = items;
            }

            public override int Count
            {
                get
                {
                    return Items.Length + 1;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return null;
            }

            public override long GetItemId(int position)
            {
                if (position >= Items.Length)
                {
                    return -1;
                }

                return Items[position].Color.ToArgb();
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (position >= Items.Length)
                {
                    // Pick custom color view
                    LinearLayout customLayout = new LinearLayout(parent.Context)
                    {
                        Orientation = Orientation.Horizontal,
                        LayoutParameters = new AbsListView.LayoutParams(
                            AbsListView.LayoutParams.MatchParent,
                            ThemeHelper.AsPx(parent.Context, 62))
                    };
                    customLayout.SetPaddingRelative(ThemeHelper.AsPx(parent.Context, 16), 0, 0, 0);

                    var tvSymbol = new TextView(parent.Context)
                    {
                        Text = "✏️",
                        LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.WrapContent,
                        ThemeHelper.AsPx(parent.Context, 20))
                        {
                            Gravity = GravityFlags.CenterVertical,
                            RightMargin = ThemeHelper.AsPx(parent.Context, 8)
                        }
                    };
                    tvSymbol.SetTextSize(ComplexUnitType.Sp, 15);
                    customLayout.AddView(tvSymbol);

                    customLayout.AddView(new TextView(parent.Context)
                    {
                        Text = "pick custom color",
                        LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.WrapContent)
                        {
                            Gravity = GravityFlags.CenterVertical
                        }
                    });

                    return customLayout;
                }

                ColorItem item = Items[position];

                LinearLayout layout = new LinearLayout(parent.Context)
                {
                    Orientation = Orientation.Horizontal,
                    LayoutParameters = new AbsListView.LayoutParams(
                        AbsListView.LayoutParams.MatchParent,
                        ThemeHelper.AsPx(parent.Context, 62))
                };
                layout.SetPaddingRelative(ThemeHelper.AsPx(parent.Context, 16), 0, 0, 0);

                var colorSquare = new View(parent.Context)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(
                        ThemeHelper.AsPx(parent.Context, 20),
                        ThemeHelper.AsPx(parent.Context, 20))
                    {
                        RightMargin = ThemeHelper.AsPx(parent.Context, 8),
                        Gravity = GravityFlags.CenterVertical
                    }
                };
                Bindings.Programmatic.Binding.SetBinding(item, nameof(item.Color), delegate
                {
                    colorSquare.SetBackgroundColor(item.Color);
                });
                layout.AddView(colorSquare);

                layout.AddView(new TextView(parent.Context)
                {
                    Text = item.Text,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.WrapContent)
                    {
                        Gravity = GravityFlags.CenterVertical
                    }
                });

                return layout;
            }
        }

        public class ColorItem : BindableBase, IEquatable<Color>
        {
            public string Text { get; set; }

            private Color m_color;
            public Color Color
            {
                get { return m_color; }
                set { SetProperty(ref m_color, value, nameof(Color)); }
            }

            public ColorItem(string text, Color color)
            {
                Text = text;
                Color = color;
            }

            public bool Equals(Color other)
            {
                if (Color == null || other == null)
                    return false;

                return Color.ToArgb() == other.ToArgb();
            }
        }
    }
}