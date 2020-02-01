using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class EditingWeightCategoryViewModel : BindableBase
    {
        public Guid Identifier { get; set; }

        [DependsOn(nameof(IsNameValid))]
        public string Name { get; set; } = "";

        [DependsOn(nameof(IsWeightValueValid))]
        public double Weight { get; set; } = 0;

        public bool IsWeightValueValid
        {
            get { return Weight >= 0; }
        }

        public bool IsValid
        {
            get { return IsNameValid && IsWeightValueValid; }
        }

        public bool IsNameValid
        {
            get { return !string.IsNullOrWhiteSpace(Name); }
        }
    }
}
