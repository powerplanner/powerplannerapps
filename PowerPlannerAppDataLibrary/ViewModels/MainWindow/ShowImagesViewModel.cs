using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Components.ImageAttachments;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow
{
    public class ShowImagesViewModel : PopupComponentViewModel
    {
        private Func<int, View> _itemTemplate;

        public ShowImagesViewModel(BaseViewModel parent, ImageAttachmentViewModel image, ImageAttachmentViewModel[] allImages) : base(parent)
        {
            AllImages = allImages;
            _index = Array.IndexOf(allImages, image);
            _itemTemplate = RenderImage;
        }

        public ImageAttachmentViewModel[] AllImages { get; set; }

        private int _index = 0;

        protected override View Render()
        {
            return new SlideView
            {
                Position = VxValue.Create(_index, i => { _index = i; MarkDirty(); }),
                ItemTemplate = _itemTemplate,
                MinPosition = 0,
                MaxPosition = AllImages.Length - 1,
                BackgroundColor = Color.Black,
                ShowMouseArrowIndicatorsOnHover = true
            };
        }

        private View RenderImage(int index)
        {
            var image = AllImages[index];
            return new ImageComponent()
            {
                ImageAttachment = image
            };
        }
    }
}
