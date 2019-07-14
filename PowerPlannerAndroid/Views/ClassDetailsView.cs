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

namespace PowerPlannerAndroid.Views
{
    public class ClassDetailsView : InterfacesDroid.Views.PopupViewHost<ClassDetailsViewModel>
    {
        public ClassDetailsView(ViewGroup root, ClassView classView) : base(Resource.Layout.ClassDetails, root)
        {
            ViewModel = classView.ViewModel.DetailsViewModel;
        }
    }
}