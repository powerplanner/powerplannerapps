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
        public static void BindText(UILabel label, INotifyPropertyChanged source, string propertyName)
        {
            Action applyFromData = delegate
            {
                label.Text = source.GetType().GetProperty(propertyName).GetValue(source) as string;
            };

            source.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>((s, e) =>
            {
                applyFromData();
            }).Handler;

            applyFromData();
        }
    }
}