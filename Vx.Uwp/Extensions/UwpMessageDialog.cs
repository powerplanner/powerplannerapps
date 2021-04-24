using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Popups;

namespace InterfacesUWP.Extensions
{
    public class UwpMessageDialog
    {
        public static async Task ShowAsync(PortableMessageDialog portableDialog)
        {
            var dialog = new MessageDialog(portableDialog.Content);
            if (portableDialog.Title != null)
            {
                dialog.Title = portableDialog.Title;
            }

            await dialog.ShowAsync();
        }
    }
}
