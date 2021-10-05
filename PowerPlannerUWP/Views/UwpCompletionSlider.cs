using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;

namespace PowerPlannerUWP.Views
{
    public class UwpCompletionSlider : UwpView<PowerPlannerAppDataLibrary.Views.CompletionSlider, CompletionSlider>
    {
        public UwpCompletionSlider()
        {
            View.OnValueChangedByUser += View_OnValueChangedByUser;
        }

        private void View_OnValueChangedByUser(object sender, double e)
        {
            if (VxView?.PercentComplete?.Value != null && VxView.PercentComplete.Value != e)
            {
                VxView.PercentComplete.ValueChanged?.Invoke(e);
            }
        }

        protected override void ApplyProperties(PowerPlannerAppDataLibrary.Views.CompletionSlider oldView, PowerPlannerAppDataLibrary.Views.CompletionSlider newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Value = newView.PercentComplete?.Value ?? 0;
        }
    }
}
