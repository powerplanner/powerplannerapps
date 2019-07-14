using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers
{
    public class SyncErrorsViewController : PopupViewControllerWithScrolling<SyncErrorsViewModel>
    {
        public SyncErrorsViewController()
        {
            Title = "Sync Errors";
        }

        public override void OnViewModelLoadedOverride()
        {
            // Padding at top
            StackView.AddArrangedSubview(new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            }.SetHeight(16));

            foreach (var error in ViewModel.SyncErrors)
            {
                // In future should make this text selectable but that requires UITextView or UITextField but none of those
                // worked well here, so just switching to label for now
                var label = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = error.Name + "\n" + error.Date + "\n" + error.Message,
                    Lines = 0
                };
                StackView.AddArrangedSubview(label);
                label.StretchWidth(StackView, left: 16, right: 16);

                // Padding
                StackView.AddArrangedSubview(new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                }.SetHeight(16));
            }

            base.OnViewModelLoadedOverride();
        }
    }
}