using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Droid;
using Vx.Droid.Views;

namespace PowerPlannerAndroid.Views
{
    public class DroidImagesView : DroidView<ImagesView, ImageAttachmentsWrapGrid>
    {
        public DroidImagesView() : base(new ImageAttachmentsWrapGrid(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(ImagesView oldView, ImagesView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.ImageAttachments = newView.ImageAttachments;
        }
    }
}