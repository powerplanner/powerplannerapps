using Foundation;
using ImageIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSImageView : iOSView<Vx.Views.ImageView, UIImageView>
    {
        public iOSImageView()
        {
            View.ContentMode = UIViewContentMode.ScaleAspectFit;
        }

        protected override void ApplyProperties(ImageView oldView, ImageView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.Equals(newView.Source, oldView?.Source) || !object.Equals(newView.UseFilePictureViewThumbnail, oldView?.UseFilePictureViewThumbnail))
            {
                if (newView.UseFilePictureViewThumbnail)
                {
                    View.Image = GetThumbnailImage(newView.Source);
                }
                else
                {
                    View.Image = GetImage(newView.Source);
                }
            }
        }

        private UIImage GetImage(ImageSource source)
        {
            if (source is UriImageSource uriSource)
            {
                if (uriSource.IosBundleName != null)
                {
                    return UIImage.FromBundle(uriSource.IosBundleName);
                }
                else if (uriSource.IosFileName != null)
                {
                    return UIImage.FromFile(uriSource.IosFileName);
                }
            }

            return null;
        }

        private UIImage GetThumbnailImage(ImageSource source)
        {
            if (source is UriImageSource uriSource && uriSource.IosFileName != null)
            {
                using (var url = new NSUrl(uriSource.IosFileName, false))
                {
                    using (var imageSource = CGImageSource.FromUrl(url))
                    {
                        var cgImage = imageSource.CreateThumbnail(0, null);
                        return UIImage.FromImage(cgImage);
                    }
                }
            }

            return null;
        }
    }
}
