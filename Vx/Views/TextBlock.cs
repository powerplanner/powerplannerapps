using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class TextBlock : View
    {
        public string Text { get; set; } = "";

        public FontWeights FontWeight { get; set; }

        public Color TextColor { get; set; } = Theme.Current.ForegroundColor;

        /// <summary>
        /// Wraps (true) by default.
        /// </summary>
        public bool WrapText { get; set; } = true;

        public float FontSize { get; set; } = Theme.Current.BodyFontSize;

        public bool Strikethrough { get; set; }

        public HorizontalAlignment TextAlignment { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// Gets or sets a value that indicates whether text selection is enabled. False by default. Note that iOS essentially ignores this (on iOS, only HyperlinkTextBlock supports text selection).
        /// </summary>
        public bool IsTextSelectionEnabled { get; set; } = false;
    }

    /// <summary>
    /// Enables link detection within a text block
    /// </summary>
    public class HyperlinkTextBlock : TextBlock
    {

    }

    public static class TextBlockExtensions
    {
        public static T TitleStyle<T>(this T textBlock) where T : TextBlock
        {
            textBlock.FontSize = Theme.Current.TitleFontSize;
            textBlock.FontWeight = FontWeights.SemiLight;
            return textBlock;
        }
    }
}
