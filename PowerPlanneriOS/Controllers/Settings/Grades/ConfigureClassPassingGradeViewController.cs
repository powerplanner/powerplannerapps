using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Controllers;
using InterfacesiOS.Converters;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassPassingGradeViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassPassingGradeViewModel>
    {
        public ConfigureClassPassingGradeViewController()
        {
            Title = PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title");

            PowerPlannerUIHelper.ConfigureForInputsStyle(this);

            var cancelButton = new UIBarButtonItem()
            {
                Title = PowerPlannerResources.GetStringCancel()
            };
            cancelButton.Clicked += new WeakEventHandler<EventArgs>(CancelButton_Clicked).Handler;
            NavigationItem.LeftBarButtonItem = cancelButton;

            var saveButton = new UIBarButtonItem()
            {
                Title = PowerPlannerResources.GetStringSave()
            };
            saveButton.Clicked += new WeakEventHandler<EventArgs>(SaveButton_Clicked).Handler;
            NavigationItem.RightBarButtonItem = saveButton;

            StackView.AddSectionDivider();

            var textFieldPassingGrade = new UITextField()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                KeyboardType = UIKeyboardType.DecimalPad,
                AdjustsFontSizeToFitWidth = true,
                Placeholder = PowerPlannerResources.GetExamplePlaceholderString(60.ToString())
            };
            BindingHost.SetTextFieldTextBinding<double>(textFieldPassingGrade, nameof(ViewModel.PassingGrade), converter: TextToDoubleConverter.Convert, backConverter: TextToDoubleConverter.ConvertBack);
            AddTextField(StackView, textFieldPassingGrade, firstResponder: true);

            StackView.AddSectionDivider();
            StackView.AddSpacing(16);

            var labelDescription = new UILabel()
            {
                Text = PowerPlannerResources.GetString("Settings_GradeOptions_PassingGrade_Explanation.Text"),
                Lines = 0,
                Font = UIFont.PreferredCaption1,
                TextColor = UIColor.LightGray
            };
            StackView.AddArrangedSubview(labelDescription);
            labelDescription.StretchWidth(StackView, left: 16, right: 16);

            StackView.AddSpacing(16);
            StackView.AddBottomSectionDivider();
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