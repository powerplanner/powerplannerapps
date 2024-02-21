using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class MoveClassViewModel : PopupComponentViewModel
    {
        private ViewItemClass _class;
        private ViewItemYear[] _years;
        private VxState<ViewItemYear> _selectedYear;
        private VxState<ViewItemSemester> _selectedSemester;

        public MoveClassViewModel(BaseViewModel parent, ViewItemClass classToMove, ViewItemYear[] years) : base(parent)
        {
            _class = classToMove;

            _years = years.Where(y => GetAvailableSemesters(y).Any()).ToArray();
            _selectedYear = new VxState<ViewItemYear>(_years.FirstOrDefault());
            if (_selectedYear.Value == null)
            {
                _selectedSemester = new VxState<ViewItemSemester>();
            }
            else
            {
                _selectedSemester = new VxState<ViewItemSemester>(GetAvailableSemesters(_years.FirstOrDefault()).FirstOrDefault());
            }

            Title = PowerPlannerResources.GetStringWithParameters("SemesterActions_MoveClass", _class.Name).ToUpper();

            PrimaryCommand = PopupCommand.Save(Save);
        }

        private IEnumerable<ViewItemSemester> GetAvailableSemesters(ViewItemYear year)
        {
            return year.Semesters.Where(s => s != _class.Semester);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                // Year picker
                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("EditSemesterPage_Year.Header"),
                    Items = _years,
                    SelectedItem = VxValue.Create<object>(_selectedYear.Value, v => _selectedYear.Value = v as ViewItemYear),
                    ItemTemplate = y => new TextBlock
                    {
                        Text = (y as ViewItemYear).Name
                    }
                },

                // Semester picker
                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("Header_Semester"),
                    Margin = new Thickness(0, 18, 0, 0),
                    Items = _selectedYear.Value == null ? null : GetAvailableSemesters(_selectedYear.Value),
                    SelectedItem = VxValue.Create<object>(_selectedSemester.Value, v => _selectedSemester.Value = v as ViewItemSemester),
                    ItemTemplate = y => new TextBlock
                    {
                        Text = (y as ViewItemSemester).Name
                    }
                }
            );
        }

        public void Save()
        {
            if (_selectedSemester.Value == null)
            {
                // Not localizing this since it should theoretically never get hit
                new PortableMessageDialog("You must select a semester to move the class into.", "No semester").Show();
                return;
            }

            TryStartDataOperationAndThenNavigate(async delegate
            {
                // If we're moving the class into the current semester, we're going to need a reset (since I didn't build the system with expectation that classes would move).
                bool needsReset = MainScreenViewModel.CurrentSemesterId == _selectedSemester.Value.Identifier;

                DataChanges changes = new DataChanges();
                changes.Add(new DataItemClass
                {
                    Identifier = _class.Identifier,
                    UpperIdentifier = _selectedSemester.Value.Identifier
                });

                await PowerPlannerApp.Current.SaveChanges(changes);

                TelemetryExtension.Current?.TrackEvent("MovedClass");

                if (needsReset)
                {
                    // Remove current semester, which will force re-loading the classes when user chooses to open it
                    SemesterItemsViewGroup.ClearCache();
                    ScheduleViewItemsGroup.ClearCache();
                    AgendaViewItemsGroup.ClearCache();
                    ViewLists.SchedulesOnDay.ClearCached();
                    ViewLists.DayScheduleItemsArranger.ClearCached();
                    await MainScreenViewModel.SetCurrentSemester(Guid.Empty, closeYearsPopup: false);
                }
            }, RemoveViewModel);
        }
    }
}
