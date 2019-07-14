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
}
