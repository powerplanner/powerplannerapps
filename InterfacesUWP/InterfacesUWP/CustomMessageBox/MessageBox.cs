using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacesUWP
{
    public enum MessageBoxButton
    {
        OK,
        OKCancel
    }

    public class MessageBox
    {
        public static CustomMessageBox Show(string message) { return Show(message, null); }

        public static CustomMessageBox Show(string message, string title) { return Show(message, title, MessageBoxButton.OK); }

        public static CustomMessageBox Show(string message, string title, MessageBoxButton buttons)
        {
            CustomMessageBox mb = null;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    mb = new CustomMessageBox(message, title, "OK");
                    break;

                case MessageBoxButton.OKCancel:
                    mb = new CustomMessageBox(message, title, "OK", "Cancel");
                    break;
            }

            mb.Show();
            return mb;
        }
    }
}
