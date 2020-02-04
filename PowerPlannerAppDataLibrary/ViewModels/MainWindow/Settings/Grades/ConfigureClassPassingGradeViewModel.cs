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

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassPassingGradeViewModel : BaseMainScreenViewModelChild
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        /// <summary>
        /// This is represented as 60 rather than 0.6 for easier display purposes on the control.
        /// </summary>
        public double PassingGrade { get; set; }

        public ConfigureClassPassingGradeViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;

            PassingGrade = c.PassingGrade * 100;
        }

        public void Save()
        {
            TryStartDataOperationAndThenNavigate(delegate
            {
                DataChanges changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    PassingGrade = PassingGrade / 100
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
