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
using PowerPlannerAndroid.ViewHosts;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;

namespace PowerPlannerAndroid.Views
{
    public class ClassesView : MainScreenViewHostDescendant<ClassesViewModel>
    {
        public ClassesView(ViewGroup root) : base(Resource.Layout.Classes, root)
        {
        }
    }
}