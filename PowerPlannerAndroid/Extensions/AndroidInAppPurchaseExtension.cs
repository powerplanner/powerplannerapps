using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using PowerPlannerAppDataLibrary.Extensions;
using System.Threading.Tasks;
using Android.BillingClient.Api;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.SyncLayer;

namespace PowerPlannerAndroid.Extensions
{
    public class AndroidInAppPurchaseExtension : InAppPurchaseExtension
    {
        public override Task<bool> OwnsInAppPurchaseAsync()
        {
            return AndroidBillingAssistant.Current.OwnsInAppPurchaseAsync();
        }

        /// <summary>
        /// This SHOULD throw if error like failed to connect occurs. The app will display a generic error message if exception occurs.
        /// </summary>
        /// <returns></returns>
        public override Task<bool> PromptPurchase()
        {
            return AndroidBillingAssistant.Current.PromptPurchase();
        }
    }

    public class AndroidBillingAssistant : Java.Lang.Object, IPurchasesResponseListener
    {
        public static AndroidBillingAssistant Current = new AndroidBillingAssistant();

        private static BillingClient _billingClient;
        private static bool _isConnected;
        private static object _lock = new object();
        private static TaskCompletionSource<BillingClient> InitializeTaskCompletionSource;
        private static bool? _cachedOwnsInAppPurchase;
        private const string ProductId = "barebonesdev.powerplanner.premiumversion";
        private static TaskCompletionSource<bool> _purchaseTaskCompletionSource;
        private static TaskCompletionSource<bool> _ownsInAppPurchaseTaskCompletionSource;

        public async Task<bool> OwnsInAppPurchaseAsync()
        {
            if (_cachedOwnsInAppPurchase != null)
            {
                return _cachedOwnsInAppPurchase.Value;
            }

            TelemetryExtension.Current?.TrackEvent("CheckingInAppPurchase");

            var client = await GetClientAsync(MainActivity.GetCurrent());

            _ownsInAppPurchaseTaskCompletionSource = new TaskCompletionSource<bool>();
            client.QueryPurchasesAsync(BillingClient.SkuType.Inapp, this);
            return await _ownsInAppPurchaseTaskCompletionSource.Task;
        }

        public void OnQueryPurchasesResponse(BillingResult result, IList<Purchase> purchases)
        {
            PurchasesUpdatedListener(result, purchases);
        }

        /// <summary>
        /// This SHOULD throw if error like failed to connect occurs. The app will display a generic error message if exception occurs.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PromptPurchase()
        {
            TelemetryExtension.Current?.TrackEvent("PromptingPurchase");

            var mainActivity = MainActivity.GetCurrent();
            var client = await GetClientAsync(mainActivity);

            // First we have to query to get the product details https://developer.android.com/google/play/billing/integrate#show-products
            var skuDetailsParams = SkuDetailsParams.NewBuilder()
                .SetSkusList(new List<string>() { ProductId })
                .SetType(BillingClient.SkuType.Inapp)
                .Build();

            var skuDetails = await _billingClient.QuerySkuDetailsAsync(skuDetailsParams);

            if (skuDetails.Result.ResponseCode != BillingResponseCode.Ok)
            {
                TrackBillingError(skuDetails.Result);
                throw new InAppPurchaseHandledException();
            }

            var product = skuDetails.SkuDetails.FirstOrDefault();
            if (product == null)
            {
                throw new Exception("Failed to find the product.");
            }

            // Then we can prompt purchase flow https://developer.android.com/google/play/billing/integrate#launch
            var billingFlowParams = BillingFlowParams.NewBuilder()
                .SetSkuDetails(product)
                .Build();

            _purchaseTaskCompletionSource = new TaskCompletionSource<bool>();
            var response = client.LaunchBillingFlow(mainActivity, billingFlowParams);
            if (response.ResponseCode != BillingResponseCode.Ok)
            {
                _purchaseTaskCompletionSource = null;
                TrackBillingError(response);
                throw new InAppPurchaseHandledException();
            }

            // PurchasesUpdatedListener will set this 
            return await _purchaseTaskCompletionSource.Task;
        }

        /// <summary>
        /// Returns null if couldn't connect
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Task<BillingClient> GetClientAsync(Context context)
        {
            lock (_lock)
            {
                if (InitializeTaskCompletionSource != null)
                {
                    if (InitializeTaskCompletionSource.Task.IsCompleted && _isConnected)
                    {
                        return InitializeTaskCompletionSource.Task;
                    }

                    if (!InitializeTaskCompletionSource.Task.IsCompleted)
                    {
                        return InitializeTaskCompletionSource.Task;
                    }

                    // Otherwise it disconnected, we'll re-connect
                }

                InitializeTaskCompletionSource = new TaskCompletionSource<BillingClient>();
                GetClientHelperAsync(context);
                return InitializeTaskCompletionSource.Task;
            }
        }

