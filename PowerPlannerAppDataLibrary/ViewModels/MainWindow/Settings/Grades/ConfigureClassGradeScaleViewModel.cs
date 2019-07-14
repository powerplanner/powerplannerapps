using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGradeScaleViewModel : BaseMainScreenViewModelDescendant
    {
        public ViewItemClass Class { get; private set; }

        public MyObservableList<GradeScale> GradeScales { get; private set; }

        /// <summary>
        /// Windows version should set this to true, so that the previous content remains visible
        /// </summary>
        public bool ShowSaveScalePopupInSeparatePopupPane { get; set; }

        public ConfigureClassGradeScaleViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;

            GradeScales = new MyObservableList<GradeScale>(c.GradeScales.Select(
                i => new GradeScale()
                {
                    StartGrade = i.StartGrade,
                    GPA = i.GPA
                }));
            foreach (var gradeScale in GradeScales)
            {
                gradeScale.PropertyChanged += GradeScale_PropertyChanged;
            }
        }

        protected override async System.Threading.Tasks.Task LoadAsyncOverride()
        {
            await ReloadSavedGradeScalesPicker();

            await base.LoadAsyncOverride();
        }

        private void GradeScale_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateSelectedSavedGradeScale();
        }

        public void AddGradeScale()
        {
            GradeScales.Add(new GradeScale());
            GradeScales.Last().PropertyChanged += GradeScale_PropertyChanged;
            UpdateSelectedSavedGradeScale();
        }

        public void RemoveGradeScale(GradeScale scale)
        {
            scale.PropertyChanged -= GradeScale_PropertyChanged;
            GradeScales.Remove(scale);
            UpdateSelectedSavedGradeScale();
        }

        public void SaveGradeScale()
        {
            try
            {
                PagedViewModelWithPopups popupHost;

                if (ShowSaveScalePopupInSeparatePopupPane)
                {
                    // We'll show on a popup layer a level higher, so that the current content is still visible
                    popupHost = MainScreenViewModel.GetPopupViewModelHost();
                }
                else
                {
                    popupHost = GetPopupViewModelHost();
                }

                popupHost.ShowPopup(new SaveGradeScaleViewModel(popupHost, MainScreenViewModel, new SaveGradeScaleViewModel.Parameter()
                {
                    Name = "",
                    OnSaved = delegate
                    {
                        var dontWait = ReloadSavedGradeScalesPicker();
                    },
                    Scales = GradeScales.ToArray()
                }));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool AreScalesValid()
        {
            //check that the numbers are valid
            for (int i = 1; i < GradeScales.Count; i++)
                if (GradeScales[i].StartGrade >= GradeScales[i - 1].StartGrade) //if the current starting grade is equal to or greater than the previous starting grade
                    return false;

            return true;
        }

        private bool HasMadeChanges()
        {
            if (Class.GradeScales == null)
            {
                return true;
            }

            if (GradeScales.Count != Class.GradeScales.Length)
            {
                return true;
            }

            for (int i = 0; i < GradeScales.Count; i++)
            {
                if (GradeScales[i].GPA != Class.GradeScales[i].GPA)
                {
                    return true;
                }

                if (GradeScales[i].StartGrade != Class.GradeScales[i].StartGrade)
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
                if (!AreScalesValid())
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("String_InvalidGradeScalesMessageBody"), PowerPlannerResources.GetString("String_InvalidGradeScalesMessageHeader")).ShowAsync();
                    return;
                }

                GradeScale[] newScales = GradeScales.ToArray();

                DataChanges changes = new DataChanges();

                // Class changes
                {
                    var c = new DataItemClass()
                    {
                        Identifier = Class.Identifier
                    };

                    c.SetGradeScales(newScales);

                    changes.Add(c);
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



        private List<SavedGradeScale> _savedGradeScales;
        public List<SavedGradeScale> SavedGradeScales
        {
            get { return _savedGradeScales; }
            set { SetProperty(ref _savedGradeScales, value, nameof(SavedGradeScales)); }
        }

        private SavedGradeScale _selectedSavedGradeScale;
        public SavedGradeScale SelectedSavedGradeScale
        {
            get { return _selectedSavedGradeScale; }
            set
            {
                if (value == _selectedSavedGradeScale)
                {
                    return;
                }

                SetProperty(ref _selectedSavedGradeScale, value, nameof(SelectedSavedGradeScale));
                OnSelectedSavedGradeScaleChanged();
            }
        }

        private void OnSelectedSavedGradeScaleChanged()
        {
            SavedGradeScale savedScale = SelectedSavedGradeScale;

            if (savedScale == null || savedScale.GradeScales == null)
                return;

            foreach (var s in GradeScales)
            {
                s.PropertyChanged -= GradeScale_PropertyChanged;
            }
            GradeScales.Clear();
            GradeScales.AddRange(savedScale.GradeScales.Select(i => new GradeScale(i.StartGrade, i.GPA)));
            foreach (var s in GradeScales)
            {
                s.PropertyChanged += GradeScale_PropertyChanged;
            }
        }

        private async System.Threading.Tasks.Task ReloadSavedGradeScalesPicker()
        {
            var account = MainScreenViewModel.CurrentAccount;
            var savedScalesManager = await SavedGradeScalesManager.GetForAccountAsync(account);
            var savedScales = await savedScalesManager.GetSavedGradeScalesAsync();

            if (!savedScales.Any(i => i.Name.Equals("United States")))
            {
                savedScales.Add(new SavedGradeScale()
                {
                    Name = "United States",
                    GradeScales = GradeScale.GenerateDefaultScaleWithoutLetters()
                });
            }

            if (!savedScales.Any(i => i.Name.Equals("Eleven-Point System")))
            {
                savedScales.Add(new SavedGradeScale()
                {
                    Name = "Eleven-Point System",
                    GradeScales = GradeScale.GenerateElevenPointScale()
                });
            }

            if (!savedScales.Any(i => i.Name.Equals("Twelve-Point System")))
            {
                savedScales.Add(new SavedGradeScale()
                {
                    Name = "Twelve-Point System",
                    GradeScales = GradeScale.GenerateTwelvePointScale()
                });
            }

            if (!savedScales.Any(i => i.Name.Equals("Mexico - 100 Point")))
            {
                savedScales.Add(new SavedGradeScale()
                {
                    Name = "Mexico - 100 Point",
                    GradeScales = GradeScale.GenerateMexico100PointScale()
                });
            }

            if (!savedScales.Any(i => i.Name.Equals("Mexico - 10 Point")))
            {
                savedScales.Add(new SavedGradeScale()
                {
                    Name = "Mexico - 10 Point",
                    GradeScales = GradeScale.GenerateMexico10PointScale()
                });
            }

            savedScales.Add(new SavedGradeScale()
            {
                Name = PowerPlannerResources.GetString("String_GradeScaleCustom")
            });

            SavedGradeScales = savedScales;

            UpdateSelectedSavedGradeScale();
        }

        private void UpdateSelectedSavedGradeScale()
        {
            try
            {
                var scales = SavedGradeScales;

                if (scales == null)
                    return;

                GradeScale[] curr = GradeScales.ToArray();

                var matching = scales.Where(i => i.GradeScales != null).FirstOrDefault(i => i.GradeScales.SequenceEqual(curr));

                // If no match, we use the Custom (not saved) last item
                if (matching == null)
                    matching = scales.Last();

                if (SelectedSavedGradeScale == matching)
                    return;

                SetSelectedGradeScaleWithoutChangingExistingScales(matching);
            }

            catch { }
        }

        private void SetSelectedGradeScaleWithoutChangingExistingScales(SavedGradeScale scale)
        {
            _selectedSavedGradeScale = scale;
            OnPropertyChanged(nameof(SelectedSavedGradeScale));
        }
    }
}
