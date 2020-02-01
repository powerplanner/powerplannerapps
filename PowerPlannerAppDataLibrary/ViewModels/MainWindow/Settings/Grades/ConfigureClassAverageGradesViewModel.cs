using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassAverageGradesViewModel : BaseMainScreenViewModelDescendant
    {
        private bool m_averageGrades;
        public bool AverageGrades
        {
            get { return m_averageGrades; }
            set
            {
                if (m_averageGrades != value)
                {
                    m_averageGrades = value;
                    OnPropertyChanged(nameof(AverageGrades));
                    Save();
                }
            }
        }

        public bool IsEnabled { get; set; } = true;

        public ViewItemClass Class { get; private set; }

        public ConfigureClassAverageGradesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            c.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Class_PropertyChanged).Handler;
            m_averageGrades = c.ShouldAverageGradeTotals;
        }

        private void Class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Class.ShouldAverageGradeTotals):
                    SetProperty(ref m_averageGrades, Class.ShouldAverageGradeTotals, nameof(AverageGrades));
                    break;
            }
        }

        private async void Save()
        {
            try
            {
                IsEnabled = false;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    ShouldAverageGradeTotals = AverageGrades
                };

                changes.Add(c);

                await TryHandleUserInteractionAsync("Save", async delegate
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);

                }, "Failed to save. Your error has been reported.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }

            finally
            {
                IsEnabled = true;
            }
        }
    }
}
