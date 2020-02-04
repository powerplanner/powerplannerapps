using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos
{
    public class PromoContributeViewModel : BaseViewModel
    {
        public class Registration : PromoRegistration
        {
            public override async Task<bool> ShouldShowAsync(AccountDataItem account)
            {
                if (account == null)
                {
                    return false;
                }

                // If we've already shown
                if (Helpers.Settings.HasShownPromoContribute)
                {
                    return false;
                }

                var dataStore = await AccountDataStore.Get(account.LocalAccountId);

                // If they actually have lots of tasks
                using (await Locks.LockDataForReadAsync())
                {
                    return dataStore.ActualTableMegaItems.Count() > 60;
                }
            }

            public override BaseViewModel Create(AccountDataItem account, BaseViewModel parent)
            {
                return new PromoContributeViewModel(parent);
            }

            public override void MarkShown(AccountDataItem account)
            {
                Helpers.Settings.HasShownPromoContribute = true;
            }
        }

        public PromoContributeViewModel(BaseViewModel parent) : base(parent)
        {
        }
    }
}
