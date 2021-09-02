using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp
{
    public class UwpNativeComponent : ContentControl, INativeComponent
    {
        public SizeF ComponentSize => new SizeF(this.ActualSize.X, this.ActualSize.Y);

        public event EventHandler<SizeF> ComponentSizeChanged;

        public UwpNativeComponent(VxComponent component)
        {
            Component = component;
            HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;

            SizeChanged += UwpNativeComponent_SizeChanged;
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
