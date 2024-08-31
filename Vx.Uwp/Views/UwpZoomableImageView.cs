using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vx.Views;

namespace Vx.Uwp.Views
{
    internal class UwpZoomableImageView : UwpView<Vx.Views.ZoomableImageView, ZoomableImage>
    {
        private CancellationTokenSource _imageSourceCanceller;
        protected override async void ApplyProperties(ZoomableImageView oldView, ZoomableImageView newView)
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
                    View.Source = await UwpImageView.GetThumbnailSource(newView.Source, _imageSourceCanceller.Token);
                }
                else
                {
                    View.Source = UwpImageView.GetSource(newView.Source);
                }
            }
        }
    }
}
