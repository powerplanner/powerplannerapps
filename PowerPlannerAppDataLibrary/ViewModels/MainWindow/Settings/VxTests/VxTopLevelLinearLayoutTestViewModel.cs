using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.VxTests
{
    public class VxTopLevelLinearLayoutTestViewModel : PopupComponentViewModel
    {
        public VxTopLevelLinearLayoutTestViewModel(BaseViewModel parent) : base(parent)
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
                        Text = "First half (similar to calendar split view)",
                        Margin = new Thickness(Theme.Current.PageMargin)
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Children =
                        {
                            new LinearLayout
                            {
                                Orientation = Orientation.Horizontal,
                                BackgroundColor = Color.Beige,
                                Margin = new Thickness(Theme.Current.PageMargin),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = "Second half header"
                                    }.LinearLayoutWeight(1),

                                    new TextBlock
                                    {
                                        Text = "Add",
                                        WrapText = false
                                    }
                                }
                            },

                            new Border
                            {
                                BackgroundColor = Color.BlanchedAlmond,
                                Content = new TextBlock
                                {
                                    Text = "Second half content (should fill)",
                                    Margin = new Thickness(Theme.Current.PageMargin)
                                }
                            }.LinearLayoutWeight(1)
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }
    }
}
