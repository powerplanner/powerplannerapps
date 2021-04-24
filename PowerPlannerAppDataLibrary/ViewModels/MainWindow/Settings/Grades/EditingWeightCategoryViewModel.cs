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

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name), nameof(IsNameValid)); }
        }

        private double? _weight = 0;
        public double? Weight
        {
            get { return _weight; }
            set { SetProperty(ref _weight, value, nameof(Weight), nameof(IsWeightValueValid)); }
        }

        public bool IsWeightValueValid
        {
            get { return Weight != null && Weight.Value >= 0; }
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
