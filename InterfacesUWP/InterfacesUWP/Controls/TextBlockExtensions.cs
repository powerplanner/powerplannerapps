using InterfacesUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    /// <summary>
    /// Adds hyperlink and email address detection to text. From https://blogs.u2u.be/diederik/post/An-auto-hyperlinking-RichTextBlock-for-Windows-81-Store-apps
    /// </summary>
    public class TextBlockExtensions : DependencyObject
    {
        /// <summary>
        /// The raw text property.
        /// </summary>
        public static readonly DependencyProperty HyperlinkColorProperty =
            DependencyProperty.RegisterAttached("HyperlinkColor", typeof(Brush), typeof(TextBlockExtensions), new PropertyMetadata(null, OnRawTextChanged));

        /// <summary>
        /// Gets the raw text.
        /// </summary>
        public static Brush GetHyperlinkColor(DependencyObject obj)
        {
            return obj.GetValue(HyperlinkColorProperty) as Brush;
        }

        /// <summary>
        /// Sets the raw text.
        /// </summary>
        public static void SetHyperlinkColor(DependencyObject obj, Brush value)
        {
            obj.SetValue(HyperlinkColorProperty, value);
        }

        /// <summary>
        /// The raw text property.
        /// </summary>
        public static readonly DependencyProperty RawTextProperty =
            DependencyProperty.RegisterAttached("RawText", typeof(string), typeof(TextBlockExtensions), new PropertyMetadata("", OnRawTextChanged));

        /// <summary>
        /// Gets the raw text.
        /// </summary>
        public static string GetRawText(DependencyObject obj)
        {
            return obj.GetValue(RawTextProperty) as string;
        }

        /// <summary>
        /// Sets the raw text.
        /// </summary>
        public static void SetRawText(DependencyObject obj, string value)
        {
            obj.SetValue(RawTextProperty, value);
        }

        /// <summary>
        /// Called when raw text changed.
        /// </summary>
        private static void OnRawTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string rawText = GetRawText(d);
            Brush hyperlinkColor = GetHyperlinkColor(d);

            TextBlock tb = d as TextBlock;
            if (tb == null)
            {
                throw new InvalidOperationException("Object must be TextBlock");
            }

            try
            {
                tb.Inlines.Clear();

                if (rawText == null || rawText.Length == 0)
                {
                    return;
                }

                foreach (var inline in TextToRichInlinesHelper.Convert(rawText, hyperlinkColor))
                {
                    tb.Inlines.Add(inline);
                }
            }
            catch (Exception ex)
            {
                tb.Text = ex.ToString();
            }
        }
    }
}
