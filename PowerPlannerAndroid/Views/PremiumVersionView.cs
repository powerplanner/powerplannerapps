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
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;

namespace PowerPlannerAndroid.Views
{
    public class PremiumVersionView : PopupViewHost<PremiumVersionViewModel>
    {
        public PremiumVersionView(ViewGroup root) : base(Resource.Layout.PremiumVersion, root)
        {
            Title = PowerPlannerResources.GetString("Settings_UpdateToPremium.Title");

            FindViewById<Button>(Resource.Id.ButtonUpgradeToPremium).Text = R.S("Settings_UpgradeToPremium_ButtonUpgrade.Content");
            FindViewById<Button>(Resource.Id.ButtonUpgradeToPremium).Click += ButtonUpgradeToPremium_Click;
            FindViewById<TextView>(Resource.Id.TextViewPremiumExplanation).Text = PowerPlannerResources.GetStringPremiumDescription(0, 1, 2);
        }

        public override void OnViewModelSetOverride()
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.ContextualMessage))
            {
                FindViewById<TextView>(Resource.Id.PremiumView_ContextualMessage).Text = ViewModel.ContextualMessage;
                FindViewById<TextView>(Resource.Id.PremiumView_ContextualMessage).Visibility = ViewStates.Visible;
            }

            base.OnViewModelSetOverride();
        }

        private void ButtonUpgradeToPremium_Click(object sender, EventArgs e)
        {
            ViewModel.PromptPurchase();
        }
    }
}