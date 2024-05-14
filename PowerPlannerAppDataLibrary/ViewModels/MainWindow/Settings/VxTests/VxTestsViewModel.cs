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
                        RenderOption<VxLinearLayoutTestsViewModel>("LinearLayout tests"),
                        RenderOption<VxTopLevelLinearLayoutTestViewModel>("Top level LinearLayout"),
                        RenderOption<VxBorderChildAlignmentTestViewModel>("Border child alignment"),
                        RenderOption<VxScrollViewerTestViewModel>("Scroll view test"),
                        RenderOption<VxTransparentContentButtonTestViewModel>("Transparent content button test")
                    }
                }
            };
        }

        private View RenderOption<T>(string name) where T : PopupComponentViewModel
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
                    var model = Activator.CreateInstance(typeof(T), this) as T;
                    ShowPopup(model);
                }
            };
        }
    }
}
