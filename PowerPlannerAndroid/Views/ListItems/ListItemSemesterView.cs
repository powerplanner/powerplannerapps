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
using System.Collections.Specialized;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemSemesterView : InflatedViewWithBinding
    {
        public event EventHandler<ViewItemSemester> OpenSemesterRequested;
        public event EventHandler<ViewItemSemester> EditSemesterRequested;
        private View _classesHeader;
        private View _classesFooter;
        private View _viewNoClasses;
        private ItemsControlWrapper _classesItemsWrapper;

        public ListItemSemesterView(ViewGroup root) : base(Resource.Layout.ListItemSemester, root)
        {
            FindViewById<Button>(Resource.Id.ButtonOpenSemester).Click += delegate { OpenSemesterRequested?.Invoke(this, (ViewItemSemester)DataContext); };
            FindViewById<View>(Resource.Id.SemesterName).Click += delegate { EditSemesterRequested?.Invoke(this, (ViewItemSemester)DataContext); };
            FindViewById<View>(Resource.Id.SemesterView).Click += delegate { OpenSemesterRequested?.Invoke(this, (ViewItemSemester)DataContext); };

            _viewNoClasses = FindViewById(Resource.Id.TextViewNoClasses);
            _classesHeader = FindViewById(Resource.Id.ClassesHeader);
            _classesFooter = FindViewById(Resource.Id.ClassesFooter);

            _classesItemsWrapper = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupClasses))
            {
                ItemTemplate = new CustomDataTemplate<ViewItemClass>(CreateClass)
            };
        }

        private NotifyCollectionChangedEventHandler _classesChangedHandler;
        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            base.OnDataContextChanged(oldValue, newValue);

            if (oldValue is ViewItemSemester && _classesChangedHandler != null)
            {
                (oldValue as ViewItemSemester).Classes.CollectionChanged -= _classesChangedHandler;
            }

            if (newValue is ViewItemSemester)
            {
                _classesChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Classes_CollectionChanged).Handler;
                (newValue as ViewItemSemester).Classes.CollectionChanged += _classesChangedHandler;
                _classesItemsWrapper.ItemsSource = (newValue as ViewItemSemester).Classes;
            }

            UpdateViewNoClassesVisibility();
        }

        private void Classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateViewNoClassesVisibility();
            }
            catch (ObjectDisposedException)
            {
                (sender as INotifyCollectionChanged).CollectionChanged -= _classesChangedHandler;
            }
        }

        private void UpdateViewNoClassesVisibility()
        {
            try
            {
                var semester = (DataContext as ViewItemSemester);
                if (semester != null && semester.Classes != null && semester.Classes.Count > 0)
                {
                    _viewNoClasses.Visibility = ViewStates.Gone;
                    _classesHeader.Visibility = ViewStates.Visible;
                    _classesFooter.Visibility = ViewStates.Visible;
                }

                else
                {
                    _viewNoClasses.Visibility = ViewStates.Visible;
                    _classesHeader.Visibility = ViewStates.Gone;
                    _classesFooter.Visibility = ViewStates.Gone;
                }
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private View CreateClass(ViewGroup root, ViewItemClass c)
        {
            return new InflatedViewWithBinding(Resource.Layout.ListItemSemesterClass, root)
            {
                DataContext = c
            };
        }
    }
}