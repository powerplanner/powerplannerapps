using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSProgressBar : iOSView<Vx.Views.ProgressBar, UIProgressBarWithIndeterminate>
    {
        protected override void ApplyProperties(ProgressBar oldView, ProgressBar newView)
        {
            base.ApplyProperties(oldView, newView);

            View.IsIndeterminate = newView.IsIndeterminate;
            View.Value = newView.Value;
            View.Maximum = newView.MaxValue;
            View.Foreground = newView.Color.ToUI();
        }
    }
}
