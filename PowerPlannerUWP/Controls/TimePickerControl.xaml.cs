using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Controls
{
    public sealed partial class TimePickerControl : UserControl
    {
        public static DependencyProperty IsEndTimeProperty = DependencyProperty.RegisterAttached("IsEndTime",
            typeof(bool),
            typeof(TimePickerControl),
            new PropertyMetadata(false, (s, e) => IsEndTimeChanged(s, e)));
        public static bool GetIsEndTime(DependencyObject obj) => (bool)obj.GetValue(IsEndTimeProperty);
        public static void SetIsEndTime(DependencyObject obj, bool val) => obj.SetValue(IsEndTimeProperty, val);

        // The below code let's us use the "Header" property on this control.
        public static DependencyProperty HeaderProperty = ComboBox.HeaderProperty;

        public static string GetHeader(DependencyObject obj) => ((TimePickerControl)obj).Header;
        public static string SetHeader(DependencyObject obj, string val) => ((TimePickerControl)obj).Header = val;

        public string Header
        {
            get => (string)TimePickerComboBox.GetValue(ComboBox.HeaderProperty);
            set => TimePickerComboBox.SetValue(ComboBox.HeaderProperty, value);
        }

        public BindingBase StartItems => new Binding()
        {
            Path = new PropertyPath("StartTimeItems"),
            Mode = BindingMode.OneWay
        };

        public BindingBase StartSelectedItem => new Binding()
        {
            Path = new PropertyPath("StartTimeSelected"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        public BindingBase EndItems => new Binding()
        {
            Path = new PropertyPath("EndTimeItems"),
            Mode = BindingMode.OneWay
        };

        public BindingBase EndSelectedItem => new Binding()
        {
            Path = new PropertyPath("EndTimeSelected"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        public TimePickerControlViewModel ControlVM;


        private static void IsEndTimeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TimePickerControl).ChangeEndTime((bool)e.NewValue);
        }

        public TimePickerControl()
        {
            this.InitializeComponent();
            ChangeEndTime(false);
        }

        public void ChangeEndTime(bool val)
        {
            // Adjust the items and selected value bindings.
            TimePickerComboBox.SetBinding(ItemsControl.ItemsSourceProperty, val ? EndItems : StartItems);
            TimePickerComboBox.SetBinding(Selector.SelectedItemProperty, val ? EndSelectedItem : StartSelectedItem);
        }
    }
}
