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
using InterfacesDroid.Themes;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.Views.Controls
{
    public class DifferentSemesterOverlayControl : InflatedView
    {
        public DifferentSemesterOverlayControl(Context context) : base(context, Resource.Layout.DifferentSemesterOverlay)
        {
            Initialize();
        }

        public DifferentSemesterOverlayControl(Context context, IAttributeSet attrs) : base(context, Resource.Layout.DifferentSemesterOverlay, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Have to localize programmatically since this view is sometimes programmatically added
            FindViewById<TextView>(Resource.Id.TextViewDifferentSemesterTitle).Text = PowerPlannerResources.GetString("DifferentSemesterOverlayControl_TextBlockHeader.Text");
            FindViewById<TextView>(Resource.Id.TextViewDifferentSemesterContent).Text = PowerPlannerResources.GetString("DifferentSemesterOverlayControl_TextBlockDescription.Text");
            FindViewById(Resource.Id.DifferentSemesterViewSemestersButton).Click += DifferentSemesterViewSemestersButton_Click;
            FindViewById(Resource.Id.DifferentSemesterOverlayBackground).Click += DifferentSemesterOverlayControl_Click;
        }

        private void DifferentSemesterOverlayControl_Click(object sender, EventArgs e)
        {
            Visibility = ViewStates.Gone;
        }

        private void DifferentSemesterViewSemestersButton_Click(object sender, EventArgs e)
        {
            try
            {
                var model = PowerPlannerApp.Current.GetMainScreenViewModel();

                model.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Years;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void PinButtonToTop(int topPadding)
        {
            var button = FindViewById(Resource.Id.DifferentSemesterViewSemestersButton);

            (button.LayoutParameters as FrameLayout.LayoutParams).Gravity = GravityFlags.Top;
            (button.LayoutParameters as FrameLayout.LayoutParams).TopMargin = ThemeHelper.AsPx(Context, topPadding);
        }
    }
}