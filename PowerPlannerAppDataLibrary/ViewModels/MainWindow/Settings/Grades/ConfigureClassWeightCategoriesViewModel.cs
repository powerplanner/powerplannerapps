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
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassWeightCategoriesViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        public ConfigureClassWeightCategoriesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Title = PowerPlannerResources.GetString("ConfigureClassGrades_Items_WeightCategories.Title");
            UseCancelForBack();
            PrimaryCommand = PopupCommand.Save(Save);

            WeightCategories = new MyObservableList<EditingWeightCategoryViewModel>(c.WeightCategories.Select(
                i => new EditingWeightCategoryViewModel()
                {
                    Name = i.Name,
                    Weight = i.WeightValue,
                    Identifier = i.Identifier
                }));
        }

        public MyObservableList<EditingWeightCategoryViewModel> WeightCategories { get; private set; }

        protected override View Render()
        {
            var views = new List<View>()
            {
                RenderRow(new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ClassPage_EditGrades_TextBlockName.Text"),
                    FontWeight = FontWeights.Bold,
                    WrapText = false
                }, new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ClassPage_EditGrades_TextBlockWeight.Text"),
                    FontWeight = FontWeights.Bold,
                    WrapText = false
                }, new TransparentContentButton
                {
                    Opacity = 0,
                    Content = new FontIcon
                    {
                        Glyph = MaterialDesign.MaterialDesignIcons.Close,
                        FontSize = 20
                    }
                })
            };

            foreach (var entry in WeightCategories)
            {
                views.Add(RenderRow(new TextBox
                {
                    Text = VxValue.Create(entry.Name, v =>
                    {
                        entry.Name = v;
                        MarkDirty();
                    })
                }, new NumberTextBox
                {
                    Number = VxValue.Create(entry.Weight, v =>
                    {
                        entry.Weight = v;
                        MarkDirty();
                    })
                }, new TransparentContentButton
                {
                    Content = new FontIcon
                    {
                        Glyph = MaterialDesign.MaterialDesignIcons.Close,
                        FontSize = 20,
                        Color = System.Drawing.Color.Red
                    },
                    Click = () => { RemoveWeightCategory(entry); }
                }));
            }

            views.Add(new Button
            {
                Text = PowerPlannerResources.GetString("ClassPage_ButtonAddWeightCategory.Content"),
                Margin = new Thickness(0, 12, 0, 0),
                Click = () => { AddWeightCategory(); }
            });

            return RenderGenericPopupContent(views);
        }

        private static View RenderRow(View first, View second, View third)
        {
            first.Margin = new Thickness(0, 0, 6, 0);
            second.Margin = new Thickness(6, 0, 0, 0);

            var layout = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    first.LinearLayoutWeight(1),
                    second.LinearLayoutWeight(1)
                },
                Margin = new Thickness(0, 0, 0, 6)
            };

            if (third != null)
            {
                third.Margin = new Thickness(12, 0, 0, 0);
                layout.Children.Add(third);
            }

            return layout;
        }

        public void AddWeightCategory()
        {
            WeightCategories.Add(new EditingWeightCategoryViewModel());
            MarkDirty();
        }

        public void RemoveWeightCategory(EditingWeightCategoryViewModel weightCategory)
        {
            WeightCategories.Remove(weightCategory);
            MarkDirty();
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

                List<BaseViewItemMegaItem> lostGrades = new List<BaseViewItemMegaItem>();

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
                    w.WeightValue = changed.Weight.Value;

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
                foreach (var lostGrade in lostGrades.OfType<ViewItemTaskOrEvent>())
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
