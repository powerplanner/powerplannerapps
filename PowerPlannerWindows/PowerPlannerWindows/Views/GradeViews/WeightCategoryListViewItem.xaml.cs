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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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
