using PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;

namespace PowerPlannerUWP.Views
{
    public class UwpImagesView : UwpView<ImagesView, ImagesWrapGrid>
    {
        public UwpImagesView()
        {
        }

        protected override void ApplyProperties(ImagesView oldView, ImagesView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.DataContext = newView.ImageAttachments;
        }
    }
}
