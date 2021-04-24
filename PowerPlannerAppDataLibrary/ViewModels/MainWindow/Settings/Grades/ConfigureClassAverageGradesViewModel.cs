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
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassAverageGradesViewModel : BaseMainScreenViewModelDescendant
    {
        private VxState<bool> _averageGrades;
        private VxState<bool> _isEnabled = new VxState<bool>(true);

        public ViewItemClass Class { get; private set; }

        public ConfigureClassAverageGradesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            c.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Class_PropertyChanged).Handler;
            _averageGrades = new VxState<bool>(c.ShouldAverageGradeTotals);

            _averageGrades.ValueChanged += _averageGrades_ValueChanged;
        }

        private void _averageGrades_ValueChanged(object sender, EventArgs e)
        {
            if (Class.ShouldAverageGradeTotals != _averageGrades.Value)
            {
                Save();
            }
        }

        private void Class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Class.ShouldAverageGradeTotals):
                    _averageGrades.Value = Class.ShouldAverageGradeTotals;
                    break;
            }
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Margin = new Thickness(12),
                Children =
                {
                    new Switch
                    {
                        Title = PowerPlannerResources.GetString("ClassPage_ToggleAverageGrades.Header"),
                        IsOn = _averageGrades,
                        IsEnabled = _isEnabled.Value
                    },

                    new TextBlock
                    {
                        Margin = new Thickness(0, 12, 0, 0),
                        Text = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpBody.Text"),
                        TextColor = Theme.Current.SubtleForegroundColor,
                        WrapText = true
                    }
                }
            };
        }

        private async void Save()
        {
            try
            {
                _isEnabled.Value = false;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    ShouldAverageGradeTotals = _averageGrades.Value
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
                _isEnabled.Value = true;
            }
        }
    }
}
