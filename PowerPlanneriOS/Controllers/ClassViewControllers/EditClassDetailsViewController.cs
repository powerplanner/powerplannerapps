﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class EditClassDetailsViewController : PopupViewControllerWithScrolling<EditClassDetailsViewModel>
    {
        public EditClassDetailsViewController()
        {
            // NOTE: This view isn't used anymore, I simply use AddClass with details enabled instead

            Title = PowerPlannerResources.GetString("String_EditDetails");

            BackButtonText = PowerPlannerResources.GetStringCancel();
            PositiveNavBarButton = new PopupRightNavBarButtonItem(PowerPlannerResources.GetStringSave(), delegate { ViewModel.Save(); });

            AddSectionDivider();

            var textViewDetails = new UITextView()
            {
                // Doesn't support placeholder: https://stackoverflow.com/questions/1328638/placeholder-in-uitextview
                ScrollEnabled = false,
                Editable = true
            };
            StackView.AddArrangedSubview(textViewDetails);
            textViewDetails.StretchWidth(StackView);
            textViewDetails.SetMinimumHeight(60);
            BindingHost.SetTextViewTextBinding(textViewDetails, nameof(ViewModel.Details));

            AddSectionDivider();
        }
    }
}