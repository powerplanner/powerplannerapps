using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IVxTextBox
    {
        VxState<string> Text { set; }
        string Header { set; }
    }

    public class VxTextBox : VxView, IVxTextBox
    {
        public VxState<string> Text
        {
            get => GetProperty<VxState<string>>();
            set => SetProperty(value);
        }

        public string Header
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public VxTextBox Error(string value)
        {
            SetProperty(value);
            return this;
        }
    }

    public static class VxTextBoxExtensions
    {
        public static T Text<T>(this T textBlock, VxState<string> value) where T : VxTextBox
        {
            textBlock.Text = value;
            return textBlock;
        }
    }
}
