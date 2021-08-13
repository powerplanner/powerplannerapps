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
    public class ConfigureClassAverageGradesViewModel : PopupComponentViewModel
    {
        private VxState<bool> _averageGrades;
        private VxState<bool> _isEnabled = new VxState<bool>(true);

        public ViewItemClass Class { get; private set; }
        private bool _currAverageGrades;

        public ConfigureClassAverageGradesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            _averageGrades = new VxState<bool>(c.ShouldAverageGradeTotals);
            _currAverageGrades = c.ShouldAverageGradeTotals;

            Title = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text");
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Children =
                {
                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("ClassPage_EditAverageGradesForThisClass")
                    },

                    new TextButton
                    {
                        Text = PowerPlannerResources.GetString("ClassPage_EditGrades_EditForAll"),
                        Click = EditDefaultAverageGrades,
                        Margin = new Thickness(0, 0, 0, 12),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        IsEnabled = _isEnabled
                    },

                    new Switch
                    {
                        Title = PowerPlannerResources.GetString("ClassPage_ToggleAverageGrades.Header"),
                        IsOn = VxValue.Create(_averageGrades.Value, Save),
                        IsEnabled = _isEnabled
                    },

                    new TextBlock
                    {
                        Margin = new Thickness(0, 12, 0, 0),
                        Text = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpBody.Text"),
                        TextColor = Theme.Current.SubtleForegroundColor
                    }
                }
            };
        }

        public override void OnViewFocused()
        {
            base.OnViewFocused();

            // Update if changed via the default settings
            if (_currAverageGrades != Class.ShouldAverageGradeTotals)
            {
                _currAverageGrades = Class.ShouldAverageGradeTotals;
                _averageGrades.Value = Class.ShouldAverageGradeTotals;
            }
        }

        private void EditDefaultAverageGrades()
        {
            ConfigureClassGradesListViewModel.ShowViewModel<ConfigureDefaultAverageGradesViewModel>(this);
        }

        private async void Save(bool averageGrades)
        {
            try
            {
                _isEnabled.Value = false;
                _averageGrades.Value = averageGrades;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    ShouldAverageGradeTotals = _averageGrades
                };

                changes.Add(c);

                await TryHandleUserInteractionAsync("Save", async delegate
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                    _currAverageGrades = averageGrades;

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
