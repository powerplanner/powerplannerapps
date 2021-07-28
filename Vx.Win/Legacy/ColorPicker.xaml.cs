﻿using InterfacesUWP.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Microsoft.UI;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return new SolidColorBrush((Windows.UI.Color)(value));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorItem : BindableBase, IEquatable<Windows.UI.Color>
    {
        public string Text { get; set; }

        private Windows.UI.Color m_color;
        public Windows.UI.Color Color
        {
            get { return m_color; }
            set { SetProperty(ref m_color, value, nameof(Color)); }
        }

        public ColorItem(string text, Windows.UI.Color color)
        {
            Text = text;
            Color = color;
        }

        public bool Equals(Windows.UI.Color other)
        {
            if (Color == null || other == null)
                return false;

            return Color.Equals(other);
        }
    }
    
    public sealed partial class ColorPicker : UserControl
    {
        public static readonly List<ColorItem> DefaultColors;
        public static Windows.UI.Color DefaultSelectedColor { get; private set; } = Windows.UI.Color.FromArgb(255, 27, 161, 226);

        static ColorPicker()
        {
            List<ColorItem> colors = new List<ColorItem>()
            {
                new ColorItem("blue", DefaultSelectedColor),
                new ColorItem("red", Windows.UI.Color.FromArgb(255,229,20,0)),
                new ColorItem("green", Windows.UI.Color.FromArgb(255,51,153,51)),
                new ColorItem("purple", Windows.UI.Color.FromArgb(255,162,0,255)),
                new ColorItem("pink", Windows.UI.Color.FromArgb(255,230,113,184)),
                new ColorItem("mango", Windows.UI.Color.FromArgb(255,240,150,9)),
                new ColorItem("teal", Windows.UI.Color.FromArgb(255,0,171,169)),
                new ColorItem("lime", Windows.UI.Color.FromArgb(255,140,191,38)),
                new ColorItem("magenta", Windows.UI.Color.FromArgb(255,255,0,151)),
                new ColorItem("brown", Windows.UI.Color.FromArgb(255,160,80,0)),
                new ColorItem("gray", Windows.UI.Color.FromArgb(255,75,75,75)),
                new ColorItem("nokia", Windows.UI.Color.FromArgb(255,16,128,221)),
                new ColorItem("htc", Windows.UI.Color.FromArgb(255,105,180,15))
            };

            DefaultColors = colors;
        }

        private ObservableCollection<ColorItem> m_Colors;
        private ColorItem m_CustomColor;
        private Microsoft.UI.Xaml.Controls.ColorPicker m_CustomPicker;

        public static Lazy<bool> IsCustomPickerSupported = new Lazy<bool>(delegate
        {
            return ApiInformation.IsTypePresent("Microsoft.UI.Xaml.Controls.ColorPicker");
        }, isThreadSafe: false);

        private static ColorItem CreateCustomItem(Windows.UI.Color color)
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
                Windows.UI.Color c = (comboBox.SelectedItem as ColorItem).Color;
                if (SelectedColor != c)
                {
                    SelectedColor = c;
                }
            }
        }

        public Windows.UI.Color SelectedColor
        {
            get { return (Windows.UI.Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ColorPicker), null);

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Windows.UI.Color), typeof(ColorPicker), new PropertyMetadata(DefaultSelectedColor, OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as ColorPicker).OnSelectedColorChanged(args);
        }

        private void OnSelectedColorChanged(DependencyPropertyChangedEventArgs args)
        {
            Windows.UI.Color c = (Windows.UI.Color)args.NewValue;

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
                    m_CustomPicker = new Microsoft.UI.Xaml.Controls.ColorPicker();
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
