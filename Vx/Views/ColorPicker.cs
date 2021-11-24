using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Vx.Views
{
    public class ColorPicker : View
    {
        public string Header { get; set; }

        public bool IsEnabled { get; set; } = true;

        public VxValue<Color> Color { get; set; }
    }

    public class InternalColorPickerComponent : VxComponent
    {
        public string Header { get => GetState<string>(); set => SetState(value); }
        public bool IsEnabled { get => GetState<bool>(); set => SetState(value); }

        private bool _hasCustomColor = false;

        public Color Color
        {
            get => GetState<Color>();
            set
            {
                if (Color == value)
                {
                    return;
                }

                // If we have custom color right now and the new color is one of the default colors
                if (_hasCustomColor && ColorItem.DefaultColors.Any(i => i.Color == value))
                {
                    // Remove the custom color
                    _colors.RemoveAt(_colors.Count - 2);
                    _hasCustomColor = false;
                }

                var matching = _colors.FirstOrDefault(i => i.Color == value);
                if (matching == null || matching == _colors.Last())
                {
                    if (_hasCustomColor)
                    {
                        _colors.RemoveAt(_colors.Count - 2);
                    }

                    var custom = new ColorItem("Custom", value);
                    _colors.Insert(_colors.Count - 1, custom);

                    _hasCustomColor = true;
                }

                SetState(value);
            }
        }

        public Action<Color> ColorChanged { get; set; }

        public Action PickCustomColor { get; set; }

        public class ColorItem : IEquatable<byte[]>
        {
            public static readonly List<ColorItem> DefaultColors = new List<ColorItem>()
            {
                new ColorItem("Blue", Color.FromArgb(27,161,226)),
                new ColorItem("Red", Color.FromArgb(229,20,0)),
                new ColorItem("Green", Color.FromArgb(51,153,51)),
                new ColorItem("Purple", Color.FromArgb(162,0,255)),
                new ColorItem("Pink", Color.FromArgb(230,113,184)),
                new ColorItem("Mango", Color.FromArgb(240,150,9)),
                new ColorItem("Teal", Color.FromArgb(0,171,169)),
                new ColorItem("Lime", Color.FromArgb(140,191,38)),
                new ColorItem("Magenta", Color.FromArgb(255,0,151)),
                new ColorItem("Brown", Color.FromArgb(160,80,0)),
                new ColorItem("Gray", Color.FromArgb(75,75,75)),
                new ColorItem("Nokia", Color.FromArgb(16,128,221)),
                new ColorItem("HTC", Color.FromArgb(105,180,15))
            };

            public string Text { get; set; }
            public Color Color { get; set; }

            public ColorItem(string text, Color color)
            {
                Text = text;
                Color = color;
            }

            public bool Equals(byte[] other)
            {
                if (Color == null || other == null)
                    return false;

                if (other.Length == 4)
                {
                    other = other.Skip(1).ToArray();
                }

                return Color.R == other[0] && Color.G == other[1] && Color.B == other[2];
            }

            public bool Equals(Color other)
            {
                return Color == other;
            }
        }

        private ObservableCollection<ColorItem> _colors;

        public InternalColorPickerComponent()
        {
            _colors = new ObservableCollection<ColorItem>(ColorItem.DefaultColors);
            _colors.Add(new ColorItem("Pick custom color", Color.Black));
        }

        protected override View Render()
        {
            return new ComboBox
            {
                Header = Header,
                IsEnabled = IsEnabled,
                Items = _colors,
                SelectedItem = VxValue.Create<object>(_colors.FirstOrDefault(i => i.Equals(Color)), v =>
                {
                    if (v == _colors.Last())
                    {
                        PickCustomColor?.Invoke();
                    }
                    else
                    {
                        Color = (v as ColorItem).Color;
                        ColorChanged?.Invoke(Color);
                    }
                }),
                Margin = new Thickness(0, 18, 0, 0),
                ItemTemplate = v =>
                {
                    if (v == null)
                    {
                        return new TextBlock { };
                    }

                    var c = v as ColorItem;

                    bool isPickCustomColor = v == _colors.Last();

                    return new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                            {

                                isPickCustomColor ? (View)new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                                    FontSize = 12,
                                    VerticalAlignment = VerticalAlignment.Center
                                } : (View)new Border
                                {
                                    Width = 16,
                                    Height = 16,
                                    BackgroundColor = c.Color,
                                    BorderColor = System.Drawing.Color.Black,
                                    BorderThickness = new Thickness(1),
                                    VerticalAlignment = VerticalAlignment.Center
                                },

                                new TextBlock
                                {
                                    Text = c.Text,
                                    WrapText = false,
                                    Margin = new Thickness(10, 0, 0, 0),
                                    VerticalAlignment = VerticalAlignment.Center
                                }
                            }
                    };
                }
            };
        }
    }
}
