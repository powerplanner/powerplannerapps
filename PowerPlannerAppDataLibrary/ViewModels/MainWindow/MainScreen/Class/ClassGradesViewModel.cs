using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassGradesViewModel : BaseClassContentViewModel
    {
        public static readonly string UNASSIGNED_ITEMS_HEADER = "Unassigned items";

        public ClassGradesViewModel(ClassViewModel parent) : base(parent)
        {
        }

        protected override async Task LoadAsyncOverride()
        {
            await ClassViewModel.ViewItemsGroupClass.LoadGradesTask;

            Class.WeightCategories.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(WeightCategories_CollectionChanged).Handler;
            UpdateShowWeightCategoriesSummary();

            await base.LoadAsyncOverride();
        }

        private void WeightCategories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateShowWeightCategoriesSummary();
            }
            catch { }
        }

        private void UpdateShowWeightCategoriesSummary()
        {
            if (Class.WeightCategories.Count > 1)
            {
                ShowWeightCategoriesSummary = true;
            }
            else if (Class.WeightCategories.Count == 1 && Class.WeightCategories[0].WeightValue != 100)
            {
                ShowWeightCategoriesSummary = true;
            }
            else
            {
                ShowWeightCategoriesSummary = false;
            }
        }

        public ViewItemClass Class
        {
            get { return ClassViewModel.ViewItemsGroupClass.Class; }
        }

        /// <summary>
        /// Opens the list view for editing all the class grade options.
        /// </summary>
        public void ConfigureGrades()
        {
            ClassViewModel.ConfigureGrades();
        }

        private bool _showWeightCategoriesSummary = false;
        public bool ShowWeightCategoriesSummary
        {
            get => _showWeightCategoriesSummary;
            set => SetProperty(ref _showWeightCategoriesSummary, value, nameof(ShowWeightCategoriesSummary));
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
                        _itemsWithHeaders = new MyAppendedObservableLists<object>(
                            new ListWithItemSelector(new MySublistsToFlatHeaderedList<ViewItemWeightCategory, BaseViewItemMegaItem>(Class.WeightCategories, SelectGrades, this), (item) =>
                            {
                                if (item is ViewItemTaskOrEvent)
                                {
                                    (item as ViewItemTaskOrEvent).IsUnassignedItem = false;
                                }
                                return item;
                            }),
                            new ListWithItemSelector(new UnassignedItemsHeaderList(ClassViewModel.ViewItemsGroupClass.UnassignedItems)),
                            new ListWithItemSelector(ClassViewModel.ViewItemsGroupClass.UnassignedItems, (item) =>
                            {
                                (item as ViewItemTaskOrEvent).IsUnassignedItem = true;
                                return item;
                            }));
                    }
                }

                return _itemsWithHeaders;
            }
        }

        private class UnassignedItemsHeaderList : ObservableCollection<string>
        {
            public UnassignedItemsHeaderList(MyObservableList<ViewItemTaskOrEvent> unassignedItems)
            {
                unassignedItems.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(UnassignedItems_CollectionChanged).Handler;

                Update(unassignedItems);
            }

            private void UnassignedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if ((sender as MyObservableList<ViewItemTaskOrEvent>).Count > 0)
                {
                    if (Count == 0)
                    {
                        Add(UNASSIGNED_ITEMS_HEADER);
                    }
                }

                else
                {
                    if (Count > 0)
                    {
                        RemoveAt(0);
                    }
                }
            }

            private void Update(MyObservableList<ViewItemTaskOrEvent> list)
            {
                if (list.Count > 0)
                {
                    if (Count == 0)
                    {
                        Add(UNASSIGNED_ITEMS_HEADER);
                    }
                }

                else
                {
                    if (Count > 0)
                    {
                        RemoveAt(0);
                    }
                }
            }
        }

        private MyObservableList<BaseViewItemMegaItem> SelectGrades(ViewItemWeightCategory weightCategory)
        {
            return weightCategory.Grades;
        }

        public async void Add()
        {
            await TryHandleUserInteractionAsync("Add", async (cancellationToken) =>
            {
                if (Class.WeightCategories.SelectMany(i => i.Grades).Count() >= 5 && !await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeGradesLimitReached"));
                    return;
                }

                MainScreenViewModel.ShowPopup(AddGradeViewModel.CreateForAdd(MainScreenViewModel, new AddGradeViewModel.AddParameter()
                {
                    Class = Class
                }));
            });
        }

        public void ShowItem(BaseViewItemMegaItem e)
        {
            MainScreenViewModel.ShowPopup(ViewGradeViewModel.Create(MainScreenViewModel, e));
        }

        public void ShowUnassignedItem(ViewItemTaskOrEvent item)
        {
            MainScreenViewModel.ShowPopup(ViewTaskOrEventViewModel.CreateForUnassigned(MainScreenViewModel, item));
        }

        public void OpenWhatIf()
        {
            MainScreenViewModel.Navigate(new ClassWhatIfViewModel(MainScreenViewModel, Class));
        }
    }
}
