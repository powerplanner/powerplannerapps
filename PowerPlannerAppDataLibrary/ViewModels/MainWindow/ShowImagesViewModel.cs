using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

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

        private ImageAttachmentViewModel _currentImage;
        public ImageAttachmentViewModel CurrentImage
        {
            get { return _currentImage; }
            set { SetProperty(ref _currentImage, value, nameof(CurrentImage)); }
        }
    }

    public class ShowImagesPopupViewModel : PopupComponentViewModel
    {
        public ShowImagesPopupViewModel(BaseViewModel parent, ImageAttachmentViewModel image, ImageAttachmentViewModel[] allImages) : base(parent)
        {
            AllImages = allImages;
            CurrentImage = image;
        }

        public ImageAttachmentViewModel[] AllImages { get; set; }

        private ImageAttachmentViewModel _currentImage;
        public ImageAttachmentViewModel CurrentImage
        {
            get { return _currentImage; }
            set { SetProperty(ref _currentImage, value, nameof(CurrentImage)); }
        }

        private int _index = 0;

        protected override View Render()
        {
            return new SlideView
            {
                Position = VxValue.Create(_index, i => { _index = i; MarkDirty(); }),
                ItemTemplate = RenderImage,
                MinPosition = 0,
                MaxPosition = AllImages.Length - 1
            };
        }

        private View RenderImage(int index)
        {
            var image = AllImages[index];
            return RenderImage(image);
        }

        private View RenderImage(ImageAttachmentViewModel img)
        {
            if (img.Status != Helpers.ImageAttachmentStatus.Loaded)
            {
                return new Border();
            }

            return new ZoomableImageView
            {
                Source = UriImageSource.FromFilePath(img.File.Path)
            };
        }
    }
}
