using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp
{
    public static class VxAppExtensions
    {
        public static UIElement Render(this VxApp app)
        {
            return new TextBlock();
        }
    }
}
