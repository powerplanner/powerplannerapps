using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.Converters;
using InterfacesiOS.Controllers;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class EditClassCreditsViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassCreditsViewModel>
    {
        public EditClassCreditsViewController()
        {
            Title = "Credits";

            PowerPlannerUIHelper.ConfigureForInputsStyle(this);

            var cancelButton = new UIBarButtonItem()
            {
                Title = "Cancel"
            };
            cancelButton.Clicked += new WeakEventHandler<EventArgs>(CancelButton_Clicked).Handler;
            NavigationItem.LeftBarButtonItem = cancelButton;

            var saveButton = new UIBarButtonItem()
            {
                Title = "Save"
            };
            saveButton.Clicked += new WeakEventHandler<EventArgs>(SaveButton_Clicked).Handler;
            NavigationItem.RightBarButtonItem = saveButton;

            StackView.AddSectionDivider();

            var textFieldCredits = new UITextField()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                KeyboardType = UIKeyboardType.DecimalPad,
                AdjustsFontSizeToFitWidth = true,
                Placeholder = "ex: 3"
            };
            BindingHost.SetTextFieldTextBinding<double>(textFieldCredits, nameof(ViewModel.Credits), converter: CreditsToTextBoxTextConverter.Convert, backConverter: CreditsToTextBoxTextConverter.ConvertBack);
            AddTextField(StackView, textFieldCredits, firstResponder: true);

            StackView.AddSectionDivider();
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.Save();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.TryRemoveViewModelViaUserInteraction();
        }
    }
}