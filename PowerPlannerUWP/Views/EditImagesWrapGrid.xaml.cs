using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using ToolsPortable;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views
{
    public sealed partial class EditImagesWrapGrid : UserControl
    {
        public event EventHandler RequestedAddImage;
        private static object _newImageObject = new object();

        public IEnumerable<BaseEditingImageAttachmentViewModel> ImageAttachments
        {
            get { return (IEnumerable<BaseEditingImageAttachmentViewModel>)GetValue(ImageAttachmentsProperty); }
            set { SetValue(ImageAttachmentsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageAttachments.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageAttachmentsProperty =
            DependencyProperty.Register("ImageAttachments", typeof(IEnumerable<BaseEditingImageAttachmentViewModel>), typeof(EditImagesWrapGrid), new PropertyMetadata(null, OnImageAttachmentsChanged));

        private static void OnImageAttachmentsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EditImagesWrapGrid).OnImageAttachmentsChanged(e);
        }

        private void OnImageAttachmentsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ImageAttachments == null)
            {
                grid.ItemsSource = null;
            }
            else
            {
                grid.ItemsSource = new MyAppendedObservableLists<object>(ImageAttachments, new object[] { _newImageObject });
            }
        }



        public class EditImageTemplateSelector : DataTemplateSelector
        {
            private DataTemplate _dataTemplateEditImage, _dataTemplateAddImage;

            public EditImageTemplateSelector(DataTemplate dataTemplateEditImage, DataTemplate dataTemplateAddImage)
            {
                _dataTemplateEditImage = dataTemplateEditImage;
                _dataTemplateAddImage = dataTemplateAddImage;
            }

            protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            {
                if (item == _newImageObject)
                    return _dataTemplateAddImage;

                return _dataTemplateEditImage;
            }
        }

        public EditImagesWrapGrid()
        {
            this.InitializeComponent();

            grid.ItemTemplateSelector = new EditImageTemplateSelector(
                Resources["EditImageTemplate"] as DataTemplate,
                Resources["AddImageTemplate"] as DataTemplate);
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            BaseEditingImageAttachmentViewModel image = (sender as FrameworkElement).DataContext as BaseEditingImageAttachmentViewModel;

            if (image == null)
                return;

            image.RemoveThisImageAttachment();
        }

        private void buttonAdd_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                RequestedAddImage?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
