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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesDroid.DataTemplates;

namespace PowerPlannerAndroid.Views
{
    public class ClassTimesView : InterfacesDroid.Views.PopupViewHost<ClassTimesViewModel>
    {
        private ItemsControlWrapper _itemsWrapper;

        public ClassTimesView(ViewGroup root, ClassTimesViewModel viewModel) : base(Resource.Layout.ClassTimes, root)
        {
            ViewModel = viewModel;
        }

        public override void OnViewModelLoadedOverride()
        {
            _itemsWrapper = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ClassTimesViewGroup))
            {
                ItemsSource = ViewModel.TimesGroupedByDay,
                ItemTemplate = new CustomDataTemplate<ClassTimesViewModel.GroupedDay>(CreateTimeGroup)
            };
        }

        private View CreateTimeGroup(ViewGroup root, ClassTimesViewModel.GroupedDay groupedDay)
        {
            return new ListItems.ListItemClassGroupedTimesView(root)
            {
                DataContext = groupedDay
            };
        }
    }
}