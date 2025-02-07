using Android.Content.Res;
using Android.Widget;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vx.Droid.Views
{
    internal class DroidProgressBar : DroidView<Vx.Views.ProgressBar, ProgressBar>
    {
        public DroidProgressBar() : base(new ProgressBar(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(Vx.Views.ProgressBar oldView, Vx.Views.ProgressBar newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Indeterminate = newView.IsIndeterminate;
            View.Max = (int)(100 * newView.MaxValue);
            View.Progress = (int)(100 * newView.Value);
            View.ProgressTintList = ColorTools.GetColorStateList(newView.Color.ToDroid());
            View.IndeterminateTintList = ColorTools.GetColorStateList(newView.Color.ToDroid());
        }
    }
}
