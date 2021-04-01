using InterfacesUWP.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return new SolidColorBrush((Color)(value));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
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

            return Color.Equals(other);
        }
    }
    
    public sealed partial class ColorPicker : UserControl
    {
        public static readonly List<ColorItem> DefaultColors;
        public static Color DefaultSelectedColor { get; private set; } = Color.FromArgb(255, 27, 161, 226);

        static ColorPicker()
        {
            List<ColorItem> colors = new List<ColorItem>()
            {
                new ColorItem("blue", DefaultSelectedColor),
                new ColorItem("red", Color.FromArgb(255,229,20,0)),
                new ColorItem("green", Color.FromArgb(255,51,153,51)),
                new ColorItem("purple", Color.FromArgb(255,162,0,255)),
                new ColorItem("pink", Color.FromArgb(255,230,113,184)),
                new ColorItem("mango", Color.FromArgb(255,240,150,9)),
                new ColorItem("teal", Color.FromArgb(255,0,171,169)),
                new ColorItem("lime", Color.FromArgb(255,140,191,38)),
                new ColorItem("magenta", Color.FromArgb(255,255,0,151)),
                new ColorItem("brown", Color.FromArgb(255,160,80,0)),
                new ColorItem("gray", Color.FromArgb(255,75,75,75)),
                new ColorItem("nokia", Color.FromArgb(255,16,128,221)),
                new ColorItem("htc", Color.FromArgb(255,105,180,15))
            };

            DefaultColors = colors;
        }

        private ObservableCollection<ColorItem> m_Colors;
        private ColorItem m_CustomColor;
        private Windows.UI.Xaml.Controls.ColorPicker m_CustomPicker;

        public static Lazy<bool> IsCustomPickerSupported = new Lazy<bool>(delegate
        {
            return ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.ColorPicker");
        }, isThreadSafe: false);

        private static ColorItem CreateCustomItem(Color color)
        {
            return new ColorItem("custom", color);
        }

        public ColorPicker()
        {
            this.InitializeComponent();

            m_Colors = new ObservableCollection<ColorItem>(DefaultColors);

            comboBox.ItemsSource = m_Colors;
            comboBox.SelectionChanged += ComboBox_SelectionChanged;

            if (IsCustomPickerSupported.Value)
            {
                comboBox.Actions = new ComboBoxAction[]
                {
                    new ComboBoxAction()
                    {
                        Title = "pick custom color",
                        Symbol = Symbol.Edit,
                        Action = ShowCustomPicker
                    }
                };
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedItem != null)
            {
                Color c = (comboBox.SelectedItem as ColorItem).Color;
                if (SelectedColor != c)
                {
                    SelectedColor = c;
                }
            }
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ColorPicker), null);

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new PropertyMetadata(DefaultSelectedColor, OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as ColorPicker).OnSelectedColorChanged(args);
        }

        private void OnSelectedColorChanged(DependencyPropertyChangedEventArgs args)
        {
            Color c = (Color)args.NewValue;

            //find the color
            for (int i = 0; i < ColorItems.Count; i++)
                if (ColorItems[i].Equals(c))
                {
                    if (comboBox.SelectedItem != ColorItems[i])
                    {
                        comboBox.SelectedItem = ColorItems[i];
                    }
                    return;
                }

            // If couldn't find, means it's a custom color
            // First we need to make sure custom color item is in list
            if (m_CustomColor == null)
            {
                m_CustomColor = CreateCustomItem(c);
                m_Colors.Add(m_CustomColor);
                comboBox.SelectedItem = m_CustomColor;
            }

            // Otherwise we need to update the custom color
            else
            {
                m_CustomColor.Color = c;
            }
        }

        public IList<ColorItem> ColorItems
        {
            get { return (IList<ColorItem>)comboBox.ItemsSource; }
            set { comboBox.ItemsSource = value; }
        }

        public void ShowCustomPicker()
        {
            try
            {
                if (!IsCustomPickerSupported.Value)
                {
                    return;
                }

                if (m_CustomPicker == null)
                {
                    m_CustomPicker = new Windows.UI.Xaml.Controls.ColorPicker();
                    CustomColorPickerContainer.Child = m_CustomPicker;
                }

                m_CustomPicker.Color = SelectedColor;

                UpdateFlyoutSize();

                FlyoutCustomPicker.ShowAt(this);
            }
            catch (Exception ex)
            {
                ExceptionHelper.OnHandledExceptionOccurred?.Invoke(ex);
            }
        }

        private void ButtonConfirmCustomColor_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = m_CustomPicker.Color;
            FlyoutCustomPicker.Hide();
        }

        private void ButtonCancelCustomColor_Click(object sender, RoutedEventArgs e)
        {
            FlyoutCustomPicker.Hide();
        }

        private void FlyoutContents_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFlyoutSize();
        }
        private void UpdateFlyoutSize()
        {
            var style = new Style(typeof(FlyoutPresenter));

            // If height hasn't been loaded yet, we use window height (start bigger). Otherwise we use the min required height.
            // We have to start bigger since XAML doesn't behave well when we start with a smaller size (like 0) and then later increase it.
            style.Setters.Add(new Setter(FlyoutPresenter.MinHeightProperty, FlyoutContents.ActualHeight == 0 ? Window.Current.Bounds.Height : Math.Min(Window.Current.Bounds.Height, FlyoutContents.ActualHeight)));
            FlyoutCustomPicker.FlyoutPresenterStyle = style;
        }
    }
}
