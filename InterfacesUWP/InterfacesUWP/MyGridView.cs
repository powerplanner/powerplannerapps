using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    class MyGridView : GridView
    {
        private object _itemsSource;
        public new object ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;

                if (value == null)
                    base.ItemsSource = null;

                else
                {
                    
                }
            }
        }
    }
}
