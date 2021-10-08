using System;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Views
{
    public class CompletionSlider : View
    {
        /// <summary>
        /// Decimal from 0.0 to 1.0
        /// </summary>
        public VxValue<double> PercentComplete { get; set; }
    }
}
