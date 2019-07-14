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
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAndroid.Views
{
    public class SettingsImageUploadOptionsView : InterfacesDroid.Views.PopupViewHost<ImageUploadOptionsViewModel>
    {
        public SettingsImageUploadOptionsView(ViewGroup root) : base(Resource.Layout.SettingsImageUploadOptions, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            var spinnerUploadImagesVia = FindViewById<Spinner>(Resource.Id.SpinnerUploadImagesVia);
            spinnerUploadImagesVia.Adapter = ObservableAdapter.Create(
                list: ViewModel.UploadOptions,
                itemResourceId: Resource.Layout.SpinnerItemCurrentUploadImageViaPreview,
                dropDownItemResourceId: Resource.Layout.SpinnerItemUploadImageOption);

            for (int i = 0; i < ViewModel.UploadOptions.Length; i++)
            {
                if (ViewModel.UploadOptions[i] == ViewModel.SelectedUploadOption)
                {
                    spinnerUploadImagesVia.SetSelection(i);
                    break;
                }
            }

            spinnerUploadImagesVia.ItemSelected += SpinnerUploadImagesVia_ItemSelected;
        }

        private void SpinnerUploadImagesVia_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewModel.SelectedUploadOption = ViewModel.UploadOptions[e.Position];
        }
    }
}