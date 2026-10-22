using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP
{
    public sealed partial class CustomColorPicker : UserControl
    {
        public CustomColorPicker()
        {
            this.InitializeComponent();
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(CustomColorPicker), new PropertyMetadata("Color"));

        public event EventHandler<Color> ColorChanged;

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(CustomColorPicker), new PropertyMetadata(Colors.Blue, OnColorChanged));

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CustomColorPicker)?.OnColorChanged(e);
        }

        private void OnColorChanged(DependencyPropertyChangedEventArgs e)
        {
            ColorNameTextBlock.Text = ToolsPortable.ColorHelper.ToString(new byte[] { Color.R, Color.G, Color.B }).ToUpper();
            ColorChanged?.Invoke(this, Color);
        }

        private void MyColorPicker_ColorChanged(Windows.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args)
        {
            Color = MyColorPicker.Color;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonPicker.Flyout.Hide();
        }

        private void Flyout_Opening(object sender, object e)
        {
            MyColorPicker.Color = Color;
        }
    }
}
