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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.Views.ListItems;
using System.Collections.Specialized;
using ToolsPortable;
using InterfacesDroid.Adapters;
using InterfacesDroid.Themes;
using InterfacesDroid.DataTemplates;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class YearsView : PopupViewHost<YearsViewModel>
    {
        private ItemsControlWrapper _itemsWrapperYears;

        public YearsView(ViewGroup root) : base(Resource.Layout.Years, root)
        {
            Title = PowerPlannerResources.GetString("MainMenuItem_Years");
        }

        public override void OnViewModelLoadedOverride()
        {
            // Must keep a strong reference so doesn't get disposed
            _itemsWrapperYears = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.YearsListView))
            {
                ItemsSource = ViewModel.YearsViewItemsGroup.School.Years,
                ItemTemplate = new CustomDataTemplate<ViewItemYear>(CreateYearView)
            };

            FindViewById<Button>(Resource.Id.ButtonAddYear).Click += ButtonAddYear_Click;

            FindViewById(Resource.Id.YearsContent).Visibility = ViewStates.Visible;
        }

        private View CreateYearView(ViewGroup root, ViewItemYear year)
        {
            var view = new ListItemYearView(root, year);

            view.OnAddSemesterRequested += delegate
            {
                ViewModel.AddSemester(year.Identifier);
            };

            view.OnOpenSemesterRequested += (s, viewItemSemester) =>
            {
                ViewModel.OpenSemester(viewItemSemester.Identifier);
            };

            view.OnEditRequested += YearView_OnEditRequested;
            view.OnEditSemesterRequested += YearView_OnEditSemesterRequested;
            
            //RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(
            //    RelativeLayout.LayoutParams.WrapContent,
            //    RelativeLayout.LayoutParams.WrapContent
            //    );
            //var marginLeftRight = ThemeHelper.AsPx(context, 10);
            //var marginTopBottom = marginLeftRight / 2;
            //p.SetMargins(marginLeftRight, marginTopBottom, marginLeftRight, marginTopBottom);

            //view.LayoutParameters = p;

            return view;
        }

        private void YearView_OnEditSemesterRequested(object sender, ViewItemSemester e)
        {
            ViewModel.EditSemester(e);
        }

        private void YearView_OnEditRequested(object sender, EventArgs e)
        {
            var view = sender as ListItemYearView;

            ViewModel.EditYear(view.DataContext as ViewItemYear);
        }

        private void ButtonAddYear_Click(object sender, EventArgs e)
        {
            ViewModel.AddYear();
        }
    }
}