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
    public class DroidEditImagesView : DroidView<EditImagesView, EditingImageAttachmentsWrapGrid>
    {
        public DroidEditImagesView() : base(new EditingImageAttachmentsWrapGrid(VxDroidExtensions.ApplicationContext))
        {
            View.RequestedAddImage += View_RequestedAddImage;
        }

        private void View_RequestedAddImage(object sender, EventArgs e)
        {
            VxView?.RequestAddImage?.Invoke();
        }

        protected override void ApplyProperties(EditImagesView oldView, EditImagesView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.ImageAttachments = newView.Attachments;
        }
    }
}