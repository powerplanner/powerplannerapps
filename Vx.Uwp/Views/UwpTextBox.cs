﻿using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpTextBox : UwpView<Vx.Views.TextBox, BareTextBox>
    {
        public UwpTextBox()
        {
            View.IsSpellCheckEnabled = true;
            View.IsTextPredictionEnabled = true;
            View.NativeTextBox.TextChanged += View_TextChanged;
            View.NativeTextBox.LostFocus += NativeTextBox_LostFocus;
            View.NativeTextBox.GotFocus += NativeTextBox_GotFocus;
            View.Loaded += View_Loaded;
        }

        private void View_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView != null && VxView.AutoFocus)
            {
                View.NativeTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                View.NativeTextBox.SelectAll();
            }
        }

        private void NativeTextBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(true);
            }
        }

        private void NativeTextBox_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(false);
            }
        }

        private void View_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = View.Text;
            }
        }

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Header = newView.Header;
            View.Text = newView.Text?.Value ?? "";
            View.PlaceholderText = newView.PlaceholderText;
            View.ValidationState = newView.ValidationState;
        }
    }
}
