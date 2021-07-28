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
