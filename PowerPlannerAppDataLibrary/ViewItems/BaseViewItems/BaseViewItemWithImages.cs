using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemWithImages : BaseViewItemWithDetails
    {
        public BaseViewItemWithImages(Guid identifier) : base(identifier) { }
        public BaseViewItemWithImages(BaseDataItemWithImages dataItem) : base(dataItem)
        {
            _imageAttachments = new Lazy<ImageAttachmentViewModel[]>(GetImageAttachments);
        }

        private string[] _imageNames;
        public string[] ImageNames
        {
            get { return _imageNames; }
            private set { _imageAttachments = new Lazy<ImageAttachmentViewModel[]>(GetImageAttachments); SetProperty(ref _imageNames, value, nameof(ImageNames), nameof(ImageAttachments)); }
        }

        private Lazy<ImageAttachmentViewModel[]> _imageAttachments = new Lazy<ImageAttachmentViewModel[]>();
        public ImageAttachmentViewModel[] ImageAttachments => _imageAttachments.Value;

        private ImageAttachmentViewModel[] GetImageAttachments()
        {
            if (ImageNames == null)
            {
                return new ImageAttachmentViewModel[0];
            }

            return ImageNames.Select(i => new ImageAttachmentViewModel(i)).ToArray();
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemWithImages i = dataItem as BaseDataItemWithImages;

            ImageNames = i.ImageNames;
        }
    }
}
