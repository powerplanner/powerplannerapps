using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.Extensions;
using System.Threading.Tasks;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Extensions
{
    public class iOSInAppPurchaseExtension : InAppPurchaseExtension
    {
        public static bool IsNewPurchaseOperation;
        public static event EventHandler<InAppPurchaseHelper.PurchaseResponse> ResponseReceived;

        public override Task<bool> OwnsInAppPurchaseAsync()
        {
            return Task.FromResult(false);
        }

        public override async Task<bool> PromptPurchase()
        {
            Task<InAppPurchaseHelper.PurchaseResponse> responseTask;

            if (IsNewPurchaseOperation)
            {
                responseTask = InAppPurchaseHelper.PurchaseAsync();
            }
            else
            {
                responseTask = InAppPurchaseHelper.RestoreAsync();
            }

            var response = await responseTask;
            ResponseReceived?.Invoke(null, response);

            return response.Success;
        }
    }
}