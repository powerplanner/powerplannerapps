using InterfacesiOS.Controllers;
using InterfacesiOS.Converters;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlanneriOS.Helpers;
using PowerPlanneriOS.Views;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using UIKit;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassGradeScaleViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassGradeScaleViewModel>
    {
        private BareUIViewItemsSourceAdapterAsStackPanel _itemsSourceGradeScales;

        public ConfigureClassGradeScaleViewController()
        {
            Title = "Grade Scale";

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

            

            //var savedScalesContainer = new UIView()
            //{
            //    TranslatesAutoresizingMaskIntoConstraints = false
            //};
            //{
            //    var pickerSavedScales = new BareUIInlinePickerView(this, left: 16, right: 16)
            //    {
            //        TranslatesAutoresizingMaskIntoConstraints = false,
            //        HeaderText = "Scale"
            //    };
            //    BindingHost.SetItemsSourceBinding(pickerSavedScales, nameof(ViewModel.SavedGradeScales));
            //    BindingHost.SetSelectedItemBinding(pickerSavedScales, nameof(ViewModel.SelectedSavedGradeScale));
            //    savedScalesContainer.AddSubview(pickerSavedScales);
            //    pickerSavedScales.StretchHeight(savedScalesContainer);

            //    var buttonSaveScale = new UIButton(UIButtonType.Custom)
            //    {
            //        TranslatesAutoresizingMaskIntoConstraints = false,
            //        //TintColor = UIColor.
            //    };
            //    buttonSaveScale.SetImage(UIImage.FromBundle("SaveAsIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            //    buttonSaveScale.TouchUpInside += new WeakEventHandler(ButtonSaveScale_TouchUpInside).Handler;
            //    savedScalesContainer.Add(buttonSaveScale);
            //    buttonSaveScale.StretchHeight(savedScalesContainer, bottom: 1);

            //    savedScalesContainer.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[picker][save(44)]-16-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
            //        "picker", pickerSavedScales,
            //        "save", buttonSaveScale));

            //    savedScalesContainer.SetHeight(44);
            //}
            //StackView.AddArrangedSubview(savedScalesContainer);
            //savedScalesContainer.StretchWidth(StackView);

            //StackView.AddSectionDivider();

            //var headerView = new UIView()
            //{
            //    TranslatesAutoresizingMaskIntoConstraints = false
            //};
            //{
            //    var labelStartingGrade = new UILabel()
            //    {
            //        TranslatesAutoresizingMaskIntoConstraints = false,
            //        Text = "Starting Grade",
            //        Font = UIFont.PreferredBody.Bold()
            //    };
            //    headerView.Add(labelStartingGrade);
            //    labelStartingGrade.StretchHeight(headerView, top: 8, bottom: 8);

            //    var labelGpa = new UILabel()
            //    {
            //        TranslatesAutoresizingMaskIntoConstraints = false,
            //        Text = "GPA",
            //        Font = UIFont.PreferredBody.Bold()
            //    };
            //    headerView.Add(labelGpa);
            //    labelGpa.StretchHeight(headerView, top: 8, bottom: 8);

            //    headerView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[startingGrade][gpa(==startingGrade)]-52-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
            //        "startingGrade", labelStartingGrade,
            //        "gpa", labelGpa));
            //}
            //StackView.AddArrangedSubview(headerView);
            //headerView.StretchWidth(StackView, left: 16, right: 16);

            //StackView.AddDivider();

            //var scalesView = new UIView()
            //{
            //    TranslatesAutoresizingMaskIntoConstraints = false
            //};
            //{
            //    _itemsSourceGradeScales = new BareUIViewItemsSourceAdapterAsStackPanel(scalesView, (w) => { return new UIEditingGradeScaleView(this) { DataContext = w }; });
            //}
            //StackView.AddArrangedSubview(scalesView);
            //scalesView.StretchWidth(StackView, left: 16);

            //var buttonAdd = new UIButton(UIButtonType.System)
            //{
            //    TranslatesAutoresizingMaskIntoConstraints = false
            //};
            //buttonAdd.SetTitle("Add Grade Scale", UIControlState.Normal);
            //buttonAdd.TouchUpInside += new WeakEventHandler(delegate { ViewModel.AddGradeScale(); }).Handler;
            //StackView.AddArrangedSubview(buttonAdd);
            //buttonAdd.StretchWidth(StackView);
            //buttonAdd.SetHeight(44);
        }

        private void ButtonSaveScale_TouchUpInside(object sender, EventArgs e)
        {
            ViewModel.SaveGradeScale();
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
            StackView.AddTopSectionDivider();

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            StackView.AddArrangedSubview(renderedComponent);
            renderedComponent.StretchWidth(StackView);

            StackView.AddBottomSectionDivider();

            base.OnViewModelLoadedOverride();
        }

        private class UIEditingGradeScaleView : BareUIView
        {
            private WeakReference<ConfigureClassGradeScaleViewController> _controller;

            public UIEditingGradeScaleView(ConfigureClassGradeScaleViewController controller)
            {
                _controller = new WeakReference<ConfigureClassGradeScaleViewController>(controller);

                var textFieldStartingGrade = new UITextField()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Placeholder = "Starting Grade",
                    AdjustsFontSizeToFitWidth = true,
                    KeyboardType = UIKeyboardType.DecimalPad
                };
                BindingHost.SetTextFieldTextBinding<double>(textFieldStartingGrade, nameof(PowerPlannerSending.GradeScale.StartGrade), TextToDoubleConverter.Convert, TextToDoubleConverter.ConvertBack);
                this.Add(textFieldStartingGrade);
                textFieldStartingGrade.StretchHeight(this, bottom: 1);

                var textFieldGpa = new UITextField()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Placeholder = "GPA",
                    AdjustsFontSizeToFitWidth = true,
                    KeyboardType = UIKeyboardType.DecimalPad
                };
                BindingHost.SetTextFieldTextBinding<double>(textFieldGpa, nameof(PowerPlannerSending.GradeScale.GPA), TextToDoubleConverter.Convert, TextToDoubleConverter.ConvertBack);
                this.Add(textFieldGpa);
                textFieldGpa.StretchHeight(this, bottom: 1);

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

                this.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[startingGrade]-8-[gpa(==startingGrade)]-8-[delete(44)]-16-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "startingGrade", textFieldStartingGrade,
                    "gpa", textFieldGpa,
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
                var gradeScale = ((sender as UIButton)?.Superview as BareUIView)?.DataContext as PowerPlannerSending.GradeScale;
                if (gradeScale != null)
                {
                    if (_controller.TryGetTarget(out ConfigureClassGradeScaleViewController controller))
                    {
                        //controller.ViewModel.RemoveGradeScale(gradeScale);
                    }
                }
            }
        }
    }
}
