using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using StoreKit;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlanneriOS.Helpers
{
    public static class InAppPurchaseHelper
    {
        private const string FULL_VERSION_ID = "com.barebonesdev.powerplanner.fullversion";
        private static PurchaseRequester _requester = new PurchaseRequester(FULL_VERSION_ID);

        private static async Task<SKProduct> GetFullVersionProductAsyncHelper()
        {
            var products = await ProductsRequester.RequestProductsAsync(new List<string>() { FULL_VERSION_ID });

            return products.FirstOrDefault();
        }

        private static Task<SKProduct> _getFullVersionProductTask;
        /// <summary>
        /// Will not throw. Returns null if can't obtain.
        /// </summary>
        /// <returns></returns>
        public static async Task<SKProduct> GetFullVersionProductAsync()
        {
            try
            {
                if (_getFullVersionProductTask == null || _getFullVersionProductTask.IsFaulted)
                {
                    _getFullVersionProductTask = GetFullVersionProductAsyncHelper();
                }

                return await _getFullVersionProductTask;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                return null;
            }
        }

        /// <summary>
        /// Returns the localized price string, or null if failed to get product
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetPriceAsync()
        {
            var product = await GetFullVersionProductAsync();

            if (product == null)
            {
                return null;
            }

            return LocalizedPrice(product);
        }

        private static string LocalizedPrice(SKProduct product)
        {
            var formatter = new NSNumberFormatter
            {
                FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
                NumberStyle = NSNumberFormatterStyle.Currency,
                Locale = product.PriceLocale,
            };

            string formattedString = formatter.StringFromNumber(product.Price);
            return formattedString;
        }

        /// <summary>
        /// Verify that the iTunes account can make this purchase for this application. Check this before requesting products or purchases.
        /// </summary>
        public static bool CanMakePayments
        {
            get { return SKPaymentQueue.CanMakePayments; }
        }

        /// <summary>
        /// Will not throw. Error info contained in response.
        /// </summary>
        /// <returns></returns>
        public static async Task<PurchaseResponse> PurchaseAsync()
        {
            try
            {
                _requester.PurchaseProduct();
                return await _requester.Task;
            }
            catch (Exception ex)
            {
                return new PurchaseResponse()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        /// <summary>
        /// Will not throw. Error info contained in response.
        /// </summary>
        /// <returns></returns>
        public static async Task<PurchaseResponse> RestoreAsync()
        {
            try
            {
                _requester.RestoreProduct();
                return await _requester.Task;
            }
            catch (Exception ex)
            {
                return new PurchaseResponse()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        public class PurchaseResponse
        {
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private class PurchaseRequester : SKPaymentTransactionObserver
        {
            private TaskCompletionSource<PurchaseResponse> _completionSource = new TaskCompletionSource<PurchaseResponse>();
            private string _appStoreProductId;
            public Task<PurchaseResponse> Task
            {
                get { return _completionSource.Task; }
            }

            public PurchaseRequester(string appStoreProductId)
            {
                _appStoreProductId = appStoreProductId;
                SKPaymentQueue.DefaultQueue.AddTransactionObserver(this);
            }

            ~PurchaseRequester()
            {
                System.Diagnostics.Debug.WriteLine("Destroyed PurchaseRequester");
            }

            public void PurchaseProduct()
            {
                _completionSource.TrySetCanceled();
                _completionSource = new TaskCompletionSource<PurchaseResponse>();
                SKPayment payment = SKPayment.CreateFrom(_appStoreProductId);
                SKPaymentQueue.DefaultQueue.AddPayment(payment);
            }

            public void RestoreProduct()
            {
                _completionSource.TrySetCanceled();
                _completionSource = new TaskCompletionSource<PurchaseResponse>();
                SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
            }

            public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
            {
                _completionSource.TrySetResult(new PurchaseResponse()
                {
                    Success = false,
                    Error = error.LocalizedDescription
                });
            }

            public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
            {
                // If it restored, it should have already been set (so this call will do nothing). Otherwise, there was nothing to restore.
                _completionSource.TrySetResult(new PurchaseResponse()
                {
                    Success = false,
                    Error = "No previous purchases to restore."
                });
            }

            /// <summary>
            /// Called when the transaction status is updated
            /// </summary>
            /// <param name="queue"></param>
            /// <param name="transactions"></param>
            public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
            {
                foreach (var t in transactions)
                {
                    if ((t.OriginalTransaction?.Payment?.ProductIdentifier != null && t.OriginalTransaction.Payment.ProductIdentifier.Equals(_appStoreProductId))
                        || (t.Payment?.ProductIdentifier != null && t.Payment.ProductIdentifier.Equals(_appStoreProductId)))
                    {
                        if (t.TransactionState == SKPaymentTransactionState.Purchased || t.TransactionState == SKPaymentTransactionState.Restored)
                        {
                            SKPaymentQueue.DefaultQueue.FinishTransaction(t);

                            var currAccount = PowerPlannerApp.Current.GetCurrentAccount();

                            if (currAccount != null)
                            {
                                var dontBlock = currAccount.SetAsLifetimePremiumAsync();
                            }

                            _completionSource.TrySetResult(new PurchaseResponse()
                            {
                                Success = true
                            });
                            return;
                        }

                        else if (t.TransactionState == SKPaymentTransactionState.Failed)
                        {
                            SKPaymentQueue.DefaultQueue.FinishTransaction(t);
                            _completionSource.TrySetResult(new PurchaseResponse()
                            {
                                Success = false,
                                Error = t.Error.LocalizedDescription
                            });
                        }
                    }
                }

                if (transactions.Length == 0)
                {
                    _completionSource.TrySetResult(new PurchaseResponse()
                    {
                        Success = false,
                        Error = "No previous purchases to restore."
                    });
                }
            }
        }

        private class ProductsRequester : SKProductsRequestDelegate
        {
            protected SKProductsRequest ProductsRequest { get; set; }
            private TaskCompletionSource<SKProduct[]> _completionSource = new TaskCompletionSource<SKProduct[]>();
            public Task<SKProduct[]> Task
            {
                get { return _completionSource.Task; }
            }

            // request multiple products at once
            private void RequestProductData(List<string> productIds)
            {
                NSString[] array = productIds.Select(pId => (NSString)pId).ToArray();
                NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);

                //set up product request for in-app purchase
                ProductsRequest = new SKProductsRequest(productIdentifiers);
                ProductsRequest.Delegate = this; // SKProductsRequestDelegate.ReceivedResponse
                ProductsRequest.Start();
            }

            public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
            {
                SKProduct[] products = response.Products;

                foreach (string invalidProductId in response.InvalidProducts)
                    Console.WriteLine("Invalid product id: {0}", invalidProductId);

                _completionSource.SetResult(products);
            }

            public static Task<SKProduct[]> RequestProductsAsync(List<string> productIds)
            {
                var requester = new ProductsRequester();
                requester.RequestProductData(productIds);
                return requester.Task;
            }
        }
    }
}