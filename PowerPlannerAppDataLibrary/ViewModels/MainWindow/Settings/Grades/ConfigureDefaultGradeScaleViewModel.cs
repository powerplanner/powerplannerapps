using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx;
using Vx.Views;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades.ConfigureClassGradeScaleViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureDefaultGradeScaleViewModel : BaseConfigureDefaultGradesPageViewModel
    {
        public ConfigureDefaultGradeScaleViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_DefaultGradeOptions_GradeScale");
        }

        private GradeScaleEditorComponent _gradeScaleEditorComponent;

        protected override View Render()
        {
            var views = new List<View>()
            {
                new GradeScaleEditorComponent(() => AllClasses, changed: MarkDirty)
                {
                    InitialGradeScale = Account.DefaultGradeScale,
                    IsEnabled = IsEnabled,
                    ViewRef = view => _gradeScaleEditorComponent = view as GradeScaleEditorComponent
                }
            };

            RenderApplyUI(views);

            return RenderGenericPopupContent(views);
        }

        protected override bool CanApply()
        {
            if (!_gradeScaleEditorComponent.CheckIfValid())
            {
                return false;
            }

            return true;
        }

        protected override async System.Threading.Tasks.Task Apply()
        {
            GradeScale[] newScales = _gradeScaleEditorComponent.GetGradeScales();

            DataChanges changes = new DataChanges();

            foreach (var c in SelectedClasses)
            {
                if (!newScales.SequenceEqual(c.GradeScales))
                {
                    var change = new DataItemClass
                    {
                        Identifier = c.Identifier
                    };

                    change.SetGradeScales(newScales);

                    changes.Add(change);
                }
            }

            await TryHandleUserInteractionAsync("save", async delegate
            {
                await Account.SaveDefaultGradeScale(newScales);

                if (!changes.IsEmpty())
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                }
                else
                {
                    _ = Sync.SyncSettings(Account, Sync.ChangedSetting.DefaultGradeScale);
                }

                TelemetryExtension.Current?.TrackEvent("AppliedDefaultGradeScale");
            });
        }
    }
}
