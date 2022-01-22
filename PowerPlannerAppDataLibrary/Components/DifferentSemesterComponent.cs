using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class DifferentSemesterComponent : VxComponent
    {
        public Action OnDismiss { get; set; }

        public Action OnOpenSemester { get; set; }

        protected override View Render()
        {
            // Background that fills entire control
            return new FrameLayout
            {
                BackgroundColor = Theme.Current.BackgroundColor.Opacity(0.4),
                Tapped = OnDismiss,
                Children =
                {
                    // Center square 
                    new Border
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        BackgroundColor = Theme.Current.ForegroundColor.Opacity(0.9),
                        CornerRadius = 6,
                        Padding = new Thickness(12),
                        Tapped = OnOpenSemester,
                        Content = new LinearLayout
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = PowerPlannerResources.GetString("DifferentSemesterOverlayControl_TextBlockHeader.Text"),
                                    TextColor = Theme.Current.BackgroundColor,
                                    TextAlignment = HorizontalAlignment.Center
                                }.TitleStyle(),

                                new TextBlock
                                {
                                    Text = PowerPlannerResources.GetString("DifferentSemesterOverlayControl_TextBlockDescription.Text"),
                                    TextColor = Theme.Current.BackgroundColor,
                                    TextAlignment = HorizontalAlignment.Center,
                                    FontSize = Theme.Current.CaptionFontSize
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
