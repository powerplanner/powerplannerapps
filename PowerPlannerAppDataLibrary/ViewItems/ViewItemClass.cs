using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Converters;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemClass : BaseViewItemWithImages, IGPACredits, IComparable<ViewItemClass>
    {
        public bool IsNoClassClass { get; private set; }

        public new DataItemClass DataItem => base.DataItem as DataItemClass;

        private ViewItemSemester _semester;
        public ViewItemSemester Semester
        {
            get { return _semester; }
            set
            {
                SetProperty(ref _semester, value, "Semester");
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public ViewItemClass(
            DataItemClass dataItem,
            Func<DataItemMegaItem, ViewItemTaskOrEvent> createTaskOrEventMethod = null,
            Func<DataItemSchedule, ViewItemSchedule> createScheduleMethod = null,
            Func<DataItemWeightCategory, ViewItemWeightCategory> createWeightMethod = null,
            Func<DataItemMegaItem, bool> isTaskOrEventChildMethod = null) : base(dataItem)
        {
            if (dataItem.Identifier == dataItem.UpperIdentifier)
            {
                IsNoClassClass = true;
            }

            if (createTaskOrEventMethod != null)
            {
                AddTasksAndEventsChildrenHelper(createTaskOrEventMethod, isTaskOrEventChildMethod);
            }

            if (createScheduleMethod != null)
            {
                Schedules = new MyObservableList<ViewItemSchedule>();

                AddChildrenHelper(new ViewItemChildrenHelper<DataItemSchedule, ViewItemSchedule>(
                    isChild: IsChild,
                    addMethod: Add,
                    removeMethod: Remove,
                    createChildMethod: createScheduleMethod,
                    children: Schedules));
            }

            if (createWeightMethod != null)
            {
                AddWeightChildrenHelper(createWeightMethod);
            }
        }

        public void AddWeightChildrenHelper(Func<DataItemWeightCategory, ViewItemWeightCategory> createWeightMethod)
        {
            WeightCategories = new MyObservableList<ViewItemWeightCategory>();

            AddChildrenHelper(new ViewItemChildrenHelper<DataItemWeightCategory, ViewItemWeightCategory>(
                isChild: IsChild,
                addMethod: Add,
                removeMethod: Remove,
                createChildMethod: createWeightMethod,
                children: WeightCategories));
        }

        public void AddTasksAndEventsChildrenHelper(Func<DataItemMegaItem, ViewItemTaskOrEvent> createItemMethod, Func<DataItemMegaItem, bool> isChildMethod = null)
        {
            TasksAndEvents = new MyObservableList<ViewItemTaskOrEvent>();

            AddChildrenHelper(new ViewItemChildrenHelper<DataItemMegaItem, ViewItemTaskOrEvent>(
                isChild: isChildMethod != null ? isChildMethod : IsTaskOrEventChild,
                addMethod: Add,
                removeMethod: Remove,
                createChildMethod: createItemMethod,
                children: TasksAndEvents));
        }

        private bool _hasGrades = false;
        public bool HasGrades
        {
            get => _hasGrades;
            private set => SetProperty(ref _hasGrades, value, nameof(HasGrades));
        }

        private double _calculatedGrade = PowerPlannerSending.Grade.UNGRADED;
        public double CalculatedGrade
        {
            get { return _calculatedGrade; }
            private set { SetProperty(ref _calculatedGrade, value, nameof(CalculatedGrade)); }
        }

        public double Grade => CachedComputation(delegate
        {
            if (OverriddenGrade != PowerPlannerSending.Grade.UNGRADED)
                return OverriddenGrade;

            return CalculatedGrade;
        }, GradeDependencies);
        private static readonly string[] GradeDependencies = new string[] { nameof(OverriddenGrade), nameof(CalculatedGrade) };

        private double _overriddenGrade = PowerPlannerSending.Grade.UNGRADED;
        public double OverriddenGrade
        {
            get { return _overriddenGrade; }
            protected set { SetProperty(ref _overriddenGrade, value, nameof(OverriddenGrade)); }
        }

        private double _calculatedGPA = PowerPlannerSending.Grade.UNGRADED;
        public double CalculatedGPA
        {
            get { return _calculatedGPA; }
            protected set { SetProperty(ref _calculatedGPA, value, "CalculatedGPA", "GPA"); }
        }

        private double m_gpa = PowerPlannerSending.Grade.UNGRADED;
        public double GPA => CachedComputation(delegate
        {
            if (OverriddenGPA != PowerPlannerSending.Grade.UNGRADED)
                return OverriddenGPA;

            return CalculatedGPA;
        }, GPADependencies);
        private static readonly string[] GPADependencies = new string[] { nameof(OverriddenGPA), nameof(CalculatedGPA) };

        private double _overriddenGPA = PowerPlannerSending.Grade.UNGRADED;
        public double OverriddenGPA
        {
            get { return _overriddenGPA; }
            protected set { SetProperty(ref _overriddenGPA, value, nameof(OverriddenGPA)); }
        }

        public bool IsPassing => CachedComputation(delegate
        {
            double roundedGrade = this.Grade;
            if (this.DoesRoundGradesUp)
            {
                roundedGrade = Math.Round(roundedGrade * 100, MidpointRounding.AwayFromZero) / 100;
            }

            return roundedGrade >= this.PassingGrade;
        }, IsPassingDependentOn);
        private static readonly string[] IsPassingDependentOn = new string[] { nameof(Grade), nameof(DoesRoundGradesUp), nameof(PassingGrade) };

        private bool _needsRecalc = true;

        private bool IsChild(DataItemWeightCategory dataChild)
        {
            return dataChild.UpperIdentifier == Identifier;
        }

        private bool IsTaskOrEventChild(DataItemMegaItem dataChild)
        {
            return (dataChild.MegaItemType == MegaItemType.Homework || dataChild.MegaItemType == MegaItemType.Homework)
                && dataChild.UpperIdentifier == Identifier;
        }

        private bool IsChild(DataItemSchedule dataSchedule)
        {
            return dataSchedule.UpperIdentifier == Identifier;
        }

        private string _courseNumber;
        public string CourseNumber
        {
            get { return _courseNumber; }
            private set { SetProperty(ref _courseNumber, value, "CourseNumber"); }
        }

        private bool _shouldAverageGradeTotals;
        public bool ShouldAverageGradeTotals
        {
            get { return _shouldAverageGradeTotals; }
            private set { SetProperty(ref _shouldAverageGradeTotals, value, "ShouldAverageGradeTotals"); }
        }

        private bool _doesRoundGradesUp;
        public bool DoesRoundGradesUp
        {
            get { return _doesRoundGradesUp; }
            private set { SetProperty(ref _doesRoundGradesUp, value, "DoesRoundGradesUp"); }
        }

        private byte[] _color;
        public byte[] Color
        {
            get { return _color; }
            private set { SetProperty(ref _color, value, "Color"); }
        }

        private byte _position;
        public byte Position
        {
            get { return _position; }
            private set { SetProperty(ref _position, value, "Position"); }
        }

        private double _credits;
        public double Credits
        {
            get { return _credits; }
            private set { SetProperty(ref _credits, value, "Credits"); }
        }

        private GradeScale[] _gradeScales;
        public GradeScale[] GradeScales
        {
            get { return _gradeScales; }
            private set { SetProperty(ref _gradeScales, value, "GradeScales"); }
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            private set { SetProperty(ref _startDate, value, nameof(StartDate)); }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get { return _endDate; }
            private set { SetProperty(ref _endDate, value, nameof(EndDate)); }
        }

        private GpaType _gpaType;
        public GpaType GpaType
        {
            get { return _gpaType; }
            private set { SetProperty(ref _gpaType, value, nameof(GpaType)); }
        }

        private double _passingGrade;
        public double PassingGrade
        {
            get { return _passingGrade; }
            private set { SetProperty(ref _passingGrade, value, nameof(PassingGrade)); }
        }

        /// <summary>
        /// This is used on class pages, where we DO want "GPA" text included
        /// </summary>
        public string GpaString => CachedComputation(delegate
        {
            if (GpaType == GpaType.PassFail)
            {
                if (IsPassing)
                {
                    return PowerPlannerResources.GetString("String_Passing");
                }
                else
                {
                    return PowerPlannerResources.GetString("String_Failing");
                }
            }
            else
            {
                return string.Format(PowerPlannerResources.GetString("String_GPA"), GPA.ToString("0.0##"));
            }
        }, GpaStringDependentOn);
        private static string[] GpaStringDependentOn = new string[] { nameof(GpaType), nameof(IsPassing), nameof(GPA) };

        /// <summary>
        /// This is used on the Years page, where we don't want "GPA" text included.
        /// </summary>
        public string GpaStringForTableDisplay => CachedComputation(delegate
        {
            // If there aren't any grades entered in the class, we ignore it.
            // This is for the case where student enters their semester ahead of time,
            // that semester shouldn't artificially raise their GPA
            if (!HasGrades)
            {
                return "--";
            }

            if (GpaType == GpaType.PassFail)
            {
                if (IsPassing)
                {
                    return PowerPlannerResources.GetString("String_Pass").ToLower();
                }
                else
                {
                    return PowerPlannerResources.GetString("String_Fail").ToLower();
                }
            }
            else
            {
                return GPA.ToString("0.0##");
            }
        }, GpaStringDependentOn.Concat(new string[] { nameof(HasGrades) }).ToArray());

        public bool DidEarnCredits => CachedComputation(delegate
        {
            switch (GpaType)
            {
                case GpaType.PassFail:
                    return IsPassing;

                default:
                    return GPA > 0;
            }
        }, EarnedCreditsDependentOn);
        private static readonly string[] EarnedCreditsDependentOn = new string[] { nameof(GpaType), nameof(IsPassing), nameof(GPA) };

        public string CreditsStringForYearsPage => CachedComputation(delegate
        {
            // If the class doesn't have credits, doesn't matter whether passed/failed
            // Or if we earned the credits, we display them normally too
            if (Credits == -1 || Credits == 0 || DidEarnCredits)
            {
                return CreditsToStringConverter.Convert(Credits);
            }

            // Otherwise, we didn't earn credits, need to show how many we didn't earn
            return "0/" + CreditsToStringConverter.Convert(Credits);
        }, CreditsStringForYearsPageDependentOn);
        private static readonly string[] CreditsStringForYearsPageDependentOn = new string[] { nameof(Credits), nameof(DidEarnCredits) };

        /// <summary>
        /// NOT data-bindable, only updates when calling get.
        /// </summary>
        public double CreditsEarned
        {
            get
            {
                if (Credits == -1)
                {
                    return -1;
                }

                return DidEarnCredits ? Credits : 0;
            }
        }

        /// <summary>
        /// NOT data-bindable, only updates when calling get.
        /// </summary>
        public double CreditsAffectingGpa
        {
            get
            {
                // We still have to return -1 if credits is unassigned since that affects
                // how we aggregate everything, and still need to know it was unassigned, even
                // if it was pass/fail or if it doesn't have grades
                if (Credits == -1)
                {
                    return -1;
                }

                return (GpaType == GpaType.PassFail || !HasGrades) ? 0 : Credits;
            }
        }

        /// <summary>
        /// Returns true if the class is active on the specified date, based on the class Start/End dates. Does not factor in whether the semester is actually active on these dates.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsActiveOnDate(DateTime date)
        {
            // If class hasn't started yet
            if (!DateValues.IsUnassigned(StartDate) && StartDate > date.Date)
            {
                return false;
            }

            // If class is already over
            if (!DateValues.IsUnassigned(EndDate) && date.Date > EndDate)
            {
                return false;
            }

            return true;
        }

        public MyObservableList<ViewItemWeightCategory> WeightCategories { get; private set; }

        internal void Add(ViewItemTaskOrEvent viewItemTaskOrEvent)
        {
            viewItemTaskOrEvent.Class = this;

            if (TasksAndEvents != null)
            {
                TasksAndEvents.InsertSorted(viewItemTaskOrEvent);
            }
        }

        internal void Remove(ViewItemTaskOrEvent viewItemTaskOrEvent)
        {
            TasksAndEvents?.Remove(viewItemTaskOrEvent);
        }

        internal void Add(ViewItemWeightCategory viewItemWeightCategory)
        {
            viewItemWeightCategory.Class = this;

            if (WeightCategories != null)
                WeightCategories.InsertSorted(viewItemWeightCategory);

            _needsRecalc = true;
        }

        internal void Remove(ViewItemWeightCategory viewItemWeightCategory)
        {
            if (WeightCategories != null)
                WeightCategories.Remove(viewItemWeightCategory);

            _needsRecalc = true;
        }

        /// <summary>
        /// List is already sorted
        /// </summary>
        public MyObservableList<ViewItemTaskOrEvent> TasksAndEvents { get; private set; }

        public MyObservableList<ViewItemSchedule> Schedules { get; private set; }

        private void Add(ViewItemSchedule schedule)
        {
            schedule.Class = this;

            if (Schedules != null)
                Schedules.InsertSorted(schedule);
        }

        private void Remove(ViewItemSchedule schedule)
        {
            if (Schedules != null)
                Schedules.Remove(schedule);
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemClass i = dataItem as DataItemClass;

            CourseNumber = i.CourseNumber;
            ShouldAverageGradeTotals = i.ShouldAverageGradeTotals;
            DoesRoundGradesUp = i.DoesRoundGradesUp;
            Color = i.RawColor;
            Position = i.Position;
            GradeScales = i.GetGradeScales() ?? GradeScale.GenerateDefaultScaleWithoutLetters();
            Credits = i.Credits;
            StartDate = DateTime.SpecifyKind(i.StartDate, DateTimeKind.Local);
            EndDate = DateTime.SpecifyKind(i.EndDate, DateTimeKind.Local);
            GpaType = i.GpaType;
            PassingGrade = i.PassingGrade;

            _needsRecalc = true;
        }

        public void PrepareForWhatIf()
        {
            foreach (var weight in WeightCategories)
                weight.PrepareForWhatIf();
        }

        /// <summary>
        /// Calculates all the weights and then the class
        /// </summary>
        public bool CalculateEverything()
        {
            if (WeightCategories == null)
            {
                return false;
            }

            bool changed = _needsRecalc;

            for (int i = 0; i < WeightCategories.Count; i++)
            {
                if (WeightCategories[i].Calculate())
                    changed = true;
            }

            if (changed)
                Calculate();

            _needsRecalc = false;

            return changed;
        }

        private void Calculate()
        {
            double totalGrade = 0, totalWeight = 0;

            // GRADE CALCULATION:
            //
            // WEIGHT VALUE  * PERCENT        = FINAL PERCENT (totalGrade is the sum of these percents)
            // 15            * 97%            = 14.55%
            // 30            * 90%            = 27%
            // 55            * 93%            = 51.15%
            // -----------------------------------------------
            // GRADE = (14.55% + 27% + 51.15%) / WEIGHT VALUE TOTAL = 92.7%
            //
            //
            // ALTERNATIVELY:
            //
            // GRADE = SUM(WEIGHT PERCENT * WEIGHT AMOUNT) / WEIGHT AMOUNT TOTAL
            //       = totalGrade                          / totalWeight;

            foreach (ViewItemWeightCategory w in WeightCategories)
            {
                if (this.ShouldAverageGradeTotals)
                {
                    if (w.WeightAchievedAveraged != PowerPlannerSending.Grade.UNGRADED)
                    {
                        totalGrade += w.WeightAchievedAveraged;
                        totalWeight += w.WeightValue;
                    }
                }

                else
                {
                    if (w.WeightAchievedSummed != PowerPlannerSending.Grade.UNGRADED)
                    {
                        totalGrade += w.WeightAchievedSummed;
                        totalWeight += w.WeightValue;
                    }
                }
            }

            //if there was actually a grade
            if (totalWeight != 0)
            {
                this.CalculatedGrade = totalGrade / totalWeight;
                this.HasGrades = true;
            }
            else
            {
                this.CalculatedGrade = 1;
                this.HasGrades = false;
            }

            this.CalculatedGPA = GetGPAForGrade(CalculatedGrade);
        }

        public double GetGPAForGrade(double grade)
        {
            GradeScale[] gradeScales;

            //if they have a custom grade scale
            if (this.GradeScales != null && this.GradeScales.Length != 0)
            {
                gradeScales = this.GradeScales;
            }


            //use default grade scale
            else
            {
                gradeScales = GradeScale.GenerateDefaultScaleWithoutLetters();
            }



            //if the grade percents are rounded up
            if (this.DoesRoundGradesUp)
            {
                int roundedGrade = (int)MyMath.Round(grade * 100);

                for (int i = 0; i < gradeScales.Length; i++)
                {
                    if (roundedGrade >= (gradeScales[i]).StartGrade)
                    {
                        return (gradeScales[i]).GPA;
                    }
                }
            }

            else
            {
                for (int i = 0; i < gradeScales.Length; i++)
                {
                    if (grade * 100 >= (gradeScales[i]).StartGrade)
                    {
                        return (gradeScales[i]).GPA;
                    }
                }
            }
            
            return gradeScales.Last().GPA;
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemClass)
                return CompareTo(other as ViewItemClass);

            return base.CompareTo(other);
        }

        public int CompareTo(ViewItemClass other)
        {
            int comp = Name.CompareTo(other.Name);

            if (comp == 0)
                return DateCreated.CompareTo(other.DateCreated);

            return comp;
        }


        #region WhatIf



        //sorted by biggest importance first
        private class SortedGradeItem : IComparable<SortedGradeItem>, IComparer<SortedGradeItem>
        {
            public BaseViewItemMegaItem Grade;
            public double Importance;

            public int Compare(SortedGradeItem x, SortedGradeItem y)
            {
                if (x.Importance > y.Importance)
                    return -1;
                else if (x.Importance == y.Importance)
                    return 0;
                return 1;
            }

            public int CompareTo(SortedGradeItem other)
            {
                if (Importance > other.Importance)
                    return -1;
                else if (Importance == other.Importance)
                    return 0;
                return 1;
            }
        }

        private double? _currDesiredGrade;

        public void ResetDream()
        {
            if (_currDesiredGrade == null)
                CalculateEverything();
            else
                SetDream(_currDesiredGrade.Value);
        }

        /// <summary>
        /// Fills in the empty grades. When it returns, everything has already been calculated.
        /// </summary>
        /// <param name="desiredGrade"></param>
        public void SetDream(double desiredGrade)
        {
            _currDesiredGrade = desiredGrade;
            List<List<int>> dreamGrades = new List<List<int>>();
            int dreamGradesCount = 0;
            double percentNeeded = 0;
            double numerator = 0, denominator = 0;
            bool isGradesActive = false;
            bool isAverage = ShouldAverageGradeTotals;

            foreach (ViewItemWeightCategory weighted in WeightCategories)
            {
                // "pointsInWeight" - The current graded points in the weight.
                // "totalPointsInWeight" - The current possible points from graded items in the weight.
                // "totalExtraPoints" - The current possible points from ungraded items in the weight.

                // We calculate all of the three above items....

                isGradesActive = false;
                double pointsInWeight = 0, totalPointsInWeight = 0, totalExtraPoints = 0;
                dreamGrades.Add(new List<int>());

                int x = 0;
                foreach (var g in weighted.Grades)
                {
                    if (!g.IsDropped)
                    {
                        isGradesActive = true;

                        // If it's a normal grade (that has a grade value)
                        if (!g.CanBeUsedForAchievingDesiredGrade)
                        {
                            if (isAverage)
                            {
                                pointsInWeight += g.GradePercent;
                                totalPointsInWeight += 1;
                            }

                            else
                            {
                                pointsInWeight += g.GradeReceived;
                                totalPointsInWeight += g.GradeTotal;
                            }
                        }

                        // If it's ungraded (and thus can be used for what if)
                        else
                        {
                            dreamGrades.Last().Add(x);
                            dreamGradesCount++;

                            if (isAverage)
                                totalExtraPoints += 1;

                            else
                                totalExtraPoints += g.GradeTotal;
                        }
                    }

                    x++;
                }


                // Now as long as this weight had a grade...
                if (isGradesActive)
                {
                    //desired grade times weight total
                    numerator += (desiredGrade) * weighted.WeightValue;

                    double numeratorWeightedPoints;
                    double denominatorWeightedPoints;

                    if (weighted.WeightValue == 0)
                    {
                        // Extra credit weight category scenario
                        numeratorWeightedPoints = pointsInWeight;
                        denominatorWeightedPoints = totalExtraPoints;
                    }
                    else
                    {
                        numeratorWeightedPoints = pointsInWeight * weighted.WeightValue;
                        denominatorWeightedPoints = totalExtraPoints * weighted.WeightValue;
                    }

                    //grade in weight category times weight percent
                    if (totalPointsInWeight != 0 || totalExtraPoints != 0)
                    {
                        numerator -= numeratorWeightedPoints / (totalPointsInWeight + totalExtraPoints);
                        denominator += denominatorWeightedPoints / (totalPointsInWeight + totalExtraPoints);
                    }
                    else
                    {
                        // Scenario where all items are extra credit items (grade total of 0)
                        numerator -= numeratorWeightedPoints;
                        denominator += denominatorWeightedPoints;
                    }
                }
            }

            percentNeeded = numerator / denominator;
            if (percentNeeded < 0)
            {
                percentNeeded = 0;
            }



            if (desiredGrade != -1)
            {
                List<SortedGradeItem> sortedGrades = new List<SortedGradeItem>();

                //add all the grades to the list, while also setting base grades
                for (int i = 0; i < dreamGrades.Count; i++)
                    for (int x = 0; x < dreamGrades[i].Count; x++)
                    {
                        sortedGrades.Add(new SortedGradeItem() // Importance = Weight Percent * Grade Total
                        {
                            Grade = WeightCategories[i].Grades[dreamGrades[i][x]],
                            Importance = WeightCategories[i].WeightValue * WeightCategories[i].Grades[dreamGrades[i][x]].GradeTotal
                        });

                        //also set base grade
                        setBaseGrade(WeightCategories[i].Grades[dreamGrades[i][x]], percentNeeded);
                    }

                //if it is magically right, stop here!
                CalculateEverything();
                if (CalculatedGrade >= desiredGrade)
                    return;

                //sort the list, most important grades (ones that have biggest impact on overall grade) are first
                sortedGrades.Sort();

                //go through list, most important first, trying to increment by 1's
                for (int i = 0; i < sortedGrades.Count; i++)
                {
                    //tries to raise grade by one as long as it doesn't make class grade greater than desired grade
                    tryIncrementGrade(sortedGrades[i].Grade, desiredGrade);

                    //if it's perfectly right, stop here!
                    if (CalculatedGrade == desiredGrade)
                        return;
                }

                //couldn't get grade perfect, so increment least important first until grade is greater than desired grade
                for (int i = sortedGrades.Count - 1; i >= 0; i--)
                {
                    //if class grade is now higher than or equal to desired grade, we're done!
                    if (incrementGrade(sortedGrades[i].Grade, desiredGrade))
                        return;
                }
            }
        }

        /// <summary>
        /// Sets the grade to the lower option, using Math.Floor.
        /// </summary>
        /// <param name="grade"></param>
        /// <param name="percentNeeded"></param>
        private static void setBaseGrade(BaseViewItemMegaItem grade, double percentNeeded)
        {
            //set grade to the lower option
            grade.GradeReceived = Math.Floor(percentNeeded * grade.GradeTotal);
            grade.WasChanged = true;
            grade.WeightCategory.NeedsRecalc = true;
        }

        /// <summary>
        /// Increments the grade received by 1 (calculates class grade too)
        /// </summary>
        /// <param name="grade"></param>
        /// <returns>Returns true if the class grade is now greater than or equal to the desired grade, else false.</returns>
        private bool incrementGrade(BaseViewItemMegaItem grade, double desiredGrade)
        {
            grade.GradeReceived++;
            grade.WasChanged = true;
            grade.WeightCategory.NeedsRecalc = true;

            CalculateEverything();
            if (CalculatedGrade >= desiredGrade)
                return true;

            return false;
        }

        /// <summary>
        /// Will increase grade received by 1 if it doesn't make the class' grade higher than the desired grade (also calculates grade)
        /// </summary>
        /// <param name="grade"></param>
        private void tryIncrementGrade(BaseViewItemMegaItem grade, double desiredGrade)
        {
            grade.GradeReceived++;
            grade.WeightCategory.NeedsRecalc = true;

            CalculateEverything();
            if (CalculatedGrade > desiredGrade) //too high
            {
                grade.GradeReceived--;
                grade.WeightCategory.NeedsRecalc = true;
                CalculateEverything();
            }

            else
                grade.WasChanged = true;
        }

        #endregion

        public List<ViewItemSchedule[]> GetSchedulesGroupedBySharedEditingValues()
        {
            List<ViewItemSchedule[]> answer = new List<ViewItemSchedule[]>();

            List<ViewItemSchedule> copiedSchedules = Schedules.ToList();

            while (copiedSchedules.Count > 0)
            {
                ViewItemSchedule s = copiedSchedules[0];

                List<ViewItemSchedule> group = new List<ViewItemSchedule>()
                {
                    s
                };

                copiedSchedules.RemoveAt(0);

                for (int i = 0; i < copiedSchedules.Count; i++)
                {
                    if (ScheduleCreator.SharesSameEditingValues(s, copiedSchedules[i]))
                    {
                        // Make sure we don't add duplicates
                        if (!group.Exists(gItem => gItem.Identifier == copiedSchedules[i].Identifier))
                        {
                            group.Add(copiedSchedules[i]);
                        }

                        // Remove regardless if there was a duplicate for some reason
                        copiedSchedules.RemoveAt(i);
                        i--;
                    }
                }

                answer.Add(group.ToArray());
            }

            return answer;
        }

        public ViewItemSchedule[] GetGroupOfSchedulesWithSharedEditingValues(ViewItemSchedule scheduleToGroupFrom)
        {
            var allGroups = GetSchedulesGroupedBySharedEditingValues();

            return allGroups.FirstOrDefault(i => i.Contains(scheduleToGroupFrom));
        }
    }
}
