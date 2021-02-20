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
using InterfacesDroid.Themes;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAndroid.Vx;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAndroid.Views
{
    public class SettingsImageUploadOptionsView : VxPopupViewHost<ImageUploadOptionsViewModel>
    {
        public SettingsImageUploadOptionsView(Context context) : base(context)
        {
            Title = PowerPlannerResources.GetString("Settings_SyncOptions_ItemImageUploadOptions_DisplayName");
        }

        public override void OnViewModelLoadedOverride()
        {
            View = VxVerticalScrollView(

                new TextView(Context, null, 0, Resource.Style.TextAppearance_AppCompat_Medium)
                {
                    Text = PowerPlannerResources.GetString("Settings_ImageUploadOptionsPage_Description.Text")
                }
                .VxPadding(16),

                new Divider(Context),

                new FullWidthSpinner(Context)
                {
                    Adapter = ObservableAdapter.Create(
                        list: ViewModel.UploadOptions,
                        itemResourceId: Resource.Layout.SpinnerItemCurrentUploadImageViaPreview,
                        dropDownItemResourceId: Resource.Layout.SpinnerItemUploadImageOption)
                }
                .VxSelection(Array.IndexOf(ViewModel.UploadOptions, ViewModel.SelectedUploadOption))
                .VxItemSelected((s, e) => { ViewModel.SelectedUploadOption = ViewModel.UploadOptions[e.Position]; }),

                new Divider(Context)

            );
        }
    }
}