using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.NativeViews;
using Vx.Views;

namespace Vx.Uwp
{
    public static class VxNative
    {
        public static void Initialize()
        {
            VxNativeView.Mappings[typeof(VxTextBlock)] = typeof(VxNativeTextBlock);
            VxNativeView.Mappings[typeof(VxTextBox)] = typeof(VxNativeTextBox);
            VxNativeView.Mappings[typeof(VxStackPanel)] = typeof(VxUwpStackPanel);

            VxDispatcher.Current = new VxUwpDispatcher();
        }
    }
}
