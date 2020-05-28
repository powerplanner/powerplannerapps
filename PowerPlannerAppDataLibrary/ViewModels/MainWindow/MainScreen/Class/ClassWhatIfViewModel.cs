using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassWhatIfViewModel : BaseMainScreenViewModelChild
    {
        private ViewItemClass _originalClass;

        /// <summary>
        /// Class must already have weights and grades loaded
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="c"></param>
        public ClassWhatIfViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            _originalClass = c;
        }

        private ViewItemClass _class;
        public ViewItemClass Class
        {
            get { return _class; }
            private set { SetProperty(ref _class, value, nameof(Class)); }
        }

        /// <summary>
        /// Here so that when viewing What If, the weight categories will be displayed. We don't bother dynamically updating this.
        /// </summary>
        public bool ShowWeightCategoriesSummary { get => true; }

        private double _desiredGrade;
        public double DesiredGrade
        {
            get { return _desiredGrade; }
            set
            {
                if (_desiredGrade == value || value == PowerPlannerSending.Grade.UNGRADED)
                {
                    return;
                }

                _desiredGrade = value;
                OnPropertyChanged(nameof(DesiredGrade));
                OnDesiredGradeChanged();
            }
        }

        private double _desiredGPA;

        public double DesiredGPA
        {
            get { return _desiredGPA; }
            set
            {
                if (_desiredGPA == value)
                {
                    return;
                }

                _desiredGPA = value;
                OnPropertyChanged(nameof(DesiredGPA));
                OnDesiredGPAChanged();
            }
        }

        private void OnDesiredGradeChanged()
        {
            DesiredErrorMessage = null;

            _desiredGPA = Class.GetGPAForGrade(DesiredGrade);
            OnPropertyChanged(nameof(DesiredGPA));

            SetDream(DesiredGrade);
        }

        private void OnDesiredGPAChanged()
        {
            DesiredErrorMessage = null;

            if (DesiredGPA == PowerPlannerSending.Grade.UNGRADED)
            {
                DesiredErrorMessage = PowerPlannerResources.GetString("ClassWhatIfPage_String_InvalidGpaErrorMessage");
                return;
            }

            double answer;

            PowerPlannerSending.GradeScale[] scale;

            if (Class.GradeScales != null && Class.GradeScales.Length > 0)
                scale = Class.GradeScales;

            else
                scale = PowerPlannerSending.GradeScale.GenerateDefaultScaleWithoutLetters();


            //find exact match
            PowerPlannerSending.GradeScale lowest = scale.Where(i => i.GPA == DesiredGPA).OrderBy(i => i.StartGrade).FirstOrDefault();

            if (lowest == null)
            {
                DesiredErrorMessage = PowerPlannerResources.GetString("ClassWhatIfPage_String_InvalidGpaErrorMessage");
                answer = 0.895;
                return;
            }



            try
            {
                double percent = lowest.StartGrade / 100;

                //if we can bring the grade down thanks to rounding (must not have decimals like 91.2%) == 0.912
                if (Class.DoesRoundGradesUp && (percent * 1000) % 10 == 0 && percent > 0.005)
                    percent -= 0.005;

                answer = percent;
            }

            catch { answer = 0.895; }

            _desiredGrade = answer;
            OnPropertyChanged(nameof(DesiredGrade));

            SetDream(answer);
        }

        private void SetDream(double grade)
        {
            Class.SetDream(grade);
        }

        private string _desiredErrorMessage;

        public string DesiredErrorMessage
        {
            get { return _desiredErrorMessage; }
            set { SetProperty(ref _desiredErrorMessage, value, nameof(DesiredErrorMessage)); }
        }


        protected override Task LoadAsyncOverride()
        {
            DataItemClass dataClass;
            DataItemWeightCategory[] dataWeightCategories;
            BaseDataItemHomeworkExamGrade[] dataGrades;

            dataClass = _originalClass.DataItem as DataItemClass;
            dataWeightCategories = _originalClass.WeightCategories.Select(i => i.DataItem as DataItemWeightCategory).ToArray();
            dataGrades = _originalClass.WeightCategories.SelectMany(i => i.Grades).Select(i => i.DataItem as BaseDataItemHomeworkExamGrade).ToArray();

            ViewItemClass classItem;

            classItem = new ViewItemClass(
                dataClass,
                createWeightMethod: CreateWeight);

            classItem.FilterAndAddChildren(dataWeightCategories);

            foreach (var weight in classItem.WeightCategories)
            {
                weight.FilterAndAddChildren(dataGrades);
            }

            Class = classItem;

            Class.CalculateEverything();
            Class.PrepareForWhatIf();
            DesiredGPA = Class.GPA;

            return Task.FromResult(true);
        }

        private static ViewItemWeightCategory CreateWeight(DataItemWeightCategory dataWeight)
        {
            return new ViewItemWeightCategory(
                dataWeight,
                createGradeMethod: ViewItemWeightCategory.CreateGradeHelper);
        }

        public void AddGrade()
        {
            try
            {
                MainScreenViewModel.ShowPopup(AddGradeViewModel.CreateForAdd(MainScreenViewModel, new AddGradeViewModel.AddParameter()
                {
                    Class = Class,
                    IsInWhatIfMode = true
                }));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ShowItem(BaseViewItemMegaItem e)
        {
            MainScreenViewModel.ShowPopup(ViewGradeViewModel.Create(MainScreenViewModel, e, isInWhatIfMode: true));
        }

        private IReadOnlyList<object> _itemsWithHeaders;
        public IReadOnlyList<object> ItemsWithHeaders
        {
            get
            {
                if (_itemsWithHeaders == null)
                {
                    // Shouldn't be null unless exception loading occurred
                    if (Class.WeightCategories == null)
                        _itemsWithHeaders = new List<object>();
                    else
                    {
                        _itemsWithHeaders = new MySublistsToFlatHeaderedList<ViewItemWeightCategory, BaseViewItemMegaItem>(Class.WeightCategories, SelectGrades, this);
                    }
                }

                return _itemsWithHeaders;
            }
        }

        private MyObservableList<BaseViewItemMegaItem> SelectGrades(ViewItemWeightCategory weightCategory)
        {
            return weightCategory.Grades;
        }
    }
}
