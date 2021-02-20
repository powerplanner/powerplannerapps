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
            View = new ScrollView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            }
            .VxChildren(
                new LinearLayout(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Orientation = Orientation.Vertical
                }
                .VxChildren(

                    new TextView(Context)
                    {
                        Text = PowerPlannerResources.GetString("Settings_ImageUploadOptionsPage_Description.Text"),
                        LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                        {
                            TopMargin = ThemeHelper.AsPx(Context, 16),
                            BottomMargin = ThemeHelper.AsPx(Context, 16),
                            LeftMargin = ThemeHelper.AsPx(Context, 16),
                            RightMargin = ThemeHelper.AsPx(Context, 16)
                        }
                        // TODO: Style of Medium
                    },

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

                    )
                );
        }
    }
}