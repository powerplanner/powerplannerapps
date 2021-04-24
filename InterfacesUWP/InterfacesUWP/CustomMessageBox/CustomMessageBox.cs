using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    public class CustomMessageBox : CustomMessageBoxWithContent
    {
        public CustomMessageBox(string message) : this(message, null) { }

        public CustomMessageBox(string message, string title) : this(message, title, "OK") { }

        public CustomMessageBox(string message, string title, params string[] buttons) : base(null, title, buttons)
        {
            if (message != null)
            {
                Content = MessageGenerator.Generate(message);
            }
        }
    }
}
