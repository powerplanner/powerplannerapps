using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.VxTests
{
    public class VxBorderChildAlignmentTestViewModel : PopupComponentViewModel
    {
        public VxBorderChildAlignmentTestViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "Border with centered text",
                        Margin = new Thickness(Theme.Current.PageMargin)
                    },

                    new Border
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = 60,
                        Height = 60,
                        BackgroundColor = Color.Beige,
                        Content = new TextBlock
                        {
                            Text = "11",
                            HorizontalAlignment = HorizontalAlignment.Center, // If I make this TextAlignment, the vertical alignment is thrown off
                            VerticalAlignment = VerticalAlignment.Center
                        }.TitleStyle()
                    }
                }
            };
        }
    }
}
