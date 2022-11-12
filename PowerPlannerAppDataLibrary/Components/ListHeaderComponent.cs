using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class ListHeaderComponent : VxComponent
    {
        public string Text { get; set; }

        protected override View Render()
        {
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        BackgroundColor = Theme.Current.BackgroundAlt2Color,
                        Content = new TextBlock
                        {
                            Text = Text,
                            FontSize = Theme.Current.SubtitleFontSize,
                            Margin = new Thickness(12,6,6,6),
                            TextColor = Theme.Current.SubtleForegroundColor
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }
    }
}
