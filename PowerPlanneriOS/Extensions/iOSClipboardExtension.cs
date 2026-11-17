using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.Extensions;
using UIKit;

namespace PowerPlanneriOS.Extensions
{
    public class iOSClipboardExtension : ClipboardExtension
    {
        public override Task SetTextAsync(string text)
        {
            UIPasteboard.General.String = text ?? "";
            return Task.CompletedTask;
        }
    }
}
