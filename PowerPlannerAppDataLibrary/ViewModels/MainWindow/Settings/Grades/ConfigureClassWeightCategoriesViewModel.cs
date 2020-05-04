using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassWeightCategoriesViewModel : BaseMainScreenViewModelDescendant
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        public ConfigureClassWeightCategoriesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;

            WeightCategories = new MyObservableList<EditingWeightCategoryViewModel>(c.WeightCategories.Select(
                i => new EditingWeightCategoryViewModel()
                {
                    Name = i.Name,
                    Weight = i.WeightValue,
                    Identifier = i.Identifier
                }));
        }

        public MyObservableList<EditingWeightCategoryViewModel> WeightCategories { get; private set; }

        public void AddWeightCategory()
        {
            WeightCategories.Add(new EditingWeightCategoryViewModel());
        }

        public void RemoveWeightCategory(EditingWeightCategoryViewModel weightCategory)
        {
            WeightCategories.Remove(weightCategory);
        }

        /// <summary>
        /// Checks that weight percents are non-negative, and that they have names. Returns false if any were negative.
        /// </summary>
        /// <returns></returns>
        private bool AreWeightsValid()
        {
            foreach (EditingWeightCategoryViewModel w in WeightCategories)
                if (!w.IsValid)
                    return false;

            return true;
        }

        private bool HasMadeChanges()
        {
            if (Class.WeightCategories.Count == 0)
            {
                return true;
            }

            if (WeightCategories.Count != Class.WeightCategories.Count)
            {
                return true;
            }

            for (int i = 0; i < WeightCategories.Count; i++)
            {
                if (!WeightCategories[i].Name.Equals(Class.WeightCategories[i].Name))
                {
                    return true;
                }

                if (WeightCategories[i].Weight != Class.WeightCategories[i].WeightValue)
                {
                    return true;
                }
            }

            return false;
        }

        public async void Save()
        {
            try
            {
                if (!AreWeightsValid())
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("String_InvalidWeightCategoriesMessageBody"), PowerPlannerResources.GetString("String_InvalidWeightCategoriesMessageHeader")).ShowAsync();
                    return;
                }

                DataChanges changes = new DataChanges();

                // Process weight category changes
                List<EditingWeightCategoryViewModel> newWeights = new List<EditingWeightCategoryViewModel>(WeightCategories);

                List<BaseViewItemHomeworkExamGrade> lostGrades = new List<BaseViewItemHomeworkExamGrade>();

                //handle weights that were deleted
                foreach (ViewItemWeightCategory existing in Class.WeightCategories)
                {
                    //if that existing weight isn't in the new weights list
                    if (!newWeights.Any(i => i.Identifier == existing.Identifier))
                    {
                        //mark it for deletion
                        changes.DeleteItem(existing.Identifier);

                        //add all of its grades to the lost grades
                        lostGrades.AddRange(existing.Grades);
                    }
                }

                //if there aren't any weights, need to add the default All Grades
                if (newWeights.Count == 0)
                {
                    newWeights.Add(new EditingWeightCategoryViewModel()
                    {
                        Name = PowerPlannerResources.GetString("WeightCategory_AllGrades"),
                        Weight = 100
                    });
                }

                Guid firstCategory = newWeights.First().Identifier;

                //strip away any existing weight categories that didn't change
                foreach (EditingWeightCategoryViewModel newWeight in newWeights.ToArray())
                {
                    ViewItemWeightCategory existing = Class.WeightCategories.FirstOrDefault(i => i.Identifier == newWeight.Identifier);

                    if (existing != null)
                    {
                        //if the name and/or value didn't change, we'll remove it from the main list
                        if (existing.Name.Equals(newWeight.Name) && existing.WeightValue == newWeight.Weight)
                            newWeights.Remove(newWeight);
                    }
                }

                //and now process the new/changed weights
                foreach (EditingWeightCategoryViewModel changed in newWeights)
                {
                    DataItemWeightCategory w;

                    ViewItemWeightCategory existing = Class.WeightCategories.FirstOrDefault(i => i.Identifier == changed.Identifier);

                    //if existing, serialize
                    if (existing != null)
                        w = new DataItemWeightCategory() { Identifier = existing.Identifier };

                    else
                    {
                        w = new DataItemWeightCategory()
                        {
                            Identifier = Guid.NewGuid(),
                            UpperIdentifier = Class.Identifier
                        };

                        //if we didn't have a first category yet
                        if (firstCategory == Guid.Empty)
                            firstCategory = w.Identifier;
                    }

                    w.Name = changed.Name;
                    w.WeightValue = changed.Weight;

                    changes.Add(w);
                }

                // And then move the lost grades into the first available weight category
                foreach (var lostGrade in lostGrades.OfType<ViewItemGrade>())
                {
                    DataItemGrade g = new DataItemGrade()
                    {
                        Identifier = lostGrade.Identifier
                    };

                    g.UpperIdentifier = firstCategory;

                    changes.Add(g);
                }
                foreach (var lostGrade in lostGrades.OfType<ViewItemHomework>())
                {
                    DataItemMegaItem g = new DataItemMegaItem()
                    {
                        Identifier = lostGrade.Identifier
                    };

                    g.WeightCategoryIdentifier = firstCategory;

                    changes.Add(g);
                }
                foreach (var lostGrade in lostGrades.OfType<ViewItemExam>())
                {
                    DataItemMegaItem g = new DataItemMegaItem()
                    {
                        Identifier = lostGrade.Identifier
                    };

                    g.WeightCategoryIdentifier = firstCategory;

                    changes.Add(g);
                }

                TryStartDataOperationAndThenNavigate(delegate
                {
                    return PowerPlannerApp.Current.SaveChanges(changes);

                }, delegate
                {
                    this.RemoveViewModel();
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }
        }
    }
}
