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
    }
}
