using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Converters;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class EditClassWeightCategoriesViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassWeightCategoriesViewModel>
    {
        private BareUIViewItemsSourceAdapterAsStackPanel _itemsSourceWeights;

        public EditClassWeightCategoriesViewController()
        {
            Title = "Weight Categories";

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

            PowerPlannerUIHelper.ConfigureForInputsStyle(this);

            // No save/cancel button, implicitly saves when going back or exiting

            StackView.AddTopSectionDivider();

            var headerView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                var labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Name",
                    Font = UIFont.PreferredBody.Bold()
                };
                headerView.Add(labelName);
                labelName.StretchHeight(headerView, top: 8, bottom: 8);

                var labelWeight = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Weight",
                    Font = UIFont.PreferredBody.Bold()
                };
                headerView.Add(labelWeight);
                labelWeight.StretchHeight(headerView, top: 8, bottom: 8);

                headerView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[name][weight(60)]-52-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "name", labelName,
                    "weight", labelWeight));
            }
            StackView.AddArrangedSubview(headerView);
            headerView.StretchWidth(StackView, left: 16, right: 16);

            StackView.AddDivider();

            var weightsView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                _itemsSourceWeights = new BareUIViewItemsSourceAdapterAsStackPanel(weightsView, (w) => { return new UIEditingWeightCategoryView(this) { DataContext = w }; });
            }
            StackView.AddArrangedSubview(weightsView);
            weightsView.StretchWidth(StackView, left: 16);

            var buttonDelete = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonDelete.SetTitle("Add Category", UIControlState.Normal);
            buttonDelete.TouchUpInside += new WeakEventHandler(delegate { ViewModel.AddWeightCategory(); }).Handler;
            StackView.AddArrangedSubview(buttonDelete);
            buttonDelete.StretchWidth(StackView);
            buttonDelete.SetHeight(44);

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

        public override void OnViewModelLoadedOverride()
        {
            _itemsSourceWeights.ItemsSource = ViewModel.WeightCategories;

            base.OnViewModelLoadedOverride();
        }

        private class UIEditingWeightCategoryView : BareUIView
        {
            private WeakReference<EditClassWeightCategoriesViewController> _controller;

            public UIEditingWeightCategoryView(EditClassWeightCategoriesViewController controller)
            {
                _controller = new WeakReference<EditClassWeightCategoriesViewController>(controller);

                var textFieldName = new UITextField()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Placeholder = "Name",
                    AdjustsFontSizeToFitWidth = true
                };
                BindingHost.SetTextFieldTextBinding(textFieldName, nameof(EditingWeightCategoryViewModel.Name));
                this.Add(textFieldName);
                textFieldName.StretchHeight(this, bottom: 1);

                var textFieldWeight = new UITextField()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Placeholder = "Weight",
                    AdjustsFontSizeToFitWidth = true,
                    KeyboardType = UIKeyboardType.DecimalPad
                };
                BindingHost.SetTextFieldTextBinding<double>(textFieldWeight, nameof(EditingWeightCategoryViewModel.Weight), WeightValueToTextBoxTextConverter.Convert, WeightValueToTextBoxTextConverter.ConvertBack);
                this.Add(textFieldWeight);
                textFieldWeight.StretchHeight(this, bottom: 1);

                // Delete button
                var buttonDelete = new UIButton(UIButtonType.Custom)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TintColor = UIColor.Red
                };
                buttonDelete.SetImage(UIImage.FromBundle("DeleteIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                buttonDelete.TouchUpInside += new WeakEventHandler(ButtonDelete_TouchUpInside).Handler;
                this.Add(buttonDelete);
                buttonDelete.StretchHeight(this, bottom: 1);

                this.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[name]-8-[weight(60)]-8-[delete(44)]-16-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "name", textFieldName,
                    "weight", textFieldWeight,
                    "delete", buttonDelete));

                var divider = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = ColorResources.InputDividers
                };
                this.Add(divider);
                divider.StretchWidth(this);
                divider.SetHeight(1f);
                divider.PinToBottom(this);

                this.SetHeight(45);
            }

            private void ButtonDelete_TouchUpInside(object sender, EventArgs e)
            {
                var weightCategory = ((sender as UIButton)?.Superview as BareUIView)?.DataContext as EditingWeightCategoryViewModel;
                if (weightCategory != null)
                {
                    if (_controller.TryGetTarget(out EditClassWeightCategoriesViewController controller))
                    {
                        controller.ViewModel.RemoveWeightCategory(weightCategory);
                    }
                }
            }
        }
    }
}