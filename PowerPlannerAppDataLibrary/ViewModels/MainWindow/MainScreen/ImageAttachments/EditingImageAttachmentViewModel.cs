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
        public EditingExistingImageAttachmentViewModel(AddTaskOrEventViewModel parent, string imageName) : base(parent)
        {
            ImageAttachment = new ImageAttachmentViewModel(imageName);
        }
    }

    public class EditingNewImageAttachmentViewModel : BaseEditingImageAttachmentViewModel
    {
        public IFile TempFile { get; private set; }

        public EditingNewImageAttachmentViewModel(AddTaskOrEventViewModel parent, IFile tempFile) : base(parent)
        {
            TempFile = tempFile;
            ImageAttachment = new ImageAttachmentViewModel(TempFile);
        }
    }

    public class BaseEditingImageAttachmentViewModel : BaseMainScreenViewModelDescendant
    {
        private AddTaskOrEventViewModel _addTaskOrEventViewModel;
        public BaseEditingImageAttachmentViewModel(AddTaskOrEventViewModel parent) : base(parent)
        {
            _addTaskOrEventViewModel = parent;
        }

        public ImageAttachmentViewModel ImageAttachment { get; protected set; }

        public void RemoveThisImageAttachment()
        {
            _addTaskOrEventViewModel.RemoveImageAttachment(this);
        }
    }
}
