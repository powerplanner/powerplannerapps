using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems;
using Vx.Views;
using Vx;
using PowerPlannerAppDataLibrary.Components.ImageAttachments;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassDetailsViewModel : BaseClassContentViewModel
    {
        [VxSubscribe]
        public ViewItemClass Class { get; private set; }

        public ClassDetailsViewModel(ClassViewModel parent) : base(parent)
        {
            Class = parent.ViewItemsGroupClass.Class;
        }

        protected override View Render()
        {
            return new ScrollView(new LinearLayout
            {
                Margin = new Thickness(VxPlatform.Current == Platform.Uwp ? 12 : Theme.Current.PageMargin),
                Children =
                {
                    !string.IsNullOrWhiteSpace(Class.Details) ? new HyperlinkTextBlock
                    {
                        Text = Class.Details,
                        IsTextSelectionEnabled = true
                    } : null,

                    string.IsNullOrWhiteSpace(Class.Details) ? new TextBlock
                    {
                        Text = R.S("ClassPage_Details_NothingHereString"),
                        TextColor = Theme.Current.SubtleForegroundColor
                    } : null,

                    new ImagesComponent
                    {
                        ImageAttachments = Class.ImageAttachments,
                        Margin = new Thickness(0, 18, 0, 0)
                    }
                }
            });
        }
    }
}
