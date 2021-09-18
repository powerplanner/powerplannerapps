using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Views
{
    public class EditImagesView : View
    {
        public ObservableCollection<BaseEditingImageAttachmentViewModel> Attachments { get; set;}

        public Action RequestAddImage { get; set; }
    }
}
