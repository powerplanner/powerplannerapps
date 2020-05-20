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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Adapters;

namespace PowerPlannerAndroid.Views
{
    public class ViewTaskOrEventView : PopupViewHost<ViewTaskOrEventViewModel>
    {
        private TaskProgressBarControl _taskProgressBarControl;
        private Button _buttonConvertToGrade;

        public ViewTaskOrEventView(ViewGroup root) : base(Resource.Layout.ViewTaskOrEvent, root)
        {
            SetMenu(Resource.Menu.view_taskorevent_menu);

            _taskProgressBarControl = FindViewById<TaskProgressBarControl>(Resource.Id.TaskProgressBarControl);
            _taskProgressBarControl.OnProgressChangedByUser += _taskProgressBarControl_OnProgressChangedByUser;
            _buttonConvertToGrade = FindViewById<Button>(Resource.Id.ButtonConvertToGrade);
            _buttonConvertToGrade.Click += _buttonConvertToGrade_Click;
        }

        private void _buttonConvertToGrade_Click(object sender, EventArgs e)
        {
            ViewModel.ConvertToGrade();
        }

        private void _taskProgressBarControl_OnProgressChangedByUser(object sender, EventArgs e)
        {
            ViewModel.SetPercentComplete(_taskProgressBarControl.Progress);
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.PropertyChanged = new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            UpdatePageTitle();

            ViewModel.Item.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Item_PropertyChanged).Handler;
            UpdatePercentCompleteFromItem();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.PageTitle):
                        UpdatePageTitle();
                        break;
                }
            }
            catch { }
        }

        private void UpdatePageTitle()
        {
            Title = ViewModel.PageTitle;
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ViewItemTaskOrEvent item = ViewModel.Item;
            if (item == null)
            {
                return;
            }

            try
            {
                switch (e.PropertyName)
                {
                    case nameof(item.PercentComplete):
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
            ViewItemTaskOrEvent item = ViewModel.Item;
            if (item == null)
            {
                return;
            }

            _taskProgressBarControl.SetProgress(item.PercentComplete);
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
