using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IVxButton
    {
        string Text { set; }

        Action ClickAction { set; }
    }

    public class VxButton : VxView, IVxButton
    {
        public string Text
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public Action ClickAction
        {
            get => GetProperty<Action>();
            set => SetProperty(value);
        }

        public VxButton(string text)
        {
            Text = text;
        }
    }

    public static class VxButtonExtensions
    {
        public static T Text<T>(this T button, string value) where T : VxButton
        {
            button.Text = value;
            return button;
        }

        public static T Click<T>(this T button, Action value) where T : VxButton
        {
            button.ClickAction = value;
            return button;
        }
    }
}
