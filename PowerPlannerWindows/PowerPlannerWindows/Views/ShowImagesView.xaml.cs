using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using System;
using Microsoft.UI.Xaml;

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
