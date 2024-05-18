using System;
using Foundation;
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

            if (!object.Equals(newView.Source, oldView?.Source))
            {
                if (newView.Source is UriImageSource uriImageSource && uriImageSource.IosBundleName != null)
                {
                    View.Image = UIImage.FromBundle(uriImageSource.IosBundleName);
                }
                else
                {
                    View.Image = null;
                }
            }
        }
    }
}