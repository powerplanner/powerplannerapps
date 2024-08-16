using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class ViewYearDetailsViewModel : PopupComponentViewModel
    {
        [VxSubscribe]
        public ViewItemYear Year { get; private set; }

        public ViewYearDetailsViewModel(YearsViewModel parent, ViewItemYear year) : base(parent)
        {
            Year = year;
            Title = year.Name;
            PrimaryCommand = PopupCommand.Edit(Edit);
        }

        protected override View Render()
        {
            if (Title != Year.Name)
            {
                Title = Year.Name;
            }

            return RenderGenericPopupContent(
                new HyperlinkTextBlock
                {
                    Text = Year.Details,
                    IsTextSelectionEnabled = true
                });
        }

        private void Edit()
        {
            ((YearsViewModel)Parent).EditYear(Year);
        }
    }
}
