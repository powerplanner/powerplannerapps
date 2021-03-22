using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IVxTextBlock
    {
        string Text { set; }
    }

    public class VxTextBlock : VxView, IVxTextBlock
    {
        public string Text
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public VxTextBlock()
        {

        }

        public VxTextBlock(string text)
        {
            Text = text;
        }
    }

    public static class VxTextBlockExtensions
    {
        public static T Text<T>(this T textBlock, string value) where T : VxTextBlock
        {
            textBlock.Text = value;
            return textBlock;
        }
    }
}
