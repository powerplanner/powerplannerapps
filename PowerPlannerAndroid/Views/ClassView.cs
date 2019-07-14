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
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Java.Lang;
using PowerPlannerAndroid.ViewHosts;
using Android.Support.V7.Widget;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class ClassView : MainScreenViewHostDescendant<ClassViewModel>
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

            RequestUpdateMenu();
            
            switch (_viewPager.CurrentItem)
            {
                case ClassPagerAdapter.HOMEWORK:
                    ViewModel.CurrentViewModel = ViewModel.HomeworkViewModel;
                    ViewModel.ViewItemsGroupClass.LoadHomeworkAndExams();
                    break;

                case ClassPagerAdapter.EXAMS:
                    ViewModel.CurrentViewModel = ViewModel.ExamsViewModel;
                    ViewModel.ViewItemsGroupClass.LoadHomeworkAndExams();
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

        public override void OnViewModelLoadedOverride()
        {
            _viewPager.Adapter = new ClassPagerAdapter(this);

            _viewPager.SetCurrentItem(LastSelectedPage, false);
            _viewPager.PageSelected += _viewPager_PageSelected;

            _tabLayout.SetupWithViewPager(_viewPager);

            OnPageSelected();
        }

        private class ClassPagerAdapter : PagerAdapter
        {
            public const int DETAILS = 0, TIMES = 1, HOMEWORK = 2, EXAMS = 3, GRADES = 4;

            public ClassView ClassView { get; private set; }

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
                        view = new ClassDetailsView(container, ClassView);
                        break;

                    case TIMES:
                        view = new ClassTimesView(container, ClassView.ViewModel.TimesViewModel);
                        break;

                    case HOMEWORK:
                        view = new ClassHomeworkOrExamsView(container, ClassView.ViewModel.HomeworkViewModel);
                        break;

                    case EXAMS:
                        view = new ClassHomeworkOrExamsView(container, ClassView.ViewModel.ExamsViewModel);
                        break;

                    case GRADES:
                        view = new ClassGradesView(container, ClassView.ViewModel.GradesViewModel);
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

                    case HOMEWORK:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemHomework.Header");

                    case EXAMS:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemExams.Header");

                    case GRADES:
                        return PowerPlannerResources.GetString("ClassPage_PivotItemGrades.Header");

                    default:
                        throw new NotImplementedException("Unexpected page");
                }
            }
        }

        protected override int GetMenuResource()
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

        public override void OnMenuItemClick(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
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