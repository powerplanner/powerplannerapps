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
    public sealed partial class BareTextBox : UserControl
    {
        public event EventHandler EnterPressed;

        public BareTextBox()
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
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(BareTextBox), new PropertyMetadata(""));



        // Best practices for input validation are documented pretty well here: https://github.com/microsoft/AdaptiveCards/issues/1978 In summary, at first nothing should be displayed. Once user starts typing, nothing should be displayed either. The first validation should occur once user leaves the field for the first time (loses focus). If valid and they return to the field and edit it, should return to "initial" state (nothing displayed, just like at start). If field was invalid and they return to the field and edit it, should live validate upon every keystroke until they exit focus.

        // To implement that, we could have the models set values all the time of if it's currently valid or not, and the UI chooses to display

        // Android's text control just has a errorEnabled and error property that you set at whatever time you'd like: https://material.io/develop/android/components/text-fields So in order to share the same logic cross-platform we should have the view models handle when errors should be shown and keep the views "dumb" so that they can be the same across platform.

        public InputValidationState ValidationState
        {
            get { return (InputValidationState)GetValue(ValidationStateProperty); }
            set { SetValue(ValidationStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationStateProperty =
            DependencyProperty.Register(nameof(ValidationState), typeof(InputValidationState), typeof(BareTextBox), new PropertyMetadata(null, OnValidationStateChanged));

        private static void OnValidationStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as BareTextBox).OnValidationStateChanged(e);
        }

        private void OnValidationStateChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ValidationState == null)
            {
                ValidationSymbolViewbox.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (ValidationState == InputValidationState.Valid)
                {
                    ValidationSymbol.Foreground = new SolidColorBrush(Colors.Green);
                    ValidationSymbol.Symbol = Symbol.Accept;
                }
                else
                {
                    ValidationSymbol.Foreground = new SolidColorBrush(Colors.Red);
                    ValidationSymbol.Symbol = Symbol.Cancel;
                }

                ValidationSymbolViewbox.Visibility = Visibility.Visible;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(BareTextBox), new PropertyMetadata(""));

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (EnterPressed != null)
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    EnterPressed(this, new EventArgs());
                }
            }
        }



        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaceholderText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(BareTextBox), new PropertyMetadata(null));



        public InputScope InputScope
        {
            get { return (InputScope)GetValue(InputScopeProperty); }
            set { SetValue(InputScopeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputScope.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputScopeProperty =
            DependencyProperty.Register(nameof(InputScope), typeof(InputScope), typeof(BareTextBox), new PropertyMetadata(null));



        public bool IsSpellCheckEnabled
        {
            get { return (bool)GetValue(IsSpellCheckEnabledProperty); }
            set { SetValue(IsSpellCheckEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSpellCheckEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSpellCheckEnabledProperty =
            DependencyProperty.Register(nameof(IsSpellCheckEnabled), typeof(bool), typeof(BareTextBox), new PropertyMetadata(true));



        public bool IsTextPredictionEnabled
        {
            get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
            set { SetValue(IsTextPredictionEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextPredictionEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextPredictionEnabledProperty =
            DependencyProperty.Register(nameof(IsTextPredictionEnabled), typeof(bool), typeof(BareTextBox), new PropertyMetadata(true));

        /// <summary>
        /// Note that setting this does nothing.
        /// </summary>
        public bool HasFocus
        {
            get { return (bool)GetValue(HasFocusProperty); }
            set { SetValue(HasFocusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasFocus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasFocusProperty =
            DependencyProperty.Register(nameof(HasFocus), typeof(bool), typeof(BareTextBox), new PropertyMetadata(false));

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            HasFocus = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HasFocus = false;
        }
    }
}
