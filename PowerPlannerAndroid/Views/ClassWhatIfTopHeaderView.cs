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
using InterfacesDroid.DataTemplates;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAndroid.Views.ListItems;
using Android.Util;

namespace PowerPlannerAndroid.Views
{
    public class ClassWhatIfTopHeaderView : InflatedViewWithBinding
    {
        private ClassGradesTopHeaderView _gradesTopHeaderView;

        public ClassWhatIfTopHeaderView(ViewGroup root) : base(Resource.Layout.ClassWhatIfTopHeader, root)
        {
            var linearLayoutRoot = FindViewById<LinearLayout>(Resource.Id.Root);

            _gradesTopHeaderView = new Views.ClassGradesTopHeaderView(linearLayoutRoot, hideWhatIfModeButton: true);
            linearLayoutRoot.AddView(_gradesTopHeaderView);
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            _gradesTopHeaderView.DataContext = newValue;
        }
    }
}