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
    public class DroidScheduleTileExtension : ScheduleTileExtension
    {
        public override bool IsPinned(Guid localAccountId)
        {
            // Isn't relevant for Droid
            return true;
        }

        public override Task PinTile(AccountDataItem account, AccountDataStore data)
        {
            // Isn't relevant for Droid
            return Task.FromResult(true);
        }

        public override Task UnpinTile(Guid localAccountId)
        {
            // Isn't relevant for Droid
            return Task.FromResult(true);
        }

        public override Task UpdateScheduleTile(AccountDataItem account, AccountDataStore data)
        {
            WidgetsHelper.UpdateScheduleWidget();
            return Task.FromResult(true);
        }
    }
}