using System;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassFinalGradeGpaViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        private readonly ViewItemClass _class;

        public ConfigureClassFinalGradeGpaViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Title = "Final grade/GPA";
            _class = c;

            IsOverrideGradeEnabled = c.OverriddenGrade != PowerPlannerSending.Grade.UNGRADED;
            IsOverrideGpaEnabled = c.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED;

            OverriddenGrade = c.Grade * 100;
            OverriddenGpa = c.GPA;

            UseCancelForBack();
            PrimaryCommand = PopupCommand.Save(Save);
        }

        public bool IsOverrideGradeEnabled { get => GetState<bool>(); set => SetState(value); }
        public bool IsOverrideGpaEnabled { get => GetState<bool>(); set => SetState(value); }
        public double? OverriddenGrade { get => GetState<double?>(); set => SetState(value); }
        public double? OverriddenGpa { get => GetState<double?>(); set => SetState(value); }
        public bool IsSaving { get => GetState<bool>(); set => SetState(value); }

        protected override View Render()
        {
            bool isEnabled = !IsSaving;

            return RenderGenericPopupContent(

                new TextBlock
                {
                    Text = "You can override your final grade or GPA for your class using the following settings."
                },

                new Switch
                {
                    Title = "Override grade",
                    IsOn = VxValue.Create(IsOverrideGradeEnabled, v =>
                    {
                        IsOverrideGradeEnabled = v;
                    }),
                    Margin = new Thickness(0, 24, 0, 0),
                    IsEnabled = isEnabled
                },

                new NumberTextBox
                {
                    Number = VxValue.Create(IsOverrideGradeEnabled ? OverriddenGrade : _class.Grade * 100, v => OverriddenGrade = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsOverrideGradeEnabled && isEnabled
                },

                new Switch
                {
                    Title = "Override GPA",
                    IsOn = VxValue.Create(IsOverrideGpaEnabled, v =>
                    {
                        IsOverrideGpaEnabled = v;
                    }),
                    Margin = new Thickness(0, 24, 0, 0),
                    IsEnabled = isEnabled
                },

                new NumberTextBox
                {
                    Number = VxValue.Create(IsOverrideGpaEnabled ? OverriddenGpa : (IsOverrideGradeEnabled && OverriddenGrade != null ? _class.GetGPAForGrade(OverriddenGrade.Value / 100) : _class.GPA), v => OverriddenGpa = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsOverrideGpaEnabled && isEnabled
                }

            );
        }

        private void Save()
        {
            if ((IsOverrideGradeEnabled && OverriddenGrade == null)
                || (IsOverrideGpaEnabled && OverriddenGpa == null))
            {
                new PortableMessageDialog("You must enter valid numbers!", "Invalid entries").Show();
                return;
            }

            TryStartDataOperationAndThenNavigate(delegate
            {
                IsSaving = true;

                DataChanges changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = _class.Identifier,
                    OverriddenGrade = IsOverrideGradeEnabled ? OverriddenGrade.Value / 100 : PowerPlannerSending.Grade.UNGRADED,
                    OverriddenGPA = IsOverrideGpaEnabled ? OverriddenGpa.Value : PowerPlannerSending.Grade.UNGRADED
                };

                changes.Add(c);

                return PowerPlannerApp.Current.SaveChanges(changes);

            }, delegate
            {
                this.RemoveViewModel();
            });
        }
    }
}
