using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxNativeTextBlock : VxUwpNativeView<VxTextBlock, TextBlock>, IVxTextBlock
    {

        public VxNativeTextBlock(VxTextBlock view) : base(view)
        {

        }

        public string Text { set => NativeView.Text = value; }
    }
}
