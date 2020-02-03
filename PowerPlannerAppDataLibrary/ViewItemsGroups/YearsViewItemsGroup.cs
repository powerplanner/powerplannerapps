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
                createWeightMethod: CreateWeight);
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
            DataItemWeightCategory[] dataWeightCategories;
            DataItemMegaItem[] dataMegaItems;
            DataItemGrade[] dataGrades;

            // Need to lock the whole thing so that if something is added while we're constructing, it'll be added after we've constructed
            using (await Locks.LockDataForReadAsync("YearsViewItemsGroup.LoadBlocking"))
            {
                dataYears = dataStore.TableYears.ToArray();
                dataSemesters = dataStore.TableSemesters.ToArray();
                dataClasses = dataStore.TableClasses.ToArray();
                dataWeightCategories = dataStore.TableWeightCategories.ToArray();
                dataGrades = dataStore.TableGrades.ToArray();
                dataMegaItems = dataStore.TableMegaItems.Where(i =>
                    (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                    && i.WeightCategoryIdentifier != PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED).ToArray();

                var school = new ViewItemSchool(CreateYear);

                school.FilterAndAddChildren(dataYears);

                foreach (var year in school.Years)
                {
                    year.FilterAndAddChildren(dataSemesters);

                    foreach (var semester in year.Semesters)
                    {
                        semester.FilterAndAddChildren(dataClasses);

                        foreach (var classItem in semester.Classes)
                        {
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

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            if (School != null)
            {
                bool changed = School.HandleDataChangedEvent(e);

                if (changed)
                {
                    // Sort years here since changes in semester dates changes year sort
                    School.Years.Sort();
                    School.CalculateEverything();
                }
            }
        }
    }
}
