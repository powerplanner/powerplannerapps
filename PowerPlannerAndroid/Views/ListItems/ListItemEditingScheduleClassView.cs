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
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.DataTemplates;
using ToolsPortable;
using System.Collections.Specialized;
using System.Drawing;
using Xamarin.Essentials;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemEditingScheduleClassView : InflatedViewWithBindingHost
    {
        public event EventHandler<ViewItemClass> OnEditClassRequested;
        public event EventHandler<ViewItemClass> OnAddClassTimeRequested;
        public event EventHandler<ViewItemSchedule[]> OnEditClassTimesRequested;

        private ItemsControlWrapper _timesItemsControlWrapper;

        public ListItemEditingScheduleClassView(ViewGroup root, ViewItemClass c) : base(Resource.Layout.ListItemEditingScheduleClass, root)
        {
            DataContext = c;

            BindingHost.SetBinding<byte[]>(nameof(c.Color), (color) =>
            {
                FindViewById(Resource.Id.ListItemEditingScheduleClass_ColorContainer).SetBackgroundColor(ColorBytesHelper.ToColor(color).ToPlatformColor());
            });

            BindingHost.SetBinding<string>(nameof(c.Name), (name) =>
            {
               FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClass_ClassName).Text = name;
            });

            FindViewById<Button>(Resource.Id.ButtonAddTime).Click += delegate { OnAddClassTimeRequested?.Invoke(this, (ViewItemClass)DataContext); };
            FindViewById(Resource.Id.ListItemEditingScheduleClass_ClassName).Click += delegate { OnEditClassRequested?.Invoke(this, (ViewItemClass)DataContext); };

            _timesItemsControlWrapper = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupClassTimes))
            {
                ItemTemplate = new CustomDataTemplate<ViewItemSchedule[]>(CreateListViewTimeItem)
            };

            c.Schedules.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Schedules_CollectionChanged).Handler;
            UpdateTimes();
        }

        private void Schedules_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateTimes();
        }

        private void UpdateTimes()
        {
            _timesItemsControlWrapper.ItemsSource = (DataContext as ViewItemClass).GetSchedulesGroupedBySharedEditingValues();
        }

        private View CreateListViewTimeItem(ViewGroup root, ViewItemSchedule[] schedules)
        {
            var view = new ListItemEditingScheduleClassTimeView(root, schedules);

            view.Clickable = true;
            view.Click += delegate
            {
                OnEditClassTimesRequested?.Invoke(this, schedules);
            };

            return view;
        }
    }
}