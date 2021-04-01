using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.Controls
{
    public class NumberTextBox : TextBox
    {
        public NumberTextBox()
        {
            InputScope = new InputScope()
            {
                Names =
                {
                    new InputScopeName(InputScopeNameValue.Number)
                }
            };

            GotFocus += NumberTextBox_GotFocus;
            LostFocus += NumberTextBox_LostFocus;
        }

        private void NumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private bool _hasLostFocusOnce;
        private void NumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _hasLostFocusOnce = true;
            IsValid();
        }

        public enum NumberTypes
        {
            Integer,
            Decimal
        }

        /// <summary>
        /// Whether this should allow integers or decimals
        /// </summary>
        public NumberTypes NumberType
        {
            get { return (NumberTypes)GetValue(NumberTypeProperty); }
            set { SetValue(NumberTypeProperty, value); }
        }

        public static readonly DependencyProperty NumberTypeProperty =
            DependencyProperty.Register(nameof(NumberType), typeof(NumberTypes), typeof(NumberTextBox), new PropertyMetadata(NumberTypes.Decimal, OnNumberPropChanged));

        private static void OnNumberPropChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as NumberTextBox).OnNumberPropChanged(e);
        }

        private void OnNumberPropChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_hasLostFocusOnce)
            {
                IsValid();
            }
        }

        /// <summary>
        /// Minimum number allowed
        /// </summary>
        public double NumberMin
        {
            get { return (double)GetValue(NumberMinProperty); }
            set { SetValue(NumberMinProperty, value); }
        }

        public static readonly DependencyProperty NumberMinProperty =
            DependencyProperty.Register(nameof(NumberMin), typeof(double), typeof(NumberTextBox), new PropertyMetadata(double.MinValue, OnNumberPropChanged));

        /// <summary>
        /// Checks whether currently valid (and also updates the UI)
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            switch (NumberType)
            {
                case NumberTypes.Integer:
                    {
                        if (int.TryParse(Text, out int val))
                        {
                            if (val >= NumberMin)
                            {
                                IsValidForUI = true;
                                return true;
                            }
                        }
                    }
                    break;

                case NumberTypes.Decimal:
                    {
                        if (double.TryParse(Text, out double val))
                        {
                            if (val >= NumberMin)
                            {
                                IsValidForUI = true;
                                return true;
                            }
                        }
                    }
                    break;
            }

            IsValidForUI = false;
            return false;
        }

        private bool _isValidForUI = true;
        private bool IsValidForUI
        {
            get { return _isValidForUI; }
            set
            {
                if (value == _isValidForUI)
                {
                    return;
                }

                _isValidForUI = value;

                if (value)
                {
                    Background = Resources["TextControlBackground"] as Brush;
                    BorderBrush = Resources["TextControlBorderBrush"] as Brush;
                }
                else
                {
                    Background = new SolidColorBrush(Color.FromArgb(55, 255, 0, 0));
                    BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }
    }
}
