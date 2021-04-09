using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public static class VxListViewExtensions
    {
        public static T BindSelectedItem<T, I>(this T listView, VxState<I> value) where T : ListView
        {
            SetBindSelectedItem(listView, value);
            return listView;
        }

        public static readonly BindableProperty BindSelectedItemProperty = BindableProperty.CreateAttached(nameof(BindSelectedItem), typeof(VxState), typeof(VxListViewExtensions), null, propertyChanged: BindSelectedItemChanged);

        public static VxState GetBindSelectedItem(BindableObject target)
        {
            return target.GetValue(BindSelectedItemProperty) as VxState;
        }

        public static void SetBindSelectedItem(BindableObject target, VxState value)
        {
            if (object.Equals(GetBindSelectedItem(target), value))
            {
                // We have to set manually since otherwise it'll consider it the same value and not update it
                if (value != null)
                {
                    (target as ListView).SelectedItem = value.Value;
                }
            }
            else
            {
                target.SetValue(BindSelectedItemProperty, value);
            }
        }

        private static void BindSelectedItemChanged(BindableObject sender, object oldVal, object newVal)
        {
            var listView = sender as ListView;

            // Assign the selected item
            if (newVal != null)
            {
                listView.SelectedItem = (newVal as VxState).Value;
            }

            // If we're setting this for the first time on the view, subscribe
            if (oldVal == null)
            {
                listView.ItemSelected += ListView_ItemSelected;
            }
        }

        internal static HashSet<ListView> ItemSelectedToIgnore { get; private set; } = new HashSet<ListView>();

        private static void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var view = sender as ListView;

            if (ItemSelectedToIgnore.Remove(view))
            {
                if (e.SelectedItem != null)
                {
                    throw new InvalidOperationException("This isn't expected...");
                }

                return;
            }

            var item = GetBindSelectedItem(view);
            if (item != null)
            {
                item.Value = e.SelectedItem;
            }
        }

        public static T ItemTap<T>(this T listView, Action<ItemTappedEventArgs> itemTapped) where T : ListView
        {
            // TODO: Need to support this in a VxComponent-manner-way. Right now this only works if the initial render returns it, but if this happens via reconciliation, it won't work. Not a big issue though, very unlikely that'd happen.
            listView.ItemTapped += (s, e) =>
            {
                itemTapped(e);
            };
            return listView;
        }
    }
}
