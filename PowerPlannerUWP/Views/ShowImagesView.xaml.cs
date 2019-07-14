using InterfacesUWP;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerUWPLibrary;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShowImagesView : ViewHostGeneric
    {
        public new ShowImagesViewModel ViewModel
        {
            get { return base.ViewModel as ShowImagesViewModel; }
            set { base.ViewModel = value; }
        }

        public ShowImagesView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                flipView.ItemsSource = ViewModel.AllImages;

                flipView.SelectedItem = ViewModel.CurrentImage;

                flipView.Focus(FocusState.Programmatic);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
