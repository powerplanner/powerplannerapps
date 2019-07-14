using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Views
{
    public class UIScheduleEditingView : BareUIView
    {
        private BareUIStackViewItemsSourceAdapter<UIEditClassScheduleView> _itemsSourceClasses;
        private ScheduleViewModel ViewModel { get { return DataContext as ScheduleViewModel; } }

        public UIScheduleEditingView(ScheduleViewModel viewModel)
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            DataContext = viewModel;

            Initialize();
        }

        private void Initialize()
        {
            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Spacing = 16
            };
            base.Add(stackView);
            stackView.StretchWidthAndHeight(this, right: 16);
            {
                UIStackView classesStackView = new UIStackView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Axis = UILayoutConstraintAxis.Vertical,
                    Spacing = 16
                };
                stackView.AddArrangedSubview(classesStackView);
                classesStackView.StretchWidth(stackView, left: 16);

                _itemsSourceClasses = new BareUIStackViewItemsSourceAdapter<UIEditClassScheduleView>(classesStackView);
                _itemsSourceClasses.OnViewCreated += _itemsSourceClasses_OnViewCreated;
                _itemsSourceClasses.ItemsSource = ViewModel.SemesterViewItemsGroup.Classes;

                UIButton buttonAdd = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                buttonAdd.SetTitle("Add Class", UIControlState.Normal);
                buttonAdd.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
                buttonAdd.BackgroundColor = ColorResources.PowerPlannerBlueChromeColor;
                buttonAdd.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.AddClass(); }).Handler;
                stackView.AddArrangedSubview(buttonAdd);
                buttonAdd.StretchWidth(stackView, left: 16);
            }
        }

        private void _itemsSourceClasses_OnViewCreated(object sender, UIEditClassScheduleView e)
        {
            e.OnRequestAddTime += new WeakEventHandler<ViewItemClass>(E_OnRequestAddTime).Handler;
            e.OnRequestEditSchedules += new WeakEventHandler<ViewItemSchedule[]>(E_OnRequestEditSchedules).Handler;
            e.OnRequestEditClass += new WeakEventHandler<ViewItemClass>(E_OnRequestEditClass).Handler;
        }

        private void E_OnRequestEditClass(object sender, ViewItemClass e)
        {
            ViewModel.EditClass(e);
        }

        private void E_OnRequestEditSchedules(object sender, ViewItemSchedule[] e)
        {
            ViewModel.EditTimes(e);
        }

        private void E_OnRequestAddTime(object sender, ViewItemClass e)
        {
            ViewModel.AddTime(e);
        }
    }
}