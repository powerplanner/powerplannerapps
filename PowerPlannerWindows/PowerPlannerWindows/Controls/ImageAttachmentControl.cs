using InterfacesUWP;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PowerPlannerUWP.Controls
{
    public class ImageAttachmentThumbnailControl : ImageAttachmentControl
    {
        private Image _image;
        protected override bool UseThumbnail => true;

        public ImageAttachmentThumbnailControl()
        {
            _image = new Image();
            base.Children.Add(_image);
        }

        protected override void SetImageSource(ImageSource source)
        {
            _image.Source = source;
        }
    }

    public class ImageAttachmentZoomableControl : ImageAttachmentControl
    {
        private ZoomableImage _image;
        protected override bool UseThumbnail => false;

        public ImageAttachmentZoomableControl()
        {
            _image = new ZoomableImage();
            base.Children.Add(_image);
        }

        protected override void SetImageSource(ImageSource source)
        {
            _image.Source = source;
        }
    }

    public abstract class ImageAttachmentControl : Grid
    {
        protected abstract bool UseThumbnail { get; }

        public ImageAttachmentControl()
        {
            Background = new SolidColorBrush(Colors.White);
        }

        public ImageAttachmentViewModel ViewModel
        {
            get { return (ImageAttachmentViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ImageAttachmentViewModel), typeof(ImageAttachmentControl), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ImageAttachmentControl).OnViewModelChanged(e);
        }

        private void OnViewModelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyPropertyChanged old)
            {
                old.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (ViewModel != null)
            {
                Update();
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.StartLoad();
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Status):
                    Update();
                    break;
            }
        }

        private void Update()
        {
            switch (ViewModel.Status)
            {
                case ImageAttachmentStatus.NotStarted:
                    break;

                case ImageAttachmentStatus.Loaded:
                    DisplayImage(ViewModel.File);
                    break;

                case ImageAttachmentStatus.Downloading:
                    DisplayLoadingImage();
                    break;

                case ImageAttachmentStatus.Offline:
                    DisplayOffline();
                    break;

                default:
                    DisplayNotFound();
                    break;
            }
        }

        private void DisplayLoadingImage()
        {
            SetImageSource(new BitmapImage(new Uri("ms-appx:///Assets/ImageLoading.jpg")));
        }

        private async void DisplayImage(IFile file)
        {
            try
            {
                if (UseThumbnail)
                {
                    StorageFile nativeFile = await StorageFile.GetFileFromPathAsync(file.Path);
                    if (nativeFile != null)
                    {
                        BitmapImage bmp = new BitmapImage();
                        await GetImageThumbnail(nativeFile, bmp);
                        SetImageSource(bmp);
                        return;
                    }

                }
                else
                {
                    SetImageSource(new BitmapImage(new Uri(file.Path)));
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static async Task GetImageThumbnail(StorageFile file, BitmapImage bmp)
        {
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.PicturesView);

            bmp.UriSource = null;
            await bmp.SetSourceAsync(thumbnail);
        }

        private void DisplayNotFound()
        {
            SetImageSource(new BitmapImage(new Uri("ms-appx:///Assets/ImageNotFound.jpg")));
        }

        private void DisplayOffline()
        {
            SetImageSource(new BitmapImage(new Uri("ms-appx:///Assets/ImageOffline.jpg")));
        }

        protected abstract void SetImageSource(ImageSource source);
    }
}
