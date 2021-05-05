using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureDefaultRoundGradesUpViewModel : BaseConfigureDefaultGradesPageViewModel
    {
        private VxState<bool> _roundGradesUp;
        private VxState<bool> _hasUnsavedChanges = new VxState<bool>(false);

        public ConfigureDefaultRoundGradesUpViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Default round grades up";
            _roundGradesUp = new VxState<bool>(Account.DefaultDoesRoundGradesUp);
        }

        protected override View Render()
        {
            var layout = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
            };

            if (VxPlatform.Current == Platform.Uwp)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = Title.ToUpper(),
                    Margin = new Thickness(0, 0, 0, 12)
                }.TitleStyle());
            }

            layout.Children.Add(new Switch
            {
                Title = PowerPlannerResources.GetString("ClassPage_ToggleRoundGradesUp.Header"),
                IsOn = _roundGradesUp,
                IsOnChanged = isOn =>
                {
                    _roundGradesUp.Value = isOn;
                    _hasUnsavedChanges.Value = Account.DefaultDoesRoundGradesUp != isOn;
                },
                IsEnabled = IsEnabled
            });

            if (_hasUnsavedChanges || State == States.Applied)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = _hasUnsavedChanges ? "Unsaved" : "Saved!",
                    Margin = new Thickness(0, 6, 0, 0),
                    TextColor = Theme.Current.SubtleForegroundColor,
                    FontSize = Theme.Current.CaptionFontSize
                });
            }

            layout.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                Text = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpBody.Text"),
                TextColor = Theme.Current.SubtleForegroundColor,
                WrapText = true
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
                    DoesRoundGradesUp = _roundGradesUp
                };

                changes.Add(change);
            }

            await TryHandleUserInteractionAsync("save", async delegate
            {
                Account.DefaultDoesRoundGradesUp = _roundGradesUp;
                Account.NeedsToSyncSettings = true;
                await AccountsManager.Save(Account);

                if (!changes.IsEmpty())
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                }
                else
                {
                    _ = Sync.SyncSettings(Account, Sync.ChangedSetting.DefaultDoesRoundGradesUp);
                }
            });

            _hasUnsavedChanges.Value = false;
        }
    }
}
