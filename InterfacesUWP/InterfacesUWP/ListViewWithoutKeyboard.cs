using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public class ListViewWithoutKeyboard : ListView
    {
        protected override void OnKeyDown(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
