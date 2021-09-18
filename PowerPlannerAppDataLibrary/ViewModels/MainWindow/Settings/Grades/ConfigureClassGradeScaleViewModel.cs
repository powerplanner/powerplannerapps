using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGradeScaleViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        private GradeScale[] _currGradeScale;
        private VxState<ViewItemClass[]> _allClasses = new VxState<ViewItemClass[]>(null);

        private bool IsEnabled => _allClasses.Value != null;

        /// <summary>
        /// Windows version should set this to true, so that the previous content remains visible
        /// </summary>
        public bool ShowSaveScalePopupInSeparatePopupPane { get; set; }

        public ConfigureClassGradeScaleViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Title = PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Title");
            UseCancelForBack();
            PrimaryCommand = PopupCommand.Save(Save);

            _currGradeScale = c.GradeScales;
        }

        protected override async void Initialize()
        {
            base.Initialize();

            var yearsGroup = await YearsViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentAccount.LocalAccountId);
            _allClasses.Value = yearsGroup.School.Years.SelectMany(i => i.Semesters).SelectMany(i => i.Classes).ToArray();
        }

        public override void OnViewFocused()
        {
            base.OnViewFocused();

            // Update if changed via the default settings
            if (_currGradeScale != Class.GradeScales)
            {
                _currGradeScale = Class.GradeScales;
                MarkDirty();
            }
        }

        private GradeScaleEditorComponent _gradeScaleEditorComponent;
        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ClassPage_EditGrades_EditGradeScaleForThisClass")
                },

                new TextButton
                {
                    Text = PowerPlannerResources.GetString("ClassPage_EditGrades_EditForAll"),
                    Click = EditDefaultGradeScale,
                    Margin = new Thickness(0, 0, 0, 12),
                    HorizontalAlignment = HorizontalAlignment.Left
                },

                new GradeScaleEditorComponent(() => _allClasses.Value)
                {
                    InitialGradeScale = Class.GradeScales,
                    IsEnabled = IsEnabled,
                    ViewRef = view => _gradeScaleEditorComponent = view as GradeScaleEditorComponent
                }
            );
        }

        private void EditDefaultGradeScale()
        {
            ConfigureClassGradesListViewModel.ShowViewModel<ConfigureDefaultGradeScaleViewModel>(this);
        }

        public async void Save()
        {
            try
            {
                if (!_gradeScaleEditorComponent.CheckIfValid())
                {
                    return;
                }

                GradeScale[] newScales = _gradeScaleEditorComponent.GetGradeScales();

                DataChanges changes = new DataChanges();

                // Class changes
                {
                    var c = new DataItemClass()
                    {
                        Identifier = Class.Identifier
                    };

                    c.SetGradeScales(newScales);

                    changes.Add(c);
                }

                TryStartDataOperationAndThenNavigate(delegate
                {
                    return PowerPlannerApp.Current.SaveChanges(changes);

                }, delegate
                {
                    this.RemoveViewModel();
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }
        }
    }
}
