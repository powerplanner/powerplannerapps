using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp
{
    public partial class UwpNativeComponent : ContentControl, INativeComponent
    {
        public SizeF ComponentSize => new SizeF((float)this.ActualWidth, (float)this.ActualHeight);

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;
        public event EventHandler<bool> MouseOverChanged;

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
                PointerCaptureLost += UwpNativeComponent_PointerCaptureLost;
            }

            // Listen for the ViewModel being permanently removed so we can break
            // the strong reference from this native component to the VxComponent,
            // allowing it to be garbage collected.
            if (component is BaseViewModel viewModel)
            {
                viewModel.RemovedFromViewModel += new WeakEventHandler<EventArgs>(ViewModel_RemovedFromViewModel).Handler;
            }
        }

        private void ViewModel_RemovedFromViewModel(object sender, EventArgs e)
        {
            Component = null;
        }

        private void UwpNativeComponent_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // PointerCaptureLost needed for when using touch, so that when swiping left on calendar, mouse overs don't stay on
            if (Component != null && e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                Component.IsMouseOver.Value = false;
                MouseOverChanged?.Invoke(this, false);
            }
        }

        private void UwpNativeComponent_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (Component != null)
            {
                Component.IsMouseOver.Value = false;
                MouseOverChanged?.Invoke(this, false);
            }
        }

        private void UwpNativeComponent_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (Component != null && e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                Component.IsMouseOver.Value = true;
                MouseOverChanged?.Invoke(this, true);
            }
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
