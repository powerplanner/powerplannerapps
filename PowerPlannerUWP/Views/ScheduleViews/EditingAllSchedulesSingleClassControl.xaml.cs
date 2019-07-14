using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using ToolsUniversal;
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

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public sealed partial class EditingAllSchedulesSingleClassControl : UserControl
    {
        public event EventHandler<ViewItemSchedule[]> HighlightRequested, UnhighlightRequested;

        public event EventHandler<ViewItemClass> OnRequestAddTime;
        public event EventHandler<ViewItemSchedule[]> OnRequestEditGroup;
        public event EventHandler<ViewItemClass> OnRequestEditClass;

        private ViewItemSchedule[] _currentlyHighlighted = new ViewItemSchedule[0];

        public EditingAllSchedulesSingleClassControl()
        {
            this.InitializeComponent();

            buttonAddTime.Content = LocalizedResources.GetString("String_AddTime").ToLower();
        }

        private void buttonAddTime_Click(object sender, RoutedEventArgs e)
        {
            OnRequestAddTime?.Invoke(this, GetClass());
        }

        public ViewItemClass GetClass()
        {
            return DataContext as ViewItemClass;
        }

        private void ListViewTimes_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewItemSchedule[] scheduleGroup = (ViewItemSchedule[])e.ClickedItem;

            OnRequestEditGroup?.Invoke(this, scheduleGroup);
        }

        private object _currPointerFocusedElement;
        
        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _currPointerFocusedElement = null;
            UnhighlightRequested?.Invoke(this, _currentlyHighlighted);
            _currentlyHighlighted = new ViewItemSchedule[0];
        }

        private void EditingScheduleClassTimeListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _currPointerFocusedElement = sender;

            EditingScheduleClassTimeListViewItem i = (sender as Border).Child as EditingScheduleClassTimeListViewItem;

            ViewItemSchedule[] desiredHighlighted = (i.DataContext as ViewItemSchedule[]);

            SetHighlightedSchedules(desiredHighlighted);
        }

        private void EditingScheduleClassTimeListViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _currPointerFocusedElement = null;
            UnhighlightRequested?.Invoke(this, _currentlyHighlighted);
            _currentlyHighlighted = new ViewItemSchedule[0];
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_currPointerFocusedElement == sender)
                return;

            _currPointerFocusedElement = sender;
            SetHighlightedSchedules(GetClass().Schedules.ToArray());
        }

        private void EditingScheduleClassTimeListViewItem_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private ViewItemClass _currClass;
        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_currClass != null)
                DeregisterOldClass(_currClass);

            _currClass = args.NewValue as ViewItemClass;

            if (_currClass != null)
                RegisterNewClass(_currClass);
        }

        private void DeregisterOldClass(ViewItemClass c)
        {
            c.Schedules.CollectionChanged -= _schedulesChangedHandler;
        }

        private NotifyCollectionChangedEventHandler _schedulesChangedHandler;
        private void RegisterNewClass(ViewItemClass c)
        {
            if (_schedulesChangedHandler == null)
            {
                _schedulesChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Schedules_CollectionChanged).Handler;
            }
            c.Schedules.CollectionChanged += _schedulesChangedHandler;
            UpdateGroupedSchedules();
        }

        private void Schedules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateGroupedSchedules();
        }

        public List<ViewItemSchedule[]> GroupedSchedules
        {
            get { return (List<ViewItemSchedule[]>)GetValue(GroupedSchedulesProperty); }
            set { SetValue(GroupedSchedulesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GroupedSchedules.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupedSchedulesProperty =
            DependencyProperty.Register("GroupedSchedules", typeof(List<ViewItemSchedule[]>), typeof(EditingAllSchedulesSingleClassControl), new PropertyMetadata(null));
        

        public void UpdateGroupedSchedules()
        {
            if (_currClass == null)
            {
                GroupedSchedules = null;
                return;
            }

            GroupedSchedules = _currClass.GetSchedulesGroupedBySharedEditingValues();
        }

        private void GridClassInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OnRequestEditClass?.Invoke(this, GetClass());
        }

        private void SetHighlightedSchedules(ViewItemSchedule[] desiredHighlighted)
        {
            ViewItemSchedule[] toUnhiglight = _currentlyHighlighted.Except(desiredHighlighted).ToArray();
            if (toUnhiglight.Length > 0)
                UnhighlightRequested?.Invoke(this, toUnhiglight);

            _currentlyHighlighted = desiredHighlighted;
            HighlightRequested?.Invoke(this, _currentlyHighlighted.ToArray());
        }
    }
}
