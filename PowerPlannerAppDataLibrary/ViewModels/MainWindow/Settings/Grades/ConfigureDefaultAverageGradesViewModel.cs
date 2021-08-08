using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureDefaultAverageGradesViewModel : BaseConfigureDefaultGradesPageViewModel
    {
        private VxState<bool> _averageGrades;
        private VxState<bool> _hasUnsavedChanges = new VxState<bool>(false);

        public ConfigureDefaultAverageGradesViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_DefaultGradeOptions_AverageGrades");
            _averageGrades = new VxState<bool>(Account.DefaultDoesAverageGradeTotals);
        }

        protected override View Render()
        {
            var layout = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
            };

            layout.Children.Add(new Switch
            {
                Title = PowerPlannerResources.GetString("ClassPage_ToggleAverageGrades.Header"),
                IsOn = VxValue.Create(_averageGrades.Value, isOn =>
                {
                    _averageGrades.Value = isOn;
                    _hasUnsavedChanges.Value = Account.DefaultDoesAverageGradeTotals != isOn;
                }),
                IsEnabled = IsEnabled
            });

            if (_hasUnsavedChanges || State == States.Applied)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = PowerPlannerResources.GetString(_hasUnsavedChanges ? "String_Unsaved" : "String_Saved"),
                    Margin = new Thickness(0, 6, 0, 0),
                    TextColor = Theme.Current.SubtleForegroundColor,
                    FontSize = Theme.Current.CaptionFontSize
                });
            }

            layout.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                Text = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpBody.Text"),
                TextColor = Theme.Current.SubtleForegroundColor
            });

            RenderApplyUI(layout);

            return new ScrollView
            {
                Content = layout
            };
        }

        protected override async Task Apply()
        {
            DataChanges changes = new DataChanges();

            foreach (var c in SelectedClasses)
            {
                var change = new DataItemClass
                {
                    Identifier = c.Identifier,
                    ShouldAverageGradeTotals = _averageGrades
                };

                changes.Add(change);
            }

            await TryHandleUserInteractionAsync("save", async delegate
            {
                Account.DefaultDoesAverageGradeTotals = _averageGrades;
                Account.NeedsToSyncSettings = true;
                await AccountsManager.Save(Account);

                if (!changes.IsEmpty())
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                }
                else
                {
                    _ = Sync.SyncSettings(Account, Sync.ChangedSetting.DefaultDoesAverageGradeTotals);
                }

                TelemetryExtension.Current?.TrackEvent("AppliedDefaultAverageGrades");
            });

            _hasUnsavedChanges.Value = false;
        }
    }
}
