using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerUWP.Flyouts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Vx.Uwp;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    public sealed partial class TaskOrEventListViewItem : UserControl
    {
        public event EventHandler<ViewItemTaskOrEvent> OnClickItem;
        private TaskOrEventListItemComponent _component;

        public TaskOrEventListViewItem()
        {
            this.InitializeComponent();

            this.DataContextChanged += TaskOrEventListViewItem_DataContextChanged;

            _component = new TaskOrEventListItemComponent()
            {
                IncludeMargin = false
            };
        }

        private ViewItemTaskOrEvent _currItem;

        private void TaskOrEventListViewItem_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _currItem = args.NewValue as ViewItemTaskOrEvent;

            _component.ViewModel = PowerPlannerApp.Current.GetMainScreenViewModel().Content as BaseMainScreenViewModelDescendant;
            _component.Item = _currItem;
            _component.MarkDirtyPublic();

            if (this.Content == null)
            {
                this.Content = _component.Render();
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (OnClickItem != null)
        //    {
        //        OnClickItem(sender, GetCurrentItem());
        //    }
        //    else
        //    {
        //        PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(GetCurrentItem());
        //    }
        //}

        /// <summary>
        /// Gets or sets whether the class name should be displayed in the subtitle. When viewing from within a class, this should be set to false.
        /// </summary>
        public bool IncludeClass
        {
            get => _component.IncludeClass;
            set => _component.IncludeClass = value;
        }

        public bool IncludeDate
        {
            get => _component.IncludeDate;
            set => _component.IncludeDate = value;
        }
    }
}
