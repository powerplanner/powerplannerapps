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
using Vx.Views;
using PowerPlannerAppDataLibrary.Helpers;
using System.Drawing;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class.ClassGradesViewModel;
using Vx;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassWhatIfViewModel : ComponentViewModel
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
            Title = PowerPlannerResources.GetString("ClassWhatIfPage_TextBlockHeader.Text");
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
                scale = MainScreenViewModel.CurrentAccount.DefaultGradeScale;


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

        private VxState<bool> _isDescriptionExpanded = new VxState<bool>(false);

        protected override View Render()
        {
            if (!IsLoaded)
            {
                return null;
            }

            var leftMargin = Theme.Current.PageMargin + NookInsets.Left;
            var rightMargin = Theme.Current.PageMargin + NookInsets.Right;
            float floatingActionButtonOffset = VxPlatform.Current == Platform.Android ? Theme.Current.PageMargin + FloatingActionButton.DefaultSize : 0;

            return ClassGradesViewModel.WrapInFloatingActionButtonIfNeeded(new ScrollView
            {
                Content = new LinearLayout
                {
                    Children =
                    {
                        new Border
                        {
                            BackgroundColor = Class.Color.ToColor(),
                            Content = new LinearLayout
                            {
                                Margin = new Thickness(leftMargin, Theme.Current.PageMargin + NookInsets.Top, rightMargin, Theme.Current.PageMargin),
                                Children =
                                {
                                    // ========= WHAT IF? ===========
                                    new LinearLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            new Border
                                            {
                                                BackgroundColor = Color.White,
                                                Height = 4,
                                                VerticalAlignment = VerticalAlignment.Center
                                            }.LinearLayoutWeight(1),

                                            new TextBlock
                                            {
                                                Text = PowerPlannerResources.GetString("ClassWhatIfPage_TextBlockHeader.Text"),
                                                FontSize = Theme.Current.TitleFontSize,
                                                TextColor = Color.White,
                                                Margin = new Thickness(6, 0, 6, 0)
                                            },

                                            new Border
                                            {
                                                BackgroundColor = Color.White,
                                                Height = 4,
                                                VerticalAlignment = VerticalAlignment.Center
                                            }.LinearLayoutWeight(1)
                                        }
                                    },

                                    new TransparentContentButton
                                    {
                                        Content = new TextBlock
                                        {
                                            Text = PowerPlannerResources.GetString("ClassWhatIfPage_RunExplanation.Text"),
                                            TextColor = Color.White,
                                            FontSize = Theme.Current.CaptionFontSize,
                                            Height = _isDescriptionExpanded.Value ? float.NaN : 40
                                        },
                                        Click = () => _isDescriptionExpanded.Value = !_isDescriptionExpanded.Value
                                    },

                                    new TextBlock
                                    {
                                        Text = PowerPlannerResources.GetString("ClassWhatIfPage_TextBlockEnterDesired.Text"),
                                        TextColor = Color.White,
                                        Margin = new Thickness(0, 6, 0, 0)
                                    },

                                    new LinearLayout
                                    {
                                        Margin = new Thickness(0, 6, 0, 0),
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            new NumberTextBox
                                            {
                                                Number = VxValue.Create<double?>(DesiredGrade * 100, v =>
                                                {
                                                    if (v != null)
                                                    {
                                                        DesiredGrade = v.Value / 100;
                                                    }
                                                }),
                                                Margin = new Thickness(0, 0, 6, 0)
                                            }.LinearLayoutWeight(1),

                                            new NumberTextBox
                                            {
                                                Number = VxValue.Create<double?>(DesiredGPA, v =>
                                                {
                                                    if (v != null)
                                                    {
                                                        DesiredGPA = v.Value;
                                                    }
                                                }),
                                                Margin = new Thickness(6, 0, 0, 0)
                                            }.LinearLayoutWeight(1)
                                        }
                                    },

                                    DesiredErrorMessage != null ? new TextBlock
                                    {
                                        Text = DesiredErrorMessage,
                                        TextColor = Color.White,
                                        FontWeight = FontWeights.SemiBold,
                                        FontSize = Theme.Current.CaptionFontSize,
                                        Margin = new Thickness(0, 6, 0, 0)
                                    } : null
                                }
                            }
                        },

                        new GradesSummaryComponent
                        {
                            Class = Class,
                            Margin = new Thickness(leftMargin, Theme.Current.PageMargin, rightMargin, 12)
                        },

                        new AdaptiveGradesListComponent
                        {
                            Class = Class,
                            OnRequestViewGrade = g => ShowItem(g),
                            Margin = new Thickness(leftMargin, 12, rightMargin, Theme.Current.PageMargin + NookInsets.Bottom + floatingActionButtonOffset),
                            IsInWhatIfMode = true
                        }
                    }
                }
            }, AddGrade, NookInsets);
        }
    }
}
