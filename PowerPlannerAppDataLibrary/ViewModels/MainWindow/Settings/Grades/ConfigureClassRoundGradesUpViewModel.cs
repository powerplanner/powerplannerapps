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
    public class ConfigureClassRoundGradesUpViewModel : PopupComponentViewModel
    {
        private VxState<bool> _roundGradesUp;
        private VxState<bool> _isEnabled = new VxState<bool>(true);

        public ViewItemClass Class { get; private set; }

        public ConfigureClassRoundGradesUpViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Title = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text");

            _roundGradesUp = new VxState<bool>(c.DoesRoundGradesUp);
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
                        Title = PowerPlannerResources.GetString("ClassPage_ToggleRoundGradesUp.Header"),
                        IsOn = _roundGradesUp,
                        IsOnChanged = Save,
                        IsEnabled = _isEnabled.Value
                    },

                    new TextBlock
                    {
                        Margin = new Thickness(0, 12, 0, 0),
                        Text = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpBody.Text"),
                        TextColor = Theme.Current.SubtleForegroundColor,
                        WrapText = true
                    }
                }
            };
        }

        private async void Save(bool roundGradesUp)
        {
            try
            {
                _roundGradesUp.Value = roundGradesUp;
                _isEnabled.Value = false;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    DoesRoundGradesUp = roundGradesUp
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
                _roundGradesUp.Value = Class.DoesRoundGradesUp;
            }

            finally
            {
                _isEnabled.Value = true;
            }
        }
    }
}
