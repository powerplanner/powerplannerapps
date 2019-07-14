using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;

namespace PowerPlannerAndroid.Views.Controls
{
    public class AddHomeworkRepeatsSubControl : InflatedViewWithBinding
    {
        public AddHomeworkRepeatsSubControl(ViewGroup root) : base(Resource.Layout.AddHomeworkRepeatsSubControl, root)
        {
            Initialize();
        }

        public AddHomeworkRepeatsSubControl(Context context, IAttributeSet attrs) : base(Resource.Layout.AddHomeworkRepeatsSubControl, context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            FindViewById<Button>(Resource.Id.ButtonUpgradeToPremium).Click += ButtonUpgradeToPremium_Click;
        }

        private void ButtonUpgradeToPremium_Click(object sender, EventArgs e)
        {
            ViewModel.UpgradeToPremiumForRepeating();
        }

        public AddHomeworkViewModel ViewModel => DataContext as AddHomeworkViewModel;

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            FindViewById<RecurrenceControl>(Resource.Id.RecurrenceControl).DataContext = ViewModel.RecurrenceControlViewModel;

            base.OnDataContextChanged(oldValue, newValue);
        }
    }
}