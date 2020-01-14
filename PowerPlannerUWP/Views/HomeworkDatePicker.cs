using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace PowerPlannerUWP.Views
{
    public sealed class HomeworkDatePicker : Control
    {
        public HomeworkDatePicker()
        {
            this.DefaultStyleKey = typeof(HomeworkDatePicker);
            //this.Style = App.Current.Resources["HomeworkDatePickerStyle"] as Style;
            
            Date = DateTime.Today;

            ButtonCommand = new ButtonCommandHandler();
        }

        private class ButtonCommandHandler : ICommand
        {
            /// <summary>
            /// Not used, only here because required for interface
            /// </summary>
#pragma warning disable 0067
            public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {

            }
        }

        public static readonly DependencyProperty DateProperty = DependencyProperty.Register("Date", typeof(DateTime), typeof(HomeworkDatePicker), new PropertyMetadata(null, OnChanged));

        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateFormatProperty = DependencyProperty.Register("DateFormat", typeof(string), typeof(HomeworkDatePicker), new PropertyMetadata("MMMM d, yyyy", OnChanged));

        public string DateFormat
        {
            get { return GetValue(DateFormatProperty) as string; }
            set { SetValue(DateFormatProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(HomeworkDatePicker), new PropertyMetadata("Date"));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private static void OnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HomeworkDatePicker).OnChanged();
        }

        private void OnChanged()
        {
            FinalDateString = Date.ToString(DateFormat);
        }

        private static readonly DependencyProperty FinalDateStringProperty = DependencyProperty.Register("FinalDateString", typeof(string), typeof(HomeworkDatePicker), new PropertyMetadata(null));

        public string FinalDateString
        {
            get { return GetValue(FinalDateStringProperty) as string; }
            set { SetValue(FinalDateStringProperty, value); }
        }

        private static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(HomeworkDatePicker), new PropertyMetadata(null));

#pragma warning disable IDE0052 // Remove unread private members
        private ICommand ButtonCommand
#pragma warning restore IDE0052 // Remove unread private members
        {
            get { return GetValue(ButtonCommandProperty) as ICommand; }
            set { SetValue(ButtonCommandProperty, value); }
        }
    }
}
