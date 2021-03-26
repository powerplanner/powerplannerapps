using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxNativeTextBox : VxNativeView<VxTextBox, TextBox>, IVxTextBox
    {
        public VxNativeTextBox(VxTextBox view) : base(view, new TextBox())
        {
            NativeView.TextChanged += NativeView_TextChanged;
        }

        private void NativeView_TextChanged(object sender, TextChangedEventArgs e)
        {
            View.Text.Value = NativeView.Text;
        }

        public VxState<string> Text
        {
            set => SetBindable(value, (s) => NativeView.Text = s);
        }

        public string Header { set => NativeView.Header = value; }
    }
}
