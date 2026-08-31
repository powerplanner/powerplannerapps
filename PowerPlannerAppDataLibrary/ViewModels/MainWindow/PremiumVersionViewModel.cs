using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow
{
    public class PremiumVersionViewModel : PopupComponentViewModel
    {
        public PremiumVersionViewModel(BaseViewModel parent, string contextualMessage) : base(parent)
        {
            ContextualMessage = contextualMessage;
            Title = PowerPlannerResources.GetString("Settings_UpdateToPremium.Title");
        }

        public string ContextualMessage { get; private set; }

        private string Price { get => GetState<string>(); set => SetState(value); }

        private string ErrorMessage { get => GetState<string>(); set => SetState(value); }

        private bool IsBusy { get => GetState<bool>(); set => SetState(value); }

        protected override void Initialize()
        {
            base.Initialize();

            if (!InAppPurchaseExtension.Current.CanMakePayments)
            {
                ErrorMessage = "App Store purchases not supported.";
            }
            else
            {
                LoadPrice();
            }
        }

        private async void LoadPrice()
        {
            try
            {
                Price = await InAppPurchaseExtension.Current.GetPriceAsync();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override View Render()
        {
            bool isEnabled = !IsBusy && InAppPurchaseExtension.Current.CanMakePayments;

            string upgradeText = PowerPlannerResources.GetString("Settings_UpgradeToPremium_ButtonUpgrade.Content");
            if (!string.IsNullOrEmpty(Price))
            {
                upgradeText += " - " + Price;
            }

            return new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                Children =
                {
                    new ScrollView
                    {
                        Content = new LinearLayout
                        {
                            Children = 
                            {
                                !string.IsNullOrWhiteSpace(ContextualMessage) ? new TextBlock
                                {
                                    Text = ContextualMessage,
                                    FontWeight = FontWeights.Bold,
                                    WrapText = true,
                                    Margin = new Thickness(0, 0, 0, 16)
                                } : null,

                                new TextBlock
                                {
                                    Text = PowerPlannerResources.GetStringPremiumDescription(0, 1, 2, 3),
                                    WrapText = true
                                },

                                !string.IsNullOrEmpty(ErrorMessage) ? new TextBlock
                                {
                                    Text = ErrorMessage,
                                    TextColor = Color.Red,
                                    FontWeight = FontWeights.Bold,
                                    WrapText = true,
                                    Margin = new Thickness(0, 16, 0, 0)
                                } : null
                            }
                        }
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Text = upgradeText,
                        Margin = new Thickness(0, 24, 0, 0),
                        Click = () => _ = PurchaseAsync(isNewPurchase: true),
                        IsEnabled = isEnabled
                    },

                    InAppPurchaseExtension.Current.SupportsRestore ? new AccentButton
                    {
                        Text = PowerPlannerResources.GetString("Settings_UpgradeToPremium_ButtonRestore.Content"),
                        Margin = new Thickness(0, 16, 0, 0),
                        Click = () => _ = PurchaseAsync(isNewPurchase: false),
                        IsEnabled = isEnabled
                    } : null
                }
            };
        }

        /// <summary>
        /// Legacy entry point used by the native Android and UWP popup views. iOS uses the Vx
        /// <see cref="Render"/> path (with inline price/restore/error handling) instead.
        /// </summary>
        public async void PromptPurchase()
        {
            try
            {
                if (await InAppPurchaseExtension.Current?.PromptPurchase())
                {
                    RemoveViewModel();
                }
            }

            catch (Exception ex)
            {
                // Only log error if unknown, unlogged error
                if (!(ex is InAppPurchaseHandledException))
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                var dontWait = new PortableMessageDialog("Something went wrong. If you can't purchase on this device, try a different device, and it will sync the premium status with your online account.", "Failed to purchase premium version").ShowAsync();
            }
        }

        private async Task PurchaseAsync(bool isNewPurchase)
        {
            if (IsBusy)
            {
                return;
            }

            ErrorMessage = null;

            try
            {
                IsBusy = true;

                bool success = isNewPurchase
                    ? await InAppPurchaseExtension.Current.PromptPurchase()
                    : await InAppPurchaseExtension.Current.PromptRestore();

                if (success)
                {
                    RemoveViewModel();
                }
            }
            catch (InAppPurchaseException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (InAppPurchaseHandledException)
            {
                ErrorMessage = "Something went wrong. If you can't purchase on this device, try a different device, and it will sync the premium status with your online account.";
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                ErrorMessage = "Something went wrong. If you can't purchase on this device, try a different device, and it will sync the premium status with your online account.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
