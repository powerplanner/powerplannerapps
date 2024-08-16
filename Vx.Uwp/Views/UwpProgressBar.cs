using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vx.Uwp.Views
{
    internal class UwpProgressBar : UwpView<Vx.Views.ProgressBar, ProgressBar>
    {
        protected override void ApplyProperties(Vx.Views.ProgressBar oldView, Vx.Views.ProgressBar newView)
        {
            base.ApplyProperties(oldView, newView);

            View.IsIndeterminate = newView.IsIndeterminate;
            View.Value = newView.Value;
            View.Maximum = newView.MaxValue;
            View.Foreground = newView.Color.ToUwpBrush();
        }
    }
}
