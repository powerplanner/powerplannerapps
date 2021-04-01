using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP.Controls
{
    public class TextBlockCompat : DependencyObject
    {
        public static readonly DependencyProperty StrikethroughProperty =
            DependencyProperty.RegisterAttached("Strikethrough", typeof(bool), typeof(TextBlockCompat), new PropertyMetadata(false, OnStrikethroughChanged));

        public static void SetStrikethrough(TextBlock element, bool value)
        {
            element.SetValue(StrikethroughProperty, value);
        }

        public static bool GetStrikethrough(TextBlock element)
        {
            return (bool)element.GetValue(StrikethroughProperty);
        }

        private static Lazy<bool> IsTextDecorationsPresent = new Lazy<bool>(delegate
        {
            // Added in 15063
            return ApiInformation.IsPropertyPresent(typeof(TextBlock).FullName, "TextDecorations");
        });

        private static void OnStrikethroughChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBlock textBlock && IsTextDecorationsPresent.Value)
            {
                textBlock.TextDecorations = GetStrikethrough(textBlock) ? Windows.UI.Text.TextDecorations.Strikethrough : Windows.UI.Text.TextDecorations.None;
            }
        }
    }
}