        /// <summary>
        /// This only gets called if billingClient is null, or billingClient has been disconnected. Only need to lock when setting InitializeTaskCompletionSource.
        /// </summary>
        /// <param name="context"></param>
        private static async void GetClientHelperAsync(Context context)
        {
            if (_billingClient == null)
            {
                var builder = BillingClient.NewBuilder(context);
                builder.SetListener(PurchasesUpdatedListener);
                builder.EnablePendingPurchases(); // This is required unfortunately
                _billingClient = builder.Build();
            }

            try
            {
                var response = await _billingClient.StartConnectionAsync(OnDisconnected);
                lock (_lock)
                {
                    if (response.ResponseCode == BillingResponseCode.Ok)
                    {
                        _isConnected = true;

                        InitializeTaskCompletionSource.TrySetResult(_billingClient);
                    }
                    else
                    {
                        TrackBillingError(response);

                        InitializeTaskCompletionSource.TrySetException(new InAppPurchaseHandledException());
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    InitializeTaskCompletionSource.TrySetException(ex);
                }
            }
        }

        private static void OnDisconnected()
        {
            lock (_lock)
            {
                _isConnected = false;
            }
        }

        private static async void PurchasesUpdatedListener(BillingResult result, IList<Purchase> purchases)
        {
            try
            {
                if (result.ResponseCode == BillingResponseCode.Ok || result.ResponseCode == BillingResponseCode.ItemAlreadyOwned)
                {
                    var premium = purchases.FirstOrDefault(i => i.Skus.Contains(ProductId));

                    if (premium != null && premium.PurchaseState == PurchaseState.Purchased)
                    {
                        _cachedOwnsInAppPurchase = true;

                        if (_purchaseTaskCompletionSource != null)
                        {
                            _purchaseTaskCompletionSource.TrySetResult(true);
                            _purchaseTaskCompletionSource = null;
                        }

                        if (_ownsInAppPurchaseTaskCompletionSource != null)
                        {
                            TrackInAppOwnershipStatus(owned: true);

                            _ownsInAppPurchaseTaskCompletionSource.TrySetResult(true);
                            _ownsInAppPurchaseTaskCompletionSource = null;
                        }

                        // Purchases need to be acknowledged https://developer.android.com/google/play/billing/integrate#process
                        if (!premium.IsAcknowledged)
                        {
                            var acknowledgePurchaseParams = AcknowledgePurchaseParams.NewBuilder()
                                .SetPurchaseToken(premium.PurchaseToken)
                                .Build();

                            await _billingClient.AcknowledgePurchaseAsync(acknowledgePurchaseParams);

                            TelemetryExtension.Current?.TrackEvent("PurchasedPremium");
                        }

                        // If their account isn't already premium, update their online account as purchased
                        try
                        {
                            var currAccount = PowerPlannerApp.Current.GetCurrentAccount();
                            if (currAccount != null && !currAccount.IsLifetimePremiumAccount)
                            {
                                var dontBlock = Sync.SetAsPremiumAccount(currAccount);
                            }
                        }
                        catch { }

                        return;
                    }
                }

                // Track telemetry first
                switch (result.ResponseCode)
                {
                    case BillingResponseCode.Ok:
                    case BillingResponseCode.ItemNotOwned:
                        if (_ownsInAppPurchaseTaskCompletionSource != null)
                        {
                            TrackInAppOwnershipStatus(owned: false);
                        }
                        break;

                    default:
                        TrackBillingError(result);
                        break;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            // In all other cases, we set it to false
            _cachedOwnsInAppPurchase = false;

            if (_purchaseTaskCompletionSource != null)
            {
                _purchaseTaskCompletionSource.TrySetResult(false);
                _purchaseTaskCompletionSource = null;
            }

            if (_ownsInAppPurchaseTaskCompletionSource != null)
            {
                _ownsInAppPurchaseTaskCompletionSource.TrySetResult(false);
                _ownsInAppPurchaseTaskCompletionSource = null;
            }
        }

        private static void TrackInAppOwnershipStatus(bool owned)
        {
            TelemetryExtension.Current?.TrackEvent("InAppOwnershipStatus", new Dictionary<string, string>()
            {
                { "Owned", owned ? "1" : "0" }
            });
        }

        private static void TrackBillingError(BillingResult result)
        {
            /** There's a few classes of responses...
               - Errors that are expected and I can't do anything about (they need to sign in to Play Store, etc... I should still track just in case they increase dramatically)
                 - (3) BILLING_UNAVAILABLE
                 - (-2) FEATURE_NOT_SUPPORTED
                 - (-3) SERVICE_TIMEOUT
                 - (-1) SERVICE_DISCONNECTED
                 - (2) SERVICE_UNAVAILALBE (Network is down)

               - Errors that are unexpected
                 - (5) DEVELOPER_ERROR
                 - (6) ERROR
                 - (4) ITEM_UNAVAILABLE
            **/

            Dictionary<string, string> telemParmas = new Dictionary<string, string>()
            {
                { "ErrorCode", ((int)result.ResponseCode).ToString() }
            };

            // Only log error messages for ones that we need the message for, ones that are unexpected
            switch (result.ResponseCode)
            {
                case BillingResponseCode.DeveloperError:
                case BillingResponseCode.Error:
                case BillingResponseCode.ItemUnavailable:
                    telemParmas.Add("ErrorMessage", result.DebugMessage);
                    break;
            }

            TelemetryExtension.Current?.TrackEvent("BillingError", telemParmas);
        }
    }
}