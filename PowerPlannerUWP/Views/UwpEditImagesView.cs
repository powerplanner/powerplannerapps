using PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;

namespace PowerPlannerUWP.Views
{
    public class UwpEditImagesView : UwpView<EditImagesView, EditImagesWrapGrid>
    {
        public UwpEditImagesView()
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
