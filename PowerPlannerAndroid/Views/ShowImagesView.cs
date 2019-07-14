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
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;

namespace PowerPlannerAndroid.Views
{
    public class ShowImagesView : InterfacesDroid.Views.PopupViewHost<ShowImagesViewModel>
    {
        public ShowImagesView(ViewGroup root) : base(Resource.Layout.ShowImages, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<ImagesPagerControl>(Resource.Id.MyImagesPagerControl).Initialize(ViewModel.CurrentImage, ViewModel.AllImages);

            base.OnViewModelLoadedOverride();
        }
    }
}