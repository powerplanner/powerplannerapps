using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class SaveGradeScaleViewController : PopupViewControllerWithScrolling<SaveGradeScaleViewModel>
    {
        public SaveGradeScaleViewController()
        {
            Title = "Save Grade Scale";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: true);

            AddBottomSectionDivider();

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { Save(); });
        }

        private void Save()
        {
            ViewModel.Save();
        }
    }
}
