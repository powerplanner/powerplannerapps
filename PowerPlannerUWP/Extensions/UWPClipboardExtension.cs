using PowerPlannerAppDataLibrary.Extensions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace PowerPlannerUWP.Extensions
{
    public class UWPClipboardExtension : ClipboardExtension
    {
        public override Task SetTextAsync(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text ?? "");
            Clipboard.SetContent(dataPackage);
            return Task.CompletedTask;
        }
    }
}
