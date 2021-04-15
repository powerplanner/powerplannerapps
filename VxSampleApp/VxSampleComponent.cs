using System;
using Vx;
using Vx.Views;

namespace VxSampleApp
{
    public class VxSampleComponent : VxComponent
    {
        private VxState<int> _timesClicked = new VxState<int>(0);

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Clicked {_timesClicked.Value} times"
                    },

                    new Button
                    {
                        Text = "Click me",
                        Click = delegate { _timesClicked.Value++; }
                    }
                }
            };
        }
    }
}
