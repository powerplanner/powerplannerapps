using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public static class VxInputViewExtensions
    {
        public static T BindText<T>(this T entry, VxState<string> value, VxBindingMode bindingMode = VxBindingMode.Default) where T : InputView
        {
            SetBindText(entry, new VxBinding<string>
            {
                State = value,
                BindingMode = bindingMode
            });
            return entry;
        }

        public static readonly BindableProperty BindTextProperty = BindableProperty.CreateAttached("BindText", typeof(VxBinding<string>), typeof(InputView), null, defaultBindingMode: BindingMode.OneTime, propertyChanged: TextPropertyChanged);

        public static VxBinding<string> GetBindText(BindableObject target)
        {
            return target.GetValue(BindTextProperty) as VxBinding<string>;
        }

        public static void SetBindText(BindableObject target, VxBinding<string> value)
        {
            target.SetValue(BindTextProperty, value);
        }

        private static void TextPropertyChanged(BindableObject sender, object oldVal, object newVal)
        {
            var entry = sender as Entry;

            if (newVal != null)
            {
                entry.Text = (newVal as VxBinding<string>).State?.Value;
            }

            if (oldVal == null)
            {
                entry.TextChanged += Entry_TextChanged;
            }
        }

        private static void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            GetBindText(entry)?.SetValue(e.NewTextValue);
        }
    }
}
