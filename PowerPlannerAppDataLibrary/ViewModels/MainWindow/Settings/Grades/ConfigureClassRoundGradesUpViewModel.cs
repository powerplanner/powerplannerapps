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
    public class ConfigureClassRoundGradesUpViewModel : BaseMainScreenViewModelDescendant
    {
        private bool m_roundGradesUp;
        public bool RoundGradesUp
        {
            get { return m_roundGradesUp; }
            set
            {
                if (m_roundGradesUp != value)
                {
                    m_roundGradesUp = value;
                    OnPropertyChanged(nameof(RoundGradesUp));
                    Save();
                }
            }
        }

        public bool IsEnabled { get; set; } = true;

        public ViewItemClass Class { get; private set; }

        public ConfigureClassRoundGradesUpViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            c.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Class_PropertyChanged).Handler;
            m_roundGradesUp = c.DoesRoundGradesUp;
        }

        private void Class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Class.DoesRoundGradesUp):
                    SetProperty(ref m_roundGradesUp, Class.DoesRoundGradesUp, nameof(RoundGradesUp));
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
                    DoesRoundGradesUp = RoundGradesUp
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
