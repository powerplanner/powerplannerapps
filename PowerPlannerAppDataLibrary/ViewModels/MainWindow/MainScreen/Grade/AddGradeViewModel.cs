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
using BareMvvm.Core.Snackbar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using Vx.Views;
using Vx;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade
{
    public class AddGradeViewModel : PopupComponentViewModel
    {
        /// <summary>
        /// View should set this if it enables editing IsDropped from here
        /// </summary>
        public bool UsesIsDropped { get; set; }

        protected override bool InitialAllowLightDismissValue => false;

        public AddGradeViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            Title = GetTitle(state);

            PrimaryCommand = PopupCommand.Save(Save);

            if (VxPlatform.Current == Platform.iOS)
            {
                UsesIsDropped = true;
            }
        }

        private static string GetTitle(OperationState state)
        {
            switch (state)
            {
                case AddGradeViewModel.OperationState.Adding:
                    return PowerPlannerResources.GetString("EditGradePage_HeaderAddString");

                case AddGradeViewModel.OperationState.AddingWhatIf:
                    return PowerPlannerResources.GetString("EditGradePage_HeaderAddWhatIfString");

                case AddGradeViewModel.OperationState.Editing:
                    return PowerPlannerResources.GetString("EditGradePage_HeaderEditString");

                case AddGradeViewModel.OperationState.EditingWhatIf:
                    return PowerPlannerResources.GetString("EditGradePage_HeaderEditWhatIfString");
            }

            throw new NotImplementedException();
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("EditGradePage_TextBoxName.Header"),
                    Text = VxValue.Create(Name, v => Name = v),
                    OnSubmit = Save,
                    AutoFocus = State == OperationState.Adding || State == OperationState.AddingWhatIf
                },

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("EditGradePage_TextBoxGradeReceived.Header"),
                    Margin = new Thickness(0, 18, 0, 0)
                },

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new NumberTextBox
                        {
                            Number = VxValue.Create<double?>(GradeReceived == PowerPlannerSending.Grade.UNGRADED ? (double?)null : (double?)GradeReceived, v => GradeReceived = v.GetValueOrDefault(PowerPlannerSending.Grade.UNGRADED)),
                            VerticalAlignment = VerticalAlignment.Center
                        },

                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("EditGradePage_TextBlockOutOf.Text"),
                            Margin = new Thickness(6, 0, 6, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = Theme.Current.CaptionFontSize,
                            WrapText = false
                        },

                        new NumberTextBox
                        {
                            Number = VxValue.Create<double?>(GradeTotal, v => GradeTotal = v.GetValueOrDefault(0)),
                            VerticalAlignment = VerticalAlignment.Center
                        },

                        new TextBlock
                        {
                            Text = GradePercent,
                            TextAlignment = HorizontalAlignment.Right,
                            WrapText = false,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = Theme.Current.TitleFontSize
                        }.LinearLayoutWeight(1)
                    }
                },

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 18, 0, 0),
                    Children =
                    {
                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("EditGradePage_DatePickerDate.Header"),
                            Value = VxValue.Create<DateTime?>(Date, v => Date = v.GetValueOrDefault(DateTime.Today)),
                            Margin = new Thickness(0, 0, 9, 0)
                        }.LinearLayoutWeight(1),

                        new ComboBox
                        {
                            Header = PowerPlannerResources.GetString("EditGradePage_ComboBoxWeightCategories.Header"),
                            Items = WeightCategories,
                            SelectedItem = VxValue.Create<object>(SelectedWeightCategory, v => SelectedWeightCategory = v as ViewItemWeightCategory),
                            Margin = new Thickness(9, 0, 0, 0)
                        }.LinearLayoutWeight(1)
                    }
                },

                UsesIsDropped ? new CheckBox
                {
                    Text = "Is dropped?",
                    Margin = new Thickness(0, 18, 0, 0),
                    IsChecked = VxValue.Create(IsDropped, v => IsDropped = v)
                } : null,

                new MultilineTextBox
                {
                    Header = PowerPlannerResources.GetString("EditGradePage_TextBoxDetails.Header"),
                    Height = 180, // For now we're just going to leave height as fixed height, haven't implemented dynamic height in iOS
                    Text = VxValue.Create(Details, v => Details = v),
                    Margin = new Thickness(0, 18, 0, 0)
                }

            );
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

            public bool ShowViewGradeSnackbarAfterSaving { get; set; }
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

            return new AddGradeViewModel(parent, addParams.IsInWhatIfMode ? OperationState.AddingWhatIf : OperationState.Adding)
            {
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

            return new AddGradeViewModel(parent, editParams.IsInWhatIfMode ? OperationState.EditingWhatIf : OperationState.Editing)
            {
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
                _originalDateOffset = editParams.Item.Date.TimeOfDay,
                _showViewGradeSnackbarAfterSaving = editParams.ShowViewGradeSnackbarAfterSaving
            };
        }

        private Action _onSaved;
        private BaseViewItemMegaItem _editingGrade;
        private bool _showViewGradeSnackbarAfterSaving;

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

                    if (_showViewGradeSnackbarAfterSaving)
                    {
                        try
                        {
                            BareSnackbar.Make(PowerPlannerResources.GetString("String_GradeAdded"), PowerPlannerResources.GetString("String_ViewGrades"), delegate
                            {
                                try
                                {
                                    TelemetryExtension.Current?.TrackEvent("ClickedSnackbarViewGrades");

                                    MainScreenViewModel.ViewClass(SelectedWeightCategory.Class, ClassViewModel.ClassPages.Grades);
                                }
                                catch (Exception ex)
                                {
                                    TelemetryExtension.Current?.TrackException(ex);
                                }
                            }).Show();
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    }
                });
            });
        }
    }
}
