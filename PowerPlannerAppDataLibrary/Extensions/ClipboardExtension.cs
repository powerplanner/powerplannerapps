using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class ClipboardExtension
    {
        public static ClipboardExtension Current { get; set; }

        public abstract Task SetTextAsync(string text);
    }
}
