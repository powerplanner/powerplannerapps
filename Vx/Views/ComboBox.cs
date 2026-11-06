using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class ComboBox : View
    {
        public string Header { get; set; }

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// This will only change if the object reference itself changes. If you add an item to the list, it won't be reflected.
        /// </summary>
        public IEnumerable Items { get; set; }

        public VxValue<object> SelectedItem { get; set; }

        public Func<object, View> ItemTemplate { get; set; }
    }
}
