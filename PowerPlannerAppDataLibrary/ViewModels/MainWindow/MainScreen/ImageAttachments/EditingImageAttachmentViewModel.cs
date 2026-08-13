using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments
{
    public class EditingExistingImageAttachmentViewModel : BaseEditingImageAttachmentViewModel
    {
        public EditingExistingImageAttachmentViewModel(BaseViewModel parent, string imageName, Action<BaseEditingImageAttachmentViewModel> removeImageAttachment) : base(parent, removeImageAttachment)
        {
            ImageAttachment = new ImageAttachmentViewModel(imageName);
        }
    }

    public class EditingNewImageAttachmentViewModel : BaseEditingImageAttachmentViewModel
    {
        public IFile TempFile { get; private set; }

        public EditingNewImageAttachmentViewModel(BaseViewModel parent, IFile tempFile, Action<BaseEditingImageAttachmentViewModel> removeImageAttachment) : base(parent, removeImageAttachment)
        {
            TempFile = tempFile;
            ImageAttachment = new ImageAttachmentViewModel(TempFile);
        }
    }

    public class BaseEditingImageAttachmentViewModel : BaseMainScreenViewModelDescendant
    {
        private Action<BaseEditingImageAttachmentViewModel> _removeImageAttachment;
        public BaseEditingImageAttachmentViewModel(BaseViewModel parent, Action<BaseEditingImageAttachmentViewModel> removeImageAttachment) : base(parent)
        {
            _removeImageAttachment = removeImageAttachment;
        }

        public ImageAttachmentViewModel ImageAttachment { get; protected set; }

        public void RemoveThisImageAttachment()
        {
            _removeImageAttachment(this);
        }
    }
}
