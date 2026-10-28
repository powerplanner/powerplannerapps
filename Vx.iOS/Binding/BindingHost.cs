using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.ComponentModel;
using ToolsPortable;
using InterfacesiOS.Views;
using CoreAnimation;
using System.Collections;
using CoreGraphics;
using BareMvvm.Core;
using BareMvvm.Core.Binding;

namespace InterfacesiOS.Binding
{
    public class BindingHost : BareMvvm.Core.Binding.BindingHost
    {
        public void SetBackgroundColorBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, byte[]> getValue)
        {
            SetBinding(propertyName, getValue, value => view.BackgroundColor = value != null ? BareUIHelper.ToColor(value) : null);
        }

        public void SetBackgroundColorBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, CGColor> getValue)
        {
            SetBinding(propertyName, getValue, value => view.BackgroundColor = value != null ? new UIColor(value) : null);
        }

        public void SetBackgroundColorBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, UIColor> getValue)
        {
            SetBinding(propertyName, getValue, value => view.BackgroundColor = value);
        }
    }
}