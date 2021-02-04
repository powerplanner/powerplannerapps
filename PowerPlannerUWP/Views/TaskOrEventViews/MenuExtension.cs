using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    // From: https://stackoverflow.com/a/33841544/3721066
    public static class MenuFlyoutItemExtension<T> where T : MenuFlyoutItemBase
    {
        public static List<T> GetMyItems(DependencyObject obj)
        { return (List<T>)obj.GetValue(MyItemsProperty); }

        public static void SetMyItems(DependencyObject obj, List<T> value)
        { obj.SetValue(MyItemsProperty, value); }

        public static readonly DependencyProperty MyItemsProperty =
            DependencyProperty.Register("MyItems", typeof(List<T>), typeof(MenuFlyoutItemExtension<T>),
            new PropertyMetadata(new List<T>(), (sender, e) =>
            {
                Debug.WriteLine("Filling collection");
                var menu = sender as MenuFlyoutSubItem;
                menu.Items.Clear();
                foreach (var item in e.NewValue as List<T>) menu.Items.Add(item);
            }));
    }
}
