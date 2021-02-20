using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public abstract class VxViewHost<TViewModel> : VxViewHostGeneric where TViewModel : BaseViewModel
    {
        public VxViewHost(Context context) : base(context) { }

        public new TViewModel ViewModel
        {
            get { return base.ViewModel as TViewModel; }
            set { base.ViewModel = value; }
        }
    }
}