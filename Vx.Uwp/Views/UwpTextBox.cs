using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpTextBox : UwpView<Vx.Views.TextBox, TextBox>
    {
        public UwpTextBox()
        {
            View.TextChanged += View_TextChanged;
        }

        private void View_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = View.Text;
            }
        }

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Header = newView.Header;
            View.Text = newView.Text?.Value ?? "";
        }
    }
}
