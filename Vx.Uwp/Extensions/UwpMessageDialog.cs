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
        public static async Task<bool> ShowAsync(PortableMessageDialog portableDialog)
        {
            var dialog = new MessageDialog(portableDialog.Content);
            if (portableDialog.Title != null)
            {
                dialog.Title = portableDialog.Title;
            }

            IUICommand positiveCommand = null;

            if (portableDialog.PositiveText != null)
            {
                positiveCommand = new UICommand(portableDialog.PositiveText);
                dialog.Commands.Add(positiveCommand);
            }

            if (portableDialog.NegativeText != null)
            {
                dialog.Commands.Add(new UICommand(portableDialog.NegativeText));
            }

            return await dialog.ShowAsync() == positiveCommand;
        }
    }
}
