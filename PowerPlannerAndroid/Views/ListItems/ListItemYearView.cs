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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemYearView : InflatedViewWithBinding
    {
        public event EventHandler OnAddSemesterRequested;
        public event EventHandler OnEditRequested;
        public event EventHandler<ViewItemSemester> OnOpenSemesterRequested;
        public event EventHandler<ViewItemSemester> OnEditSemesterRequested;

        private ItemsControlWrapper _itemsWrapperSemester;

        public ViewItemYear Year { get; private set; }

        public ListItemYearView(ViewGroup root, ViewItemYear year) : base(Resource.Layout.ListItemYear, root)
        {
            DataContext = year;
            Year = year;

            FindViewById<Button>(Resource.Id.ButtonAddSemester).Click += delegate { OnAddSemesterRequested?.Invoke(this, new EventArgs()); };
            FindViewById<View>(Resource.Id.YearName).Click += delegate { OnEditRequested?.Invoke(this, new EventArgs()); };

            _itemsWrapperSemester = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupSemesters))
            {
                ItemsSource = year.Semesters,
                ItemTemplate = new CustomDataTemplate<ViewItemSemester>(CreateSemester)
            };
        }

        private View CreateSemester(ViewGroup root, ViewItemSemester semester)
        {
            var view = new ListItemSemesterView(root)
            {
                DataContext = semester
            };

            view.OpenSemesterRequested += ListItemSemesterView_OpenSemesterRequested;
            view.EditSemesterRequested += ListItemSemesterView_EditSemesterRequested;

            return view;
        }

        private void ListItemSemesterView_EditSemesterRequested(object sender, ViewItemSemester e)
        {
            OnEditSemesterRequested?.Invoke(this, e);
        }

        private void ListItemSemesterView_OpenSemesterRequested(object sender, ViewItemSemester e)
        {
            OnOpenSemesterRequested?.Invoke(this, e);
        }
    }
}