using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    public class YearsViewItemsGroup : BaseAccountViewItemsGroup
    {
        public ViewItemSchool School { get; private set; }

        private YearsViewItemsGroup(Guid localAccountId) : base(localAccountId)
        {
        }

        public static async Task<YearsViewItemsGroup> LoadAsync(Guid localAccountId)
        {
            YearsViewItemsGroup answer = new YearsViewItemsGroup(localAccountId);
            await answer.LoadAsync();
            return answer;
        }

        private async Task LoadAsync()
        {
            await Task.Run(LoadBlocking);
        }

        private static ViewItemYear CreateYear(DataItemYear dataYear)
        {
            return new ViewItemYear(dataYear, createSemesterMethod: CreateSemester);
        }

        private static ViewItemSemester CreateSemester(DataItemSemester dataSemester)
        {
            return new ViewItemSemester(
                dataSemester,
                createClassMethod: CreateClass);
        }

        private static ViewItemClass CreateClass(DataItemClass dataClass)
        {
            return new ViewItemClass(
                dataClass,
                createScheduleMethod: CreateSchedule,
                createWeightMethod: CreateWeight);
        }

        private static ViewItemSchedule CreateSchedule(DataItemSchedule dataSchedule)
        {
            return new ViewItemSchedule(dataSchedule);
        }

        private static ViewItemWeightCategory CreateWeight(DataItemWeightCategory dataWeight)
        {
            return new ViewItemWeightCategory(
                dataWeight,
                createGradeMethod: ViewItemWeightCategory.CreateGradeHelper);
        }

        private async Task LoadBlocking()
        {
            var dataStore = await GetDataStore();

            DataItemYear[] dataYears;
            DataItemSemester[] dataSemesters;
            DataItemClass[] dataClasses;
            DataItemSchedule[] dataSchedules; // Schedules needed to support Copy Semester
            DataItemWeightCategory[] dataWeightCategories;
            DataItemMegaItem[] dataMegaItems;
            DataItemGrade[] dataGrades;

            // Need to lock the whole thing so that if something is added while we're constructing, it'll be added after we've constructed
            using (await Locks.LockDataForReadAsync("YearsViewItemsGroup.LoadBlocking"))
            {
                dataYears = dataStore.TableYears.ToArray();
                dataSemesters = dataStore.TableSemesters.ToArray();
                dataClasses = dataStore.TableClasses.ToArray();
                dataSchedules = dataStore.TableSchedules.ToArray();
                dataWeightCategories = dataStore.TableWeightCategories.ToArray();
                dataGrades = dataStore.TableGrades.ToArray();
                dataMegaItems = dataStore.TableMegaItems.Where(i =>
                    (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                    && i.WeightCategoryIdentifier != PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED).ToArray();

                var school = School ?? new ViewItemSchool(CreateYear);

                school.FilterAndAddChildren(dataYears);

                foreach (var year in school.Years)
                {
                    year.FilterAndAddChildren(dataSemesters);

                    foreach (var semester in year.Semesters)
                    {
                        semester.FilterAndAddChildren(dataClasses);

                        foreach (var classItem in semester.Classes)
                        {
                            classItem.FilterAndAddChildren(dataSchedules);
                            classItem.FilterAndAddChildren(dataWeightCategories);

                            foreach (var weight in classItem.WeightCategories)
                            {
                                weight.FilterAndAddChildren<BaseDataItemHomeworkExamGrade>(dataGrades);
                                weight.FilterAndAddChildren<BaseDataItemHomeworkExamGrade>(dataMegaItems);
                            }
                        }
                    }
                }

                // Sort the years here, since when initially inserting they can't be sorted, since they
                // sort based on their start dates, but their start dates are calculated based on their
                // semesters, and their semesters hadn't been added when the years are inserted
                school.Years.Sort();

                this.School = school;
            }

            School.CalculateEverything();
        }

        protected override async void OnDataChangedEvent(DataChangedEvent e)
        {
            if (School != null)
            {
                // A semester or class moving is rare. The previous code wasn't written with support for this scenario, but since it's so rare, I'll just re-load everything.
                if (ContainsSemesterThatMovedYears(e) || ContainsClassThatMovedSemesters(e))
                {
                    School.Years.Clear();
                    await LoadBlocking();
                    return;
                }

                bool changed = School.HandleDataChangedEvent(e);

                if (changed)
                {
                    // Sort years here since changes in semester dates changes year sort
                    School.Years.Sort();
                    School.CalculateEverything();
                }
            }
        }

        private bool ContainsSemesterThatMovedYears(DataChangedEvent e)
        {
            foreach (var editedSemester in e.EditedItems.OfType<DataItemSemester>())
            {
                foreach (var existingYears in School.Years)
                {
                    if (existingYears.Identifier == editedSemester.UpperIdentifier
                        && !existingYears.Semesters.Any(existingSemester => existingSemester.Identifier == editedSemester.Identifier))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ContainsClassThatMovedSemesters(DataChangedEvent e)
        {
            foreach (var editedClass in e.EditedItems.OfType<DataItemClass>())
            {
                foreach (var existingSemester in School.Years.SelectMany(i => i.Semesters))
                {
                    if (existingSemester.Identifier == editedClass.UpperIdentifier
                        && !existingSemester.Classes.Any(existingClass => existingClass.Identifier == editedClass.Identifier))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
