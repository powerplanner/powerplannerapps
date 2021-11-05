using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow
{
    public class PremiumVersionViewModel : BaseViewModel
    {
        public PremiumVersionViewModel(BaseViewModel parent, string contextualMessage) : base(parent)
        {
            ContextualMessage = contextualMessage;
        }

        public string ContextualMessage { get; private set; }

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
                TelemetryExtension.Current?.TrackException(ex);

                var dontWait = new PortableMessageDialog("Something went wrong. If you can't purchase on this device, try a different device, and it will sync the premium status with your online account.", "Failed to purchase premium version").ShowAsync();
            }
        }
    }
}
