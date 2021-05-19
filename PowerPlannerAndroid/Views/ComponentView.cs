using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Droid;

namespace PowerPlannerAndroid.Views
{
    public class ComponentView : ViewHostGeneric
    {
        public ComponentView(ViewGroup root) : base(root)
        {
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            var nativeView = ViewModel.Render();
            nativeView.LayoutParameters = new Android.Views.ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            AddView(nativeView);
        }
    }
}