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
        private bool _currRoundGradesUp;

        public ConfigureClassRoundGradesUpViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Title = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text");

            _roundGradesUp = new VxState<bool>(c.DoesRoundGradesUp);
            _currRoundGradesUp = c.DoesRoundGradesUp;
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
                        Text = PowerPlannerResources.GetString("ClassPage_EditRoundGradesUpForThisClass")
                    },

                    new TextButton
                    {
                        Text = PowerPlannerResources.GetString("ClassPage_EditGrades_EditForAll"),
                        Click = EditDefaultRoundGradesUp,
                        Margin = new Thickness(0, 0, 0, 12),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        IsEnabled = _isEnabled
                    },

                    new Switch
                    {
                        Title = PowerPlannerResources.GetString("ClassPage_ToggleRoundGradesUp.Header"),
                        IsOn = VxValue.Create(_roundGradesUp.Value, Save),
                        IsEnabled = _isEnabled
                    },

                    new TextBlock
                    {
                        Margin = new Thickness(0, 12, 0, 0),
                        Text = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpBody.Text"),
                        TextColor = Theme.Current.SubtleForegroundColor
                    }
                }
            };
        }

        public override void OnViewFocused()
        {
            base.OnViewFocused();

            // Update if changed via the default settings
            if (_currRoundGradesUp != Class.DoesRoundGradesUp)
            {
                _currRoundGradesUp = Class.DoesRoundGradesUp;
                _roundGradesUp.Value = Class.DoesRoundGradesUp;
            }
        }

        private void EditDefaultRoundGradesUp()
        {
            ConfigureClassGradesListViewModel.ShowViewModel<ConfigureDefaultRoundGradesUpViewModel>(this);
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
                    _currRoundGradesUp = roundGradesUp;

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
