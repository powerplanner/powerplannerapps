﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp
{
    public class UwpNativeComponent : ContentControl, INativeComponent
    {
        public SizeF ComponentSize => new SizeF(this.ActualSize.X, this.ActualSize.Y);

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;

        public UwpNativeComponent(VxComponent component)
        {
            Component = component;
            HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            VerticalContentAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;

            SizeChanged += UwpNativeComponent_SizeChanged;
            ActualThemeChanged += UwpNativeComponent_ActualThemeChanged;

            if (component.SubscribeToIsMouseOver)
            {
                PointerEntered += UwpNativeComponent_PointerEntered;
                PointerExited += UwpNativeComponent_PointerExited;
                //PointerCaptureLost += UwpNativeComponent_PointerCaptureLost;
            }
        }

        //private void UwpNativeComponent_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        //{
        //    Component.IsMouseOver.Value = false;
        //}

        private void UwpNativeComponent_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Component.IsMouseOver.Value = false;
        }

        private void UwpNativeComponent_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Component.IsMouseOver.Value = true;
        }

        private void UwpNativeComponent_ActualThemeChanged(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            ThemeChanged?.Invoke(this, null);
        }

        private void UwpNativeComponent_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ComponentSizeChanged?.Invoke(this, new SizeF((float)e.NewSize.Width, (float)e.NewSize.Height));
        }

        public VxComponent Component { get; private set; }

        public void ChangeView(View view)
        {
            Content = view.CreateFrameworkElement();
        }
    }
}
