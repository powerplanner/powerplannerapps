using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.ComponentModel;
using ToolsPortable;

namespace InterfacesiOS.Binding
{
    public static class LabelBinding
    {
        public static void BindText<TSource>(UILabel label, TSource source, string propertyName, Func<TSource, string> getValue) where TSource : INotifyPropertyChanged
        {
            Action applyFromData = delegate
            {
                label.Text = getValue(source);
            };

            source.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>((s, e) =>
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName)
                {
                    applyFromData();
                }
            }).Handler;

            applyFromData();
        }
    }
}