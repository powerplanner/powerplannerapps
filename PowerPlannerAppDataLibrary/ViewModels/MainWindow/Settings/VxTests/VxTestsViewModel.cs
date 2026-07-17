using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.VxTests
{
    public class VxTestsViewModel : PopupComponentViewModel
    {
        public VxTestsViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Children =
                    {
                        RenderOption("LinearLayout tests", () => new VxLinearLayoutTestsViewModel(this)),
                        RenderOption("Top level LinearLayout", () => new VxTopLevelLinearLayoutTestViewModel(this)),
                        RenderOption("Border child alignment", () => new VxBorderChildAlignmentTestViewModel(this)),
                        RenderOption("Scroll view test", () => new VxScrollViewerTestViewModel(this)),
                        RenderOption("Transparent content button test", () => new VxTransparentContentButtonTestViewModel(this))
                    }
                }
            };
        }

        private View RenderOption(string name, Func<PopupComponentViewModel> createViewModel)
        {
            return new TransparentContentButton
            {
                Content = new TextBlock
                {
                    Text = name,
                    Margin = new Thickness(Theme.Current.PageMargin)
                },
                Click = () =>
                {
                    ShowPopup(createViewModel());
                }
            };
        }
    }
}
