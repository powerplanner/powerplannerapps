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
using BareMvvm.Core.ViewModels;

namespace InterfacesDroid.Views
{
    public abstract class PopupViewHost<TViewModel> : ViewHostGeneric where TViewModel : BaseViewModel
    {
        public PopupViewHost(int resourceId, ViewGroup root) : base(resourceId, root)
        {
        }
        
        public new TViewModel ViewModel
        {
            get { return base.ViewModel as TViewModel; }
            set { base.ViewModel = value; }
        }
    }
}