using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Schedule;
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

namespace PowerPlannerUWP.Views
{
    public sealed partial class ExportSchedulePopupView : PopupViewHostGeneric
    {
        public new ExportSchedulePopupViewModel ViewModel
        {
            get { return base.ViewModel as ExportSchedulePopupViewModel; }
            set { base.ViewModel = value; }
        }

        public ExportSchedulePopupView()
        {
            this.InitializeComponent();

            TextBlockTitle.Text = LocalizedResources.GetString("String_ExportToImage");
            TextBlockDescription.Text = LocalizedResources.GetString("String_ScheduleExportToImage_Description");
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ListViewShareItems.ItemsSource = ViewModel.ShareItems;
            CanvasForRenderingSchedule.Children.Add(ViewModel.Element);

            ViewModel.PanelForPrinting = GridForPrinting;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                (e.ClickedItem as ExportSchedulePopupViewModel.ShareItem).ClickAction();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private class ShareItem
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }

            public Action ClickAction { get; set; }
        }
    }
}
