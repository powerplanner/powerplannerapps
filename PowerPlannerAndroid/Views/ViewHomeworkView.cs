using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Adapters;

namespace PowerPlannerAndroid.Views
{
    public class ViewHomeworkView : PopupViewHost<ViewHomeworkViewModel>
    {
        private HomeworkProgressBarControl _homeworkProgressBarControl;
        private Button _buttonConvertToGrade;

        public ViewHomeworkView(ViewGroup root) : base(Resource.Layout.ViewHomework, root)
        {
            SetMenu(Resource.Menu.view_homework_menu);

            _homeworkProgressBarControl = FindViewById<HomeworkProgressBarControl>(Resource.Id.HomeworkProgressBarControl);
            _homeworkProgressBarControl.OnProgressChangedByUser += _homeworkProgressBarControl_OnProgressChangedByUser;
            _buttonConvertToGrade = FindViewById<Button>(Resource.Id.ButtonConvertToGrade);
            _buttonConvertToGrade.Click += _buttonConvertToGrade_Click;
        }

        private void _buttonConvertToGrade_Click(object sender, EventArgs e)
        {
            ViewModel.ConvertToGrade();
        }

        private void _homeworkProgressBarControl_OnProgressChangedByUser(object sender, EventArgs e)
        {
            if (ViewModel.Type == ViewHomeworkViewModel.ItemType.Homework)
            {
                ViewModel.SetPercentComplete(_homeworkProgressBarControl.Progress);
            }
        }

        private BaseViewItemHomeworkExam _prevItem;
        private PropertyChangedEventHandler _itemPropertyChangedHandler;
        public override void OnViewModelLoadedOverride()
        {
            if (_prevItem != null && _itemPropertyChangedHandler != null)
            {
                _prevItem.PropertyChanged -= _itemPropertyChangedHandler;
                _prevItem = null;
            }

            switch (ViewModel.Type)
            {
                case ViewHomeworkViewModel.ItemType.Exam:
                    Title = PowerPlannerResources.GetString("ViewHomeworkPage_String_TitleExam");
                    _homeworkProgressBarControl.Visibility = ViewStates.Gone;
                    break;

                case ViewHomeworkViewModel.ItemType.Homework:
                    Title = PowerPlannerResources.GetString("ViewHomeworkPage_String_TitleHomework");

                    if (ViewModel.IsUnassigedMode)
                    {
                        _homeworkProgressBarControl.Visibility = ViewStates.Gone;
                        _buttonConvertToGrade.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        _homeworkProgressBarControl.Visibility = ViewStates.Visible;
                        _itemPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(Item_PropertyChanged).Handler;
                        ViewModel.Item.PropertyChanged += _itemPropertyChangedHandler;
                        _prevItem = ViewModel.Item;
                        UpdatePercentCompleteFromItem();
                    }
                    break;
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ViewItemHomework homework = ViewModel.Item as ViewItemHomework;
            if (homework == null)
            {
                return;
            }

            try
            {
                switch (e.PropertyName)
                {
                    case nameof(homework.PercentComplete):
                        UpdatePercentCompleteFromItem();
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                // If disposed, de-register the changed handler
                if (_itemPropertyChangedHandler != null && ViewModel?.Item != null)
                {
                    ViewModel.Item.PropertyChanged -= _itemPropertyChangedHandler;
                }
            }
        }

        private void UpdatePercentCompleteFromItem()
        {
            ViewItemHomework homework = ViewModel.Item as ViewItemHomework;
            if (homework == null)
            {
                return;
            }

            _homeworkProgressBarControl.SetProgress(homework.PercentComplete);
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemEdit:
                    ViewModel.Edit();
                    break;

                case Resource.Id.MenuItemDelete:
                    ViewModel.Delete();
                    break;
            }
        }

        
    }
}