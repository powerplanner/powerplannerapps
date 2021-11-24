using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class InAppPurchaseExtension
    {
        public static InAppPurchaseExtension Current;

        public abstract Task<bool> OwnsInAppPurchaseAsync();

        public abstract Task<bool> PromptPurchase();
    }

    /// <summary>
    /// Thrown when there was an in app purchase error but the exception has already been handled and logged. Should still display info to end user though.
    /// </summary>
    public class InAppPurchaseHandledException : Exception { }
}
