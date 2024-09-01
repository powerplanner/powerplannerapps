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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesDroid.Views;
using Java.Lang;
using PowerPlannerAndroid.ViewHosts;
using PowerPlannerAppDataLibrary;
using Google.Android.Material.Tabs;
using AndroidX.ViewPager.Widget;
using Vx.Droid;

namespace PowerPlannerAndroid.Views
{
    public class ClassView : PopupViewHost<ClassViewModel>
    {
        private static int LastSelectedPage;

        private TabLayout _tabLayout;
        private ViewPager _viewPager;

        public ClassView(ViewGroup root) : base(Resource.Layout.SingleClass, root)
        {
            _tabLayout = FindViewById<TabLayout>(Resource.Id.ClassPagerTabs);
            _viewPager = FindViewById<ViewPager>(Resource.Id.ClassPager);
        }

        private void _viewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            OnPageSelected();
        }

        private void OnPageSelected()
        {
            LastSelectedPage = _viewPager.CurrentItem;

            UpdateMenu();
            
            switch (_viewPager.CurrentItem)
            {
                case ClassPagerAdapter.DETAILS:
                    ViewModel.CurrentViewModel = ViewModel.DetailsViewModel;
                    break;

                case ClassPagerAdapter.TIMES:
                    ViewModel.CurrentViewModel = ViewModel.TimesViewModel;
                    break;

                case ClassPagerAdapter.TASKS:
                    ViewModel.CurrentViewModel = ViewModel.TasksViewModel;
                    ViewModel.ViewItemsGroupClass.LoadTasksAndEvents();
                    break;

                case ClassPagerAdapter.EVENTS:
                    ViewModel.CurrentViewModel = ViewModel.EventsViewModel;
                    ViewModel.ViewItemsGroupClass.LoadTasksAndEvents();
                    break;

                case ClassPagerAdapter.GRADES:
                    ViewModel.CurrentViewModel = ViewModel.GradesViewModel;
                    ViewModel.ViewItemsGroupClass.LoadGrades();
                    break;

                default:
                    ViewModel.CurrentViewModel = null;
                    break;
            }
        }

        public override void OnViewModelSetOverride()
        {
            SetBinding(nameof(ViewModel.ClassName), target: this, targetPropertyName: nameof(Title));

            base.OnViewModelSetOverride();
        }

        public override void OnViewModelLoadedOverride()
        {
            _viewPager.Adapter = new ClassPagerAdapter(this);

            int page = LastSelectedPage;
            if (ViewModel.InitialPage != null)
            {
                page = (int)ViewModel.InitialPage;
                page--; // Don't have Overview so decrement
                if (page < 0)
                {
                    page = 0;
                }
            }

            _viewPager.SetCurrentItem(page, false);
            _viewPager.PageSelected += _viewPager_PageSelected;

            _tabLayout.SetupWithViewPager(_viewPager);

            OnPageSelected();
        }

        private class ClassPagerAdapter : PagerAdapter
        {
            public const int DETAILS = 0, TIMES = 1, TASKS = 2, EVENTS = 3, GRADES = 4;

            public ClassView ClassView { get; private set; }

            private View _renderedDetails;
            private View _renderedTimes;
            private View _renderedGrades;
            private View _renderedTasks;
            private View _renderedEvents;

            public ClassPagerAdapter(ClassView classView)
            {
                ClassView = classView;
            }

            public override int Count
            {
                get
                {
                    return 5;
                }
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
            {
                return view == objectValue;
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                View view;

                switch (position)
                {
                    case DETAILS:
                        if (_renderedDetails == null)
                        {
                            _renderedDetails = ClassView.ViewModel.DetailsViewModel.Render();
                        }
                        view = _renderedDetails;
                        break;

                    case TIMES:
                        if (_renderedTimes == null)
                        {
                            _renderedTimes = ClassView.ViewModel.TimesViewModel.Render();
                        }
                        view = _renderedTimes;
                        break;

                    case TASKS:
                        if (_renderedTasks == null)
                        {
                            _renderedTasks = ClassView.ViewModel.TasksViewModel.Render();
                        }
                        view = _renderedTasks;
                        break;

                    case EVENTS:
                        if (_renderedEvents == null)
                        {
                            _renderedEvents = ClassView.ViewModel.EventsViewModel.Render();
                        }
                        view = _renderedEvents;
                        break;

                    case GRADES:
                        if (_renderedGrades == null)
                        {
                            _renderedGrades = ClassView.ViewModel.GradesViewModel.Render();
                        }
                        view = _renderedGrades;
                        break;

                    default:
                        throw new NotImplementedException("Unexpected page");
                }

                container.AddView(view);
                return view;
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue)
            {
                container.RemoveView((View)objectValue);
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(GetPageTitleString(position));
            }

            private static string GetPageTitleString(int position)
            {
                switch (position)
                {
                    case DETAILS:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemDetails.Header");

                    case TIMES:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemTimes.Header");

                    case TASKS:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemTasks.Header");

                    case EVENTS:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemEvents.Header");

                    case GRADES:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemGrades.Header");

                    default:
                        throw new NotImplementedException("Unexpected page");
                }
            }
        }

        private void UpdateMenu()
        {
            SetMenu(GetMenuResource());
        }

        private int GetMenuResource()
        {
            switch (_viewPager.CurrentItem)
            {
                case ClassPagerAdapter.DETAILS:
                    return Resource.Menu.class_details_menu;

                case ClassPagerAdapter.TIMES:
                    return Resource.Menu.class_times_menu;

                case ClassPagerAdapter.GRADES:
                    return Resource.Menu.class_grades_menu;
            }

            return Resource.Menu.class_basic_menu;
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemEdit:

                    switch (_viewPager.CurrentItem)
                    {
                        case ClassPagerAdapter.DETAILS:
                            ViewModel.EditDetails();
                            break;

                        case ClassPagerAdapter.TIMES:
                            ViewModel.EditTimes();
                            break;
                    }

                    break;

                case Resource.Id.MenuItemEditClass:
                    ViewModel.EditClass();
                    break;

                case Resource.Id.MenuItemDeleteClass:
                    PromptDeleteClass();
                    break;
            }
        }

        private void PromptDeleteClass()
        {
            var builder = new AlertDialog.Builder(Context);

            builder
                .SetTitle(PowerPlannerResources.GetString("String_ConfirmDeleteClassHeader"))
                .SetMessage(PowerPlannerResources.GetString("String_ConfirmDeleteClassMessage"))
                .SetPositiveButton(PowerPlannerResources.GetMenuItemDelete(), delegate { ViewModel.DeleteClass(); })
                .SetNegativeButton(PowerPlannerResources.GetStringCancel(), delegate { });

            builder.Create().Show();
        }
    }
}