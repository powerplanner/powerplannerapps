using Android.Content;
using PowerPlannerAppDataLibrary.Extensions;
using System.Threading.Tasks;
using Vx.Droid;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidClipboardExtension : ClipboardExtension
    {
        public override Task SetTextAsync(string text)
        {
            var clipboardManager = (ClipboardManager)VxDroidExtensions.ApplicationContext.GetSystemService(Context.ClipboardService);
            clipboardManager.PrimaryClip = ClipData.NewPlainText("Copied text", text ?? "");
            return Task.CompletedTask;
        }
    }
}
