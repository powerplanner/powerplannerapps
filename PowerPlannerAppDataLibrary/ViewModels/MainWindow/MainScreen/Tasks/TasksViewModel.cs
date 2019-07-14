using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.NavigationManager;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Tasks
{
    public class TasksViewModel : PagedViewModel
    {
        private MainMenuSelections _selectedItem;
        public MainMenuSelections SelectedItem
        {
            get { return _selectedItem; }
            private set { SetProperty(ref _selectedItem, value, nameof(SelectedItem)); }
        }

        public TasksViewModel(MainScreenViewModel parent) : base(parent)
        {
            PropertyChanged += TasksViewModel_PropertyChanged;
        }

        private void TasksViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Content):
                    OnContentChanged();
                    break;
            }
        }

        private void OnContentChanged()
        {
            if (Content != null)
            {
                SelectedItem = GetCurrentSelectionBasedOnContent();
                NavigationManager.MainMenuSelection = SelectedItem;
                PowerPlannerAppDataLibrary.Helpers.Settings.NavigationManagerSettings.TasksViewSelection = SelectedItem;
            }
        }

        private static Dictionary<Type, MainMenuSelections> ContentTypesToMenuSelections = new Dictionary<Type, MainMenuSelections>()
        {
            { typeof(CalendarViewModel), MainMenuSelections.Calendar },
            { typeof(DayViewModel), MainMenuSelections.Day },
            { typeof(AgendaViewModel), MainMenuSelections.Agenda }
        };

        private MainMenuSelections GetCurrentSelectionBasedOnContent()
        {
            if (Content == null)
                throw new NullReferenceException("Content was null");

            if (!ContentTypesToMenuSelections.ContainsKey(Content.GetType()))
            {
                throw new KeyNotFoundException("Please register this content type for menu item selection");
            }

            return ContentTypesToMenuSelections[Content.GetType()];
        }
    }
}
