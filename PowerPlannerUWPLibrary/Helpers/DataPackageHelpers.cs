using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace PowerPlannerUWPLibrary.Helpers
{
    public static class DataPackageHelpers
    {
        public static BaseViewItem GetViewItem(DataPackageView dataView)
        {
            object obj;
            dataView.Properties.TryGetValue("ViewItem", out obj);
            return obj as BaseViewItem;
        }

        public static T GetViewItem<T>(DataPackageView dataView) where T : BaseViewItem
        {
            return GetViewItem(dataView) as T;
        }
    }
}
