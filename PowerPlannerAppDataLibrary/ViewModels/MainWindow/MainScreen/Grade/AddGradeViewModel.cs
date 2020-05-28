using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade
{
    public class AddGradeViewModel : BaseMainScreenViewModelChild
    {
        /// <summary>
        /// View should set this if it enables editing IsDropped from here
        /// </summary>
        public bool UsesIsDropped { get; set; }

        protected override bool InitialAllowLightDismissValue => false;

        public AddGradeViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public enum OperationState { Adding, Editing, AddingWhatIf, EditingWhatIf }

        public OperationState State { get; private set; }

        public override string GetPageName()
        {
            if (State == OperationState.Adding)
            {
                return "AddGradeView";
            }
            else
            {
                return "EditGradeView";
            }
        }

        public bool IsUnassignedItem { get; private set; }

        public class AddParameter
        {
            public ViewItemClass Class { get; set; }

            public bool IsInWhatIfMode { get; set; }
        }

        public class EditParameter
        {
            public BaseViewItemMegaItem Item { get; set; }

            public Action OnSaved { get; set; }

            public bool IsInWhatIfMode { get; set; }

            public bool IsUnassignedItem { get; set; }
        }

        public static AddGradeViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            DateTime? date = NavigationManager.GetPreviousAddItemDate();
            if (date == null)
            {
                date = DateTime.Today;
            }

            ViewItemWeightCategory weight = addParams.Class.WeightCategories.FirstOrDefault(i => i.Identifier == NavigationManager.SelectedWeightCategoryIdentifier);
            if (weight == null)
            {
                weight = addParams.Class.WeightCategories.First();
            }

            return new AddGradeViewModel(parent)
            {
                State = addParams.IsInWhatIfMode ? OperationState.AddingWhatIf : OperationState.Adding,
                Date = date.Value,
                WeightCategories = addParams.Class.WeightCategories,
                SelectedWeightCategory = weight
            };
        }

        public static AddGradeViewModel CreateForEdit(BaseViewModel parent, EditParameter editParams)
        {
            ViewItemWeightCategory weight = editParams.Item.WeightCategory;

            ViewItemClass c;
            if (editParams.Item is ViewItemTaskOrEvent item)
            {
                c = item.Class;
            }
            else
            {
                if (weight == null)
                {
                    throw new NullReferenceException("WeightCategory was null");
                }
                c = weight.Class;
            }
            if (c == null)
            {
                throw new NullReferenceException("Class was null");
            }

            if (weight == null || weight == ViewItemWeightCategory.UNASSIGNED || weight == ViewItemWeightCategory.EXCLUDED)
            {
                weight = c.WeightCategories.FirstOrDefault(i => i.Identifier == NavigationManager.SelectedWeightCategoryIdentifier);
                if (weight == null)
                {
                    weight = c.WeightCategories.FirstOrDefault();
                }
                if (weight == null)
                {
                    throw new InvalidOperationException("No weight categories found for class");
                }
            }

            return new AddGradeViewModel(parent)
            {
                State = editParams.IsInWhatIfMode ? OperationState.EditingWhatIf : OperationState.Editing,
                Name = editParams.Item.Name,
                Date = editParams.Item.DateInSchoolTime,
                Details = editParams.Item.Details,
                WeightCategories = c.WeightCategories,
                SelectedWeightCategory = weight,
                IsDropped = editParams.Item.IsDropped,
                GradeReceived = editParams.Item.GradeReceived,
                GradeTotal = editParams.Item.GradeTotal,
                _editingGrade = editParams.Item,
                _onSaved = editParams.OnSaved,
                IsUnassignedItem = editParams.IsUnassignedItem,
                _originalDateOffset = editParams.Item.Date.TimeOfDay
            };
        }

        private Action _onSaved;
        private BaseViewItemMegaItem _editingGrade;

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        private TimeSpan _originalDateOffset = new TimeSpan();
        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, nameof(Date)); }
        }

        private string _details = "";

        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        private double _gradeReceived = PowerPlannerSending.Grade.UNGRADED;
        public double GradeReceived
        {
            get { return _gradeReceived; }
            set { SetProperty(ref _gradeReceived, value, nameof(GradeReceived), nameof(GradePercent)); }
        }

        private double _gradeTotal = 100;
        public double GradeTotal
        {
            get { return _gradeTotal; }
            set { SetProperty(ref _gradeTotal, value, nameof(GradeTotal), nameof(GradePercent)); }
        }

        public string GradePercent
        {
            get
            {
                if (GradeReceived == PowerPlannerSending.Grade.UNGRADED || GradeTotal == PowerPlannerSending.Grade.UNGRADED)
                {
                    return "--%";
                }

                if (GradeTotal == 0)
                {
                    return GradeReceived + " " + PowerPlannerResources.GetString("String_ExtraCreditAbbreviation");
                }

                return (GradeReceived / GradeTotal).ToString("0.##%");
            }
        }

        public MyObservableList<ViewItemWeightCategory> WeightCategories { get; private set; }

        private ViewItemWeightCategory _selectedWeightCategory;
        public ViewItemWeightCategory SelectedWeightCategory
        {
            get { return _selectedWeightCategory; }
            set { SetProperty(ref _selectedWeightCategory, value, nameof(SelectedWeightCategory)); }
        }

        private bool _isDropped;
        public bool IsDropped
        {
            get { return _isDropped; }
            set { SetProperty(ref _isDropped, value, nameof(IsDropped)); }
        }

        public async void Save()
        {
            // First we block user interaction on checking if full version, since don't want to dismiss the UI
            // before prompting that they need the full version (otherwise they'd lose data)
            await TryHandleUserInteractionAsync("SaveCheckingIfAllowed", async (cancellationToken) =>
            {
                Guid identifierToIgnore = _editingGrade == null ? Guid.Empty : _editingGrade.Identifier;
                // For free version, block assigning grade if number of graded items exceeds 5
                if (SelectedWeightCategory != null
                    && (_editingGrade == null || _editingGrade.GradeReceived == PowerPlannerSending.Grade.UNGRADED)
                    && SelectedWeightCategory.Class.WeightCategories.SelectMany(i => i.Grades).Where(i => i.Identifier != identifierToIgnore && i.GradeReceived != PowerPlannerSending.Grade.UNGRADED).Count() >= 5
                    && !await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeGradesLimitReached"));
                    return;
                }

                // And then we perform the data operation which doesn't need any UI interaction blocks
                TryStartDataOperationAndThenNavigate(async delegate
                {
                    string name = Name;
                    if (name.Length == 0)
                    {
                        new PortableMessageDialog(PowerPlannerResources.GetString("String_NoNameMessageBody"), PowerPlannerResources.GetString("String_NoNameMessageHeader")).Show();
                        return;
                    }

                    ViewItemWeightCategory weightCategory = SelectedWeightCategory;
                    if (weightCategory == null)
                    {
                        new PortableMessageDialog(PowerPlannerResources.GetString("EditGradePage_MessageNoWeightCategoryBody"), PowerPlannerResources.GetString("EditGradePage_MessageNoWeightCategoryHeader")).Show();
                        return;
                    }

                    //// What If mode
                    if (State == OperationState.AddingWhatIf || State == OperationState.EditingWhatIf)
                    {
                        BaseViewItemMegaItem whatIfGrade;

                        // New
                        if (_editingGrade == null)
                            whatIfGrade = new ViewItemGrade(null)
                            {
                                DateCreated = DateTime.UtcNow,
                                Updated = DateTime.UtcNow
                            };

                        // Existing
                        else
                        {
                            whatIfGrade = _editingGrade;
                            whatIfGrade.Updated = DateTime.UtcNow;
                        }

                        whatIfGrade.Name = name;
                        whatIfGrade.Details = Details;
                        whatIfGrade.Date = DateTime.SpecifyKind(Date, DateTimeKind.Utc).Date.Add(_originalDateOffset);
                        whatIfGrade.GradeReceived = GradeReceived;
                        whatIfGrade.GradeTotal = GradeTotal;

                        if (UsesIsDropped)
                        {
                            whatIfGrade.IsDropped = IsDropped;
                        }

                        whatIfGrade.WasChanged = true;

                        whatIfGrade.CanBeUsedForAchievingDesiredGrade = whatIfGrade.GradeReceived == PowerPlannerSending.Grade.UNGRADED;

                        if (whatIfGrade.WeightCategory != null)
                            whatIfGrade.WeightCategory.Remove(whatIfGrade);

                        weightCategory.Add(whatIfGrade);

                        weightCategory.Class.ResetDream();
                    }

                    else
                    {
                        BaseDataItemHomeworkExamGrade g;
                        if (State == OperationState.Adding)
                            g = new DataItemGrade() { Identifier = Guid.NewGuid() };
                        else
                        {
                            g = _editingGrade.CreateBlankDataItem();
                        }

                        g.Name = name;

                        if (g is DataItemGrade)
                        {
                            g.UpperIdentifier = weightCategory.Identifier;
                        }
                        else if (g is DataItemMegaItem)
                        {
                            (g as DataItemMegaItem).WeightCategoryIdentifier = weightCategory.Identifier;
                        }

                        g.Details = Details;
                        g.Date = DateTime.SpecifyKind(Date, DateTimeKind.Utc).Date.Add(_originalDateOffset);
                        g.GradeReceived = GradeReceived;
                        g.GradeTotal = GradeTotal;

                        if (UsesIsDropped)
                        {
                            g.IsDropped = IsDropped;
                        }

                        DataChanges changes = new DataChanges();
                        changes.Add(g);
                        await PowerPlannerApp.Current.SaveChanges(changes);
                        // We don't cancel here since we need the following to occur, and removing the view model is fine regardless
                    }

                    NavigationManager.SetPreviousAddItemDate(Date);
                    NavigationManager.SelectedWeightCategoryIdentifier = weightCategory.Identifier;

                }, delegate
                {
                    _onSaved?.Invoke();
                    this.RemoveViewModel();
                });
            });
        }
    }
}
