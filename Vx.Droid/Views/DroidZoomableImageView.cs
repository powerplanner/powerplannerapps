using ImageViews.Photo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace Vx.Droid.Views
{
    internal class DroidZoomableImageView : DroidView<Vx.Views.ZoomableImageView, PhotoView>
    {
        public DroidZoomableImageView() : base(new PhotoView(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(ZoomableImageView oldView, ZoomableImageView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.Equals(newView.Source, oldView?.Source))
            {
                DroidImageView.SetImageSource(View, newView.Source);
            }
        }
    }
}
