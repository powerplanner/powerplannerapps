using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public static class VxBindExtensions
    {
        public static T BindText<T>(this T entry, VxState<string> state, VxBindingMode mode = VxBindingMode.Default) where T : Entry
        {
            VxBindings.SetBinding(entry, Entry.TextProperty, new VxBinding()
            {
                State = state,
                BindingMode = mode
            });

            return entry;
        }

        public static T BindSelectedItem<T>(this T picker, VxState state, VxBindingMode mode = VxBindingMode.Default) where T : Picker
        {
            VxBindings.SetBinding(picker, Picker.SelectedItemProperty, new VxBinding()
            {
                State = state,
                BindingMode = mode
            });

            return picker;
        }

        public static T BindSelectedItem<T>(this T picker, string propertyPath, object source, VxBindingMode mode = VxBindingMode.Default) where T : Picker
        {
            VxBindings.SetBinding(picker, Picker.SelectedItemProperty, new VxBinding()
            {
                PropertyPath = propertyPath,
                Source = source,
                BindingMode = mode
            });

            return picker;
        }

        public static T BindDate<T>(this T picker, VxState<DateTime> state) where T : DatePicker
        {
            VxBindings.SetBinding(picker, DatePicker.DateProperty, new VxBinding()
            {
                State = state
            });

            return picker;
        }

        public static T BindCurrentItem<T>(this T carouselView, VxState state) where T : CarouselView
        {
            VxBindings.SetBinding(carouselView, CarouselView.CurrentItemProperty, new VxBinding
            {
                State = state
            });

            return carouselView;
        }

        public static T BindPosition<T>(this T carouselView, VxState<int> state) where T : CarouselView
        {
            VxBindings.SetBinding(carouselView, CarouselView.PositionProperty, new VxBinding
            {
                State = state
            });

            return carouselView;
        }
    }
}
