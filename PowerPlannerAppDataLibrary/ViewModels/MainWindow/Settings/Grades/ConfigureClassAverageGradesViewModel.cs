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
        private VxState<bool> _isEnabled = new VxState<bool>(true);

        [VxSubscribe]
        public ViewItemClass Class { get; private set; }

        public ConfigureClassAverageGradesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Children =
                {
                    new Switch
                    {
                        Title = PowerPlannerResources.GetString("ClassPage_ToggleAverageGrades.Header"),
                        IsOn = Class.ShouldAverageGradeTotals,
                        IsOnChanged = Save,
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

        private async void Save(bool averageGrades)
        {
            try
            {
                _isEnabled.Value = false;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    ShouldAverageGradeTotals = averageGrades
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
