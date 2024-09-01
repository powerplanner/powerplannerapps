using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    internal class iOSZoomableImageView : iOSView<Vx.Views.ZoomableImageView, UIScrollView>
    {
        private iOSImageView _imageView;

        public iOSZoomableImageView()
        {
            View.MaximumZoomScale = 4;
            View.MinimumZoomScale = 1;

            _imageView = new iOSImageView();

            _imageView.View.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(_imageView.View);
            // Constrain the UIImageView to the UIScrollView
            NSLayoutConstraint.ActivateConstraints([
                _imageView.View.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _imageView.View.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _imageView.View.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _imageView.View.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _imageView.View.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                _imageView.View.HeightAnchor.ConstraintEqualTo(View.HeightAnchor)
            ]);

            View.ViewForZoomingInScrollView = (scrollView) => _imageView.View;
        }

        protected override void ApplyProperties(ZoomableImageView oldView, ZoomableImageView newView)
        {
            base.ApplyProperties(oldView, newView);

            _imageView.Apply(newView);
        }
    }
}
