using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class MoveSemesterViewModel : PopupComponentViewModel
    {
        private ViewItemSemester _semester;
        private ViewItemYear[] _otherYears;
        private VxState<ViewItemYear> _copySelectedYear;

        public MoveSemesterViewModel(BaseViewModel parent, ViewItemSemester semesterToMove, ViewItemYear[] years) : base(parent)
        {
            _semester = semesterToMove;
            _otherYears = years.Where(i => i.Identifier != semesterToMove.Year.Identifier).ToArray();
            _copySelectedYear = new VxState<ViewItemYear>(_otherYears.FirstOrDefault());

            Title = "Move Semester";

            PrimaryCommand = PopupCommand.Save(Save);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                // Year picker
                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("EditSemesterPage_Year.Header"),
                    Items = _otherYears,
                    SelectedItem = VxValue.Create<object>(_copySelectedYear.Value, v => _copySelectedYear.Value = v as ViewItemYear),
                    ItemTemplate = y => new TextBlock
                    {
                        Text = (y as ViewItemYear).Name
                    }
                }
            );
        }

        public void Save()
        {
            if (_copySelectedYear.Value == null)
            {
                // Not localizing this since it should theoretically never get hit
                new PortableMessageDialog("You must select a year to move the semester into.", "No year").Show();
                return;
            }

            TryStartDataOperationAndThenNavigate(async delegate
            {
                DataChanges changes = new DataChanges();
                changes.Add(new DataItemSemester
                {
                    Identifier = _semester.Identifier,
                    UpperIdentifier = _copySelectedYear.Value.Identifier
                });

                await PowerPlannerApp.Current.SaveChanges(changes);

                TelemetryExtension.Current?.TrackEvent("MovedSemester");
            }, RemoveViewModel);
        }
    }
}
