using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP
{
    public class DayOfWeekComboBox : ComboBox
    {
        public DayOfWeekComboBox()
        {
            base.ItemsSource = new DayOfWeek[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };

            base.SetBinding(ComboBox.SelectedItemProperty, new Binding()
            {
                Path = new PropertyPath("SelectedDayOfWeek"),
                Source = this,
                Mode = BindingMode.TwoWay
            });
        }

        public static readonly DependencyProperty SelectedDayOfWeekProperty = DependencyProperty.Register("SelectedDayOfWeek", typeof(DayOfWeek), typeof(DayOfWeekComboBox), new PropertyMetadata(DayOfWeek.Monday));

        public DayOfWeek SelectedDayOfWeek
        {
            get { return (DayOfWeek)GetValue(SelectedDayOfWeekProperty); }
            set { SetValue(SelectedDayOfWeekProperty, value); }
        }
    }
}
