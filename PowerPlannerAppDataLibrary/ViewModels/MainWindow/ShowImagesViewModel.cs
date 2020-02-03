using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow
{
    public class ShowImagesViewModel : BaseViewModel
    {
        public ShowImagesViewModel(BaseViewModel parent, ImageAttachmentViewModel image, ImageAttachmentViewModel[] allImages) : base(parent)
        {
            AllImages = allImages;
            CurrentImage = image;
        }

        public ImageAttachmentViewModel[] AllImages { get; set; }

        public ImageAttachmentViewModel CurrentImage { get; set; }
    }
}
