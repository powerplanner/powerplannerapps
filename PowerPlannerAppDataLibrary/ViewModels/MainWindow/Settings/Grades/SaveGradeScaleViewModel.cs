using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerSending;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class SaveGradeScaleViewModel : BaseMainScreenViewModelChild
    {
        public new MainScreenViewModel MainScreenViewModel { get; private set; }

        public SaveGradeScaleViewModel(BaseViewModel parent, MainScreenViewModel mainScreenViewModel, Parameter parameter) : base(parent)
        {
            // We're using a higher parent so that this popup appears above the current popup
            MainScreenViewModel = mainScreenViewModel;
            _parameter = parameter;

            if (parameter.Name != null)
            {
                Name = parameter.Name;
            }
        }

        private Parameter _parameter;

        public class Parameter
        {
            public string Name { get; set; }

            public GradeScale[] Scales { get; set; }

            public Action OnSaved { get; set; }
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        public async void Save()
        {
            try
            {
                string name = Name.Trim();

                if (name.Length == 0)
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("String_NoNameMessageBody")).ShowAsync();
                    return;
                }

                await TryHandleUserInteractionAsync("Save", async (cancellationTask) =>
                {
                    SavedGradeScalesManager manager = await SavedGradeScalesManager.GetForAccountAsync(MainScreenViewModel.CurrentAccount);
                    cancellationTask.ThrowIfCancellationRequested();

                    if (manager == null)
                        throw new NullReferenceException("manager was null");

                    await manager.SaveGradeScale(name, _parameter.Scales);

                    if (_parameter.OnSaved != null)
                        _parameter.OnSaved.Invoke();

                    // We cancel after invoking the OnSaved, since otherwise the views wouldn't be updated correctly
                    cancellationTask.ThrowIfCancellationRequested();

                    this.RemoveViewModel();

                }, "Failed to save. Your error has been reported.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                await new PortableMessageDialog("Failed to save. Your error has been reported.").ShowAsync();
                return;
            }
        }
    }
}
