using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.Extensions;
using System.Threading.Tasks;
using PowerPlanneriOS.Helpers;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlanneriOS.Extensions
{
    public class iOSInAppPurchaseExtension : InAppPurchaseExtension
    {
        public override Task<bool> OwnsInAppPurchaseAsync()
        {
            return Task.FromResult(Settings.OwnsInAppPurchase);
        }

        public override bool CanMakePayments => InAppPurchaseHelper.CanMakePayments;

        public override Task<string> GetPriceAsync() => InAppPurchaseHelper.GetPriceAsync();

        public override bool SupportsRestore => true;

        public override Task<bool> PromptPurchase()
        {
            return CompleteAsync(InAppPurchaseHelper.PurchaseAsync());
        }

        public override Task<bool> PromptRestore()
        {
            return CompleteAsync(InAppPurchaseHelper.RestoreAsync());
        }

        private static async Task<bool> CompleteAsync(Task<InAppPurchaseHelper.PurchaseResponse> responseTask)
        {
            var response = await responseTask;

            if (!response.Success && !string.IsNullOrEmpty(response.Error))
            {
                throw new InAppPurchaseException(response.Error);
            }

            return response.Success;
        }
    }
}