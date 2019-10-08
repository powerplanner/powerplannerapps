using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerUWP.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.GradeViews
{
    public sealed partial class WeightCategoryListViewItem : UserControl
    {
        public event EventHandler OnRequestEdit;
        public event EventHandler<BaseViewItemMegaItem> OnRequestViewGrade;

        public WeightCategoryListViewItem()
        {
            this.InitializeComponent();
        }

        private void header_Click(object sender, RoutedEventArgs e)
        {
            triggerEdit();
        }

        private void triggerEdit()
        {
            if (OnRequestEdit != null)
                OnRequestEdit(this, null);
        }

        private void GradeListViewItem_OnRequestViewGrade(object sender, BaseViewItemMegaItem e)
        {
            OnRequestViewGrade?.Invoke(this, e);
        }
    }
}
