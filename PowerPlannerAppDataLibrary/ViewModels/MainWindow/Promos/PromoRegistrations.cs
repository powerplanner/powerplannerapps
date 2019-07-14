using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos
{
    public static class PromoRegistrations
    {
        private static bool _hasBeenRequestedThisSession;
        public static readonly List<Type> Registrations = new List<Type>()
        {
            // Right now only UWP adds any registrations, it does so in PowerPlannerUwpApp.cs
        };

        /// <summary>
        /// Will spin off a separate thread and wait for a bit and then see which promotion should be shown. This must be called from UI thread.
        /// </summary>
        /// <param name="account"></param>
        public static async void StartPromoLogic(AccountDataItem account)
        {
            if (_hasBeenRequestedThisSession)
            {
                return;
            }
            _hasBeenRequestedThisSession = true;

            if (Registrations.Count == 0)
            {
                return;
            }

            try
            {
                // Run this logic on background thread
                PromoRegistration promoToShow = await Task.Run(async delegate
                {
                    // First wait a bit to no slow down app initialization
                    await Task.Delay(3000);

                    foreach (var regType in Registrations.Distinct())
                    {
                        try
                        {
                            var reg = (PromoRegistration)Activator.CreateInstance(regType);
                            if (await reg.ShouldShowAsync(account))
                            {
                                return reg;
                            }
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    }

                    return null;
                });

                if (promoToShow != null)
                {
                    PowerPlannerApp.Current.ShowPopup((parentViewModel) => promoToShow.Create(account, parentViewModel));
                    promoToShow.MarkShown(account);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }

    public abstract class PromoRegistration
    {
        /// <summary>
        /// This will be called on a background thread
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public abstract Task<bool> ShouldShowAsync(AccountDataItem account);

        public abstract BaseViewModel Create(AccountDataItem account, BaseViewModel parent);

        public abstract void MarkShown(AccountDataItem account);
    }
}
