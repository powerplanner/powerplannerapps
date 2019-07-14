using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views
{
    public sealed partial class CompletionSlider : UserControl
    {
        public event EventHandler<double> OnValueChangedByUser;

        private void triggerOnValueChangedByUser()
        {
            if (OnValueChangedByUser != null)
                OnValueChangedByUser(this, Value);
        }

        private static readonly Brush INCOMPLETE_BRUSH = new SolidColorBrush(Colors.Red);
        /// <summary>
        /// LightGreen
        /// </summary>
        private static readonly Brush COMPLETE_BRUSH = new SolidColorBrush(Color.FromArgb(255, 42, 222, 42)); // #2ade2a
        private static readonly Brush UNSELECTED_BRUSH = new SolidColorBrush(Color.FromArgb(255, 110, 110, 110));

        public CompletionSlider()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(CompletionSlider), new PropertyMetadata(0.0, onValueChanged));

        private static void onValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as CompletionSlider).onValueChanged(args);
        }

        private void onValueChanged(DependencyPropertyChangedEventArgs args)
        {
            setValue((double)args.NewValue);
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void setValue(double value)
        {
            if (value < 0)
                value = 0;

            if (value > 1)
                value = 1;


            if (value == 0)
            {
                ellipseIncomplete.Fill = INCOMPLETE_BRUSH;
                ellipseComplete.Fill = UNSELECTED_BRUSH;
            }

            else if (value == 1)
            {
                ellipseIncomplete.Fill = UNSELECTED_BRUSH;
                ellipseComplete.Fill = COMPLETE_BRUSH;
            }

            else
            {
                ellipseIncomplete.Fill = UNSELECTED_BRUSH;
                ellipseComplete.Fill = UNSELECTED_BRUSH;
            }
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void UserControl_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void ellipseIncomplete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Value = 0;
            e.Handled = true;
            triggerOnValueChangedByUser();
        }

        private void ellipseComplete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Value = 1;
            e.Handled = true;
            triggerOnValueChangedByUser();
        }

        private void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            triggerOnValueChangedByUser();
        }
    }
}
