using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vx.Views;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Vx.Uwp.Views
{
    public class UwpImageView : UwpView<Vx.Views.ImageView, Image>
    {
        public UwpImageView()
        {
            View.Stretch = Windows.UI.Xaml.Media.Stretch.Uniform;
        }

        private CancellationTokenSource _imageSourceCanceller;
        protected override async void ApplyProperties(ImageView oldView, ImageView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.Equals(newView.Source, oldView?.Source) || !object.Equals(newView.UseFilePictureViewThumbnail, oldView?.UseFilePictureViewThumbnail))
            {
                if (_imageSourceCanceller != null)
                {
                    _imageSourceCanceller.Cancel();
                    _imageSourceCanceller = null;
                }

                if (newView.UseFilePictureViewThumbnail)
                {
                    _imageSourceCanceller = new CancellationTokenSource();
                    View.Source = null;
                    View.Source = await GetThumbnailSource(newView.Source, _imageSourceCanceller.Token);
                }
                else
                {
                    View.Source = GetSource(newView.Source);
                }
            }
        }

        internal static Windows.UI.Xaml.Media.ImageSource GetSource(ImageSource source)
        {
            if (source is UriImageSource uriSource)
            {
                return new BitmapImage(new Uri(uriSource.UwpUri));
            }

            return null;
        }

        internal static async Task<Windows.UI.Xaml.Media.ImageSource> GetThumbnailSource(ImageSource source, CancellationToken cancellationToken)
        {
            if (source is UriImageSource uriSource)
            {
                try
                {
                    StorageFile nativeFile = await StorageFile.GetFileFromPathAsync(uriSource.UwpUri);
                    cancellationToken.ThrowIfCancellationRequested();

                    if (nativeFile != null)
                    {
                        BitmapImage bmp = new BitmapImage();

                        StorageItemThumbnail thumbnail = await nativeFile.GetThumbnailAsync(ThumbnailMode.PicturesView);
                        cancellationToken.ThrowIfCancellationRequested();

                        bmp.UriSource = null;
                        await bmp.SetSourceAsync(thumbnail);
                        cancellationToken.ThrowIfCancellationRequested();

                        return bmp;
                    }
                }
                catch { }
            }

            return null;
        }
    }
}
