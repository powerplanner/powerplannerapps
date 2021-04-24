using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassPassingGradeViewModel : BaseMainScreenViewModelChild
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        public ConfigureClassPassingGradeViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;

            _passingGrade = new VxState<double?>(c.PassingGrade * 100);
        }

        /// <summary>
        /// This is represented as 60 rather than 0.6 for easier display purposes on the control.
        /// </summary>
        private VxState<double?> _passingGrade;

        protected override View Render()
        {
            return new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Children =
                {
                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title"),
                        FontWeight = FontWeights.Bold
                    },

                    new NumberTextBox
                    {
                        Number = _passingGrade,
                        PlaceholderText = PowerPlannerResources.GetExamplePlaceholderString(60.ToString())
                    },

                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("Settings_GradeOptions_PassingGrade_Explanation.Text"),
                        TextColor = Theme.Current.SubtleForegroundColor,
                        WrapText = true,
                        Margin = new Thickness(0, 12, 0, 0)
                    }
                }
            };
        }

        public void Save()
        {
            if (_passingGrade.Value == null || _passingGrade.Value.Value < 0)
            {
                new PortableMessageDialog("You must enter a valid non-negative number.", "Invalid grade").Show();
                return;
            }

            TryStartDataOperationAndThenNavigate(delegate
            {
                DataChanges changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    PassingGrade = _passingGrade.Value.Value / 100
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
