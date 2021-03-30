using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxUwpButton : VxUwpNativeView<VxButton, Button>, IVxButton
    {
        protected override void Initialize()
        {
            base.Initialize();

            NativeView.Click += NativeView_Click;
        }

        private void NativeView_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            View.ClickAction?.Invoke();
        }

        public string Text { set => NativeView.Content = value; }
        public Action ClickAction { set { } }
    }
}
