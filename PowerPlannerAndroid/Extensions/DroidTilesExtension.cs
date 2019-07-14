using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAndroid.Helpers;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidTilesExtension : TilesExtension
    {
        public override Task UnpinAllTilesForAccount(Guid localAccountId)
        {
            // Not implemented on Droid
            return Task.FromResult(true);
        }

        public override Task UpdatePrimaryTileNotificationsAsync()
        {
            // This gets called upon account log off
            WidgetsHelper.UpdateAllWidgets();
            return Task.FromResult(true);
        }

        public override Task UpdateTileNotificationsForAccountAsync(AccountDataItem account, AccountDataStore data)
        {
            WidgetsHelper.UpdateAllWidgets();
            return Task.FromResult(true);
        }
    }
}