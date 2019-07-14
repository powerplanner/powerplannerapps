using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsUniversal;
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

namespace PowerPlannerUWP.Views
{
    public sealed partial class ImagesWrapGrid : UserControl
    {
        public ImagesWrapGrid()
        {
            this.InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageAttachmentViewModel image = grid.SelectedItem as ImageAttachmentViewModel;

            if (image == null)
                return;

            grid.SelectedItem = null;

            IEnumerable<ImageAttachmentViewModel> allImages = DataContext as IEnumerable<ImageAttachmentViewModel>;
            if (allImages == null)
            {
                return;
            }

            PowerPlannerApp.Current.ShowImage(image, allImages.ToArray());
        }
    }
}
