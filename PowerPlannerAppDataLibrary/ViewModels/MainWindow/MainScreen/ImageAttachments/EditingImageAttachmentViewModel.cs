using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments
{
    public class EditingExistingImageAttachmentViewModel : BaseEditingImageAttachmentViewModel
    {
        public EditingExistingImageAttachmentViewModel(AddHomeworkViewModel parent, string imageName) : base(parent)
        {
            ImageAttachment = new ImageAttachmentViewModel(imageName);
        }
    }

    public class EditingNewImageAttachmentViewModel : BaseEditingImageAttachmentViewModel
    {
        public IFile TempFile { get; private set; }

        public EditingNewImageAttachmentViewModel(AddHomeworkViewModel parent, IFile tempFile) : base(parent)
        {
            TempFile = tempFile;
            ImageAttachment = new ImageAttachmentViewModel(TempFile);
        }
    }

    public class BaseEditingImageAttachmentViewModel : BaseMainScreenViewModelDescendant
    {
        private AddHomeworkViewModel _addHomeworkViewModel;
        public BaseEditingImageAttachmentViewModel(AddHomeworkViewModel parent) : base(parent)
        {
            _addHomeworkViewModel = parent;
        }

        public ImageAttachmentViewModel ImageAttachment { get; protected set; }

        public void RemoveThisImageAttachment()
        {
            _addHomeworkViewModel.RemoveImageAttachment(this);
        }
    }
}
