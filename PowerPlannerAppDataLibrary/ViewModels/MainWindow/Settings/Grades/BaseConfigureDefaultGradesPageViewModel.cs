using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public abstract class BaseConfigureDefaultGradesPageViewModel : PopupComponentViewModel
    {
        public VxState<States> State { get; private set; } = new VxState<States>(States.Loading);
        public AccountDataItem Account { get; private set; }
        private VxState<MyObservableList<ViewItemYear>> _years = new VxState<MyObservableList<ViewItemYear>>(null);
        protected HashSet<ViewItemClass> SelectedClasses { get; private set; } = new HashSet<ViewItemClass>();
        public IEnumerable<ViewItemClass> AllClasses => _years.Value.SelectMany(i => i.Semesters).SelectMany(i => i.Classes);
        private int _totalClasses;
        public bool IsEnabled => State == States.Initial || State == States.Applied;

        protected BaseConfigureDefaultGradesPageViewModel(BaseViewModel parent) : base(parent)
        {
            Account = MainScreenViewModel.CurrentAccount;
        }

        public enum States
        {
            Loading,
            Initial, // Can return to this after editing anything
            Saving,
            Applied
        }

        protected override async System.Threading.Tasks.Task LoadAsyncOverride()
        {
            var yearsGroup = await YearsViewItemsGroup.LoadAsync(Account.LocalAccountId);
            _years.Value = yearsGroup.School.Years;
            foreach (var c in _years.Value.SelectMany(i => i.Semesters).SelectMany(i => i.Classes))
            {
                SelectedClasses.Add(c);
                _totalClasses++;
            }
            State.Value = States.Initial;
        }

        public void RenderApplyUI(LinearLayout layout)
        {
            string applyText;
            var state = State.Value;

            switch (state)
            {
                case States.Loading:
                case States.Initial:
                    applyText = PowerPlannerResources.GetStringWithParameters("Settings_DefaultGradeOptions_SaveAndApply", (SelectedClasses.Count == _totalClasses ? PowerPlannerResources.GetString("String_All") : SelectedClasses.Count.ToString()));
                    break;

                case States.Saving:
                    applyText = PowerPlannerResources.GetString("String_Saving");
                    break;

                case States.Applied:
                    applyText = PowerPlannerResources.GetString("String_Saved");
                    break;

                default:
                    throw new NotImplementedException();
            }

            layout.Children.Add(new AccentButton
            {
                Text = applyText,
                Margin = new Thickness(0, 24, 0, 0),
                Click = StartApply,
                IsEnabled = State.Value == States.Initial
            });

            if (_years.Value != null)
            {
                foreach (var year in _years.Value)
                {
                    foreach (var semester in year.Semesters)
                    {
                        if (semester.Classes.Count > 0)
                        {
                            layout.Children.Add(new TextBlock
                            {
                                Text = year.Name + " > " + semester.Name,
                                Margin = new Thickness(0, 12, 0, 0),
                                WrapText = false
                            });

                            foreach (var c in semester.Classes)
                            {
                                layout.Children.Add(new CheckBox
                                {
                                    Text = c.Name,
                                    IsChecked = SelectedClasses.Contains(c),
                                    IsCheckedChanged = isChecked =>
                                    {
                                        if (isChecked)
                                        {
                                            SelectedClasses.Add(c);
                                            MarkDirty();
                                        }
                                        else
                                        {
                                            SelectedClasses.Remove(c);
                                            MarkDirty();
                                        }
                                    },
                                    IsEnabled = State.Value != States.Saving
                                });
                            }
                        }
                    }
                }
            }

            // When any other property changed, we reset to initial
            if (State.Value == States.Applied)
            {
                State.SetValueSilently(States.Initial);
            }
        }

        private async void StartApply()
        {
            try
            {
                if (!CanApply())
                {
                    return;
                }

                State.Value = States.Saving;

                await Apply();

                State.Value = States.Applied;
            }

            catch (Exception ex)
            {
                State.Value = States.Initial;
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }
        }

        protected virtual bool CanApply()
        {
            return true;
        }

        protected abstract Task Apply();
    }
}
