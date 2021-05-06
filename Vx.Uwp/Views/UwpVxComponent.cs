using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml;

namespace Vx.Uwp.Views
{
    public class UwpVxComponent : UwpView<VxComponent, FrameworkElement>
    {
        public UwpVxComponent(VxComponent component) : base(component.Render())
        {
        }
    }
}
