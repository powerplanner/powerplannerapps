using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
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

        protected override void ApplyProperties(ImageView oldView, ImageView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.Equals(newView.Source, oldView?.Source))
            {
                if (newView.Source != null)
                {
                    if (newView.Source is UriImageSource uriSource)
                    {
                        View.Source = new BitmapImage(new Uri(uriSource.Uri));
                    }
                }
            }
        }
    }
}
