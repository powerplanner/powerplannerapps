using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.Extensions;
using Xamarin.InAppBilling;
using System.Threading.Tasks;

namespace PowerPlannerAndroid.Extensions
{
    public class AndroidInAppPurchaseExtension : InAppPurchaseExtension
    {
        private const string PublicKey = "";
        public override Task<bool> OwnsInAppPurchaseAsync()
        {
            MainActivity activity = MainActivity.GetCurrent();
            if (activity != null)
            {
                return activity.OwnsInAppPurchase();
            }

            return Task.FromResult(false);
        }

        public override Task<bool> PromptPurchase()
        {
            return MainActivity.GetCurrent()?.PromptPurchase();
        }
    }

    public class MyInAppBillingAssistant
    {
        private Task _connectTask;
        private InAppBillingServiceConnection _connection;
        private Activity _activity;
        private const string PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn+JtIuthU0LU9ykFZkixwHASG2EkGXwWLAn0KdavQV1QmRbz5QtSvmlPqTkjQiZ6Ku2oMvkgUQAsfMhqR7LzJm87ltpSGGePMOWAGn5budmcdJ5bw59MigfeC58HxOv7wAN/yW8ewDOPLDy62OUovh9EvS09PZkL9BjdQWt+CuBu18ny26bXsKDq2OrXAAt/MVNfLIYsYvc+UuQZpFNmZcAgPn1FHwyp16RvRSWj9de73d/djpovIJbcfOaHUwrW2UCO1vNgBt2MtlqQf8x7a0eNLn/SZY3gXREzed3tQK9lGO/7C/QgyP2NLAqSBKHHaFQcjeJcNBJLA1WG9XekJQIDAQAB";
        private const string ProductId = "barebonesdev.powerplanner.premiumversion";

        private static bool? _cachedOwnsInAppPurchase;

        public MyInAppBillingAssistant(Activity activity)
        {
            _activity = activity;
        }

        private Task Connect()
        {
            if (_connectTask != null)
                return _connectTask;

            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            _connectTask = completionSource.Task;

            _connection = new InAppBillingServiceConnection(_activity, PublicKey);
            _activity = null;
            _connection.OnConnected += delegate
            {
                var cs = completionSource;
                completionSource = null;
                cs?.SetResult(true);
            };
            _connection.OnInAppBillingError += delegate
            {
                var cs = completionSource;
                completionSource = null;
                cs?.SetException(new Exception("InAppBillingError"));
            };
            _connection.Connect();
            return _connectTask;
        }

        public async Task<bool> OwnsInAppPurchase()
        {
            if (_cachedOwnsInAppPurchase != null)
                return _cachedOwnsInAppPurchase.Value;

            await Connect();
            var purchases = _connection.BillingHandler.GetPurchases(ItemType.Product);

            var answer = purchases.Any(i => i.ProductId.Equals(ProductId) && i.PurchaseState == 0);
            _cachedOwnsInAppPurchase = answer;
            return answer;
        }

        public async Task<bool> PromptPurchase()
        {
            await Connect();
            _cachedOwnsInAppPurchase = null;
            
            _connection.BillingHandler.BuyProduct(ProductId, ItemType.Product, "");
            return false; // Couldn't figure out how to get the event response, the event handlers didn't seem to trigger.
            // So we'll just return false for now, and the user can go back.
        }

        public void Disconnect()
        {
            if (_connection != null)
            {
                _connection.Disconnect();
                _connection = null;
                _activity = null;
            }
        }
    }
}