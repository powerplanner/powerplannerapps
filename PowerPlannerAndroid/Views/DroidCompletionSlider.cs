using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Droid;
using Vx.Droid.Views;

namespace PowerPlannerAndroid.Views
{
    public class DroidCompletionSlider : DroidView<CompletionSlider, TaskProgressBarControl>
    {
        public DroidCompletionSlider() : base(new TaskProgressBarControl(VxDroidExtensions.ApplicationContext))
        {
            View.OnProgressChangedByUser += View_OnProgressChangedByUser;
        }

        private void View_OnProgressChangedByUser(object sender, EventArgs e)
        {
            if (VxView.PercentComplete != null)
            {
                if (View.Progress != VxView.PercentComplete.Value)
                {
                    VxView.PercentComplete.ValueChanged?.Invoke(View.Progress);
                }
            }
        }

        protected override void ApplyProperties(CompletionSlider oldView, CompletionSlider newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.PercentComplete != null)
            {
                View.SetProgress(newView.PercentComplete.Value);
            }
        }
    }
}