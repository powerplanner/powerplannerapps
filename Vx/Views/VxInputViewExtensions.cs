using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public static class VxInputViewExtensions
    {
        public static T BindText<T>(this T entry, VxState<string> value) where T : InputView
        {
            SetBindText(entry, value);
            return entry;
        }

        public static readonly BindableProperty BindTextProperty = BindableProperty.CreateAttached("BindText", typeof(VxState<string>), typeof(InputView), null, defaultBindingMode: BindingMode.OneTime, propertyChanged: TextPropertyChanged);

        public static VxState<string> GetBindText(BindableObject target)
        {
            return target.GetValue(BindTextProperty) as VxState<string>;
        }

        public static void SetBindText(BindableObject target, VxState<string> value)
        {
            target.SetValue(BindTextProperty, value);
        }

        private static void TextPropertyChanged(BindableObject sender, object oldVal, object newVal)
        {
            var entry = sender as Entry;

            if (newVal != null)
            {
                entry.Text = (newVal as VxState<string>).Value;
            }

            if (oldVal == null)
            {
                entry.TextChanged += Entry_TextChanged;
            }
        }

        private static void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            var textState = GetBindText(entry);
            if (textState != null)
            {
                textState.Value = e.NewTextValue;
            }
        }
    }
}
