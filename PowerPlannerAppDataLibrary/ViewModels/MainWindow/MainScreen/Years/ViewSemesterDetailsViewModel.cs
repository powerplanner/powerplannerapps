using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class ViewSemesterDetailsViewModel : PopupComponentViewModel
    {
        [VxSubscribe]
        public ViewItemSemester Semester { get; private set; }

        public ViewSemesterDetailsViewModel(YearsViewModel parent, ViewItemSemester semester) : base(parent)
        {
            Semester = semester;
            Title = semester.Name;
            PrimaryCommand = PopupCommand.Edit(Edit);
        }

        protected override View Render()
        {
            if (Title != Semester.Name)
            {
                Title = Semester.Name;
            }

            return RenderGenericPopupContent(
                new HyperlinkTextBlock
                {
                    Text = Semester.Details,
                    IsTextSelectionEnabled = true
                });
        }

        private void Edit()
        {
            ((YearsViewModel)Parent).EditSemester(Semester);
        }
    }
}
