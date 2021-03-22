using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxNativeTextBlock : VxNativeView<VxTextBlock, TextBlock>, IVxTextBlock
    {

        public VxNativeTextBlock(VxTextBlock view) : base(view, new TextBlock())
        {

        }

        public string Text { set => NativeView.Text = value; }
    }
}
