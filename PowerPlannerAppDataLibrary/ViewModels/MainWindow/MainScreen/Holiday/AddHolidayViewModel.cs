using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday
{
    public class AddHolidayViewModel : PopupComponentViewModel
    {
        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        protected override bool InitialAllowLightDismissValue => false;

        public override string GetPageName()
        {
            if (State == OperationState.Adding)
            {
                return "AddHolidayView";
            }
            else
            {
                return "EditHolidayView";
            }
        }

        public ViewItemHoliday HolidayToEdit { get; private set; }

        public class AddParameter
        {
            public Guid SemesterIdentifier;
            public DateTime StartDate = DateTime.Today;
            public DateTime EndDate = DateTime.Today;
        }

        public AddParameter AddParams { get; private set; }

        private AddHolidayViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            base.Title = PowerPlannerResources.GetString(state == OperationState.Adding ? "String_AddHoliday" : "String_EditHoliday");

            if (State == OperationState.Editing)
            {
                Commands = new PopupCommand[]
                {
                    PopupCommand.Save(Save),
                    PopupCommand.Delete(ConfirmDelete)
                };
            }
            else
            {
                PrimaryCommand = PopupCommand.Save(Save);
            }
        }

        public static AddHolidayViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            return new AddHolidayViewModel(parent, OperationState.Adding)
            {
                AddParams = addParams,
                _startDate = addParams.StartDate.Date,
                _endDate = addParams.EndDate.Date
            };
        }

        public static AddHolidayViewModel CreateForEdit(BaseViewModel parent, ViewItemHoliday holidayToEdit)
        {
            var viewModel = new AddHolidayViewModel(parent, OperationState.Editing)
            {
                HolidayToEdit = holidayToEdit,
                Name = holidayToEdit.Name
            };

            viewModel._startDate = holidayToEdit.Date.Date;

            if (holidayToEdit.EndTime != PowerPlannerSending.DateValues.UNASSIGNED)
                viewModel._endDate = holidayToEdit.EndTime.Date;
            else
            {
                viewModel._endDate = viewModel.StartDate;
            }

            viewModel.ListenToItem(holidayToEdit.Identifier).Deleted += viewModel.Holiday_Deleted;

            return viewModel;
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("AddHolidayView_TextBoxName.Header"),
                    Text = VxValue.Create(Name, v => Name = v),
                    AutoFocus = State == OperationState.Adding,
                    OnSubmit = Save
                },

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 18, 0, 0),
                    Children =
                    {
                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("AddHolidayView_DatePickerStart.Header"),
                            Value = VxValue.Create<DateTime?>(StartDate, v => StartDate = v.GetValueOrDefault(DateTime.Today)),
                            Margin = new Thickness(0, 0, 9, 0)
                        }.LinearLayoutWeight(1),

                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("AddHolidayView_DatePickerEnd.Header"),
                            Value = VxValue.Create<DateTime?>(EndDate, v => EndDate = v.GetValueOrDefault(DateTime.Today)),
                            Margin = new Thickness(9, 0, 0, 0)
                        }.LinearLayoutWeight(1)
                    }
                }

            );
        }

        private void Holiday_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        private DateTime _startDate;

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                value = value.Date;

                if (value > EndDate)
                {
                    _endDate = value.Add(_endDate - _startDate);
                    OnPropertyChanged(nameof(EndDate));
                }

                SetProperty(ref _startDate, value, nameof(StartDate));
            }
        }

        private DateTime _endDate;

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                value = value.Date;

                if (value < StartDate)
                {
                    _startDate = value.Add(_startDate - _endDate);
                    OnPropertyChanged(nameof(StartDate));
                }

                SetProperty(ref _endDate, value, nameof(EndDate));
            }
        }

        public async void Save()
        {
            try
            {
                string name = Name;

                if (string.IsNullOrWhiteSpace(name))
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetStringNoNameMessageBody(), PowerPlannerResources.GetStringNoNameMessageHeader()).ShowAsync();
                    return;
                }

                DataItemMegaItem holiday;

                if (HolidayToEdit != null)
                    holiday = new DataItemMegaItem()
                    {
                        Identifier = HolidayToEdit.Identifier
                    };

                else if (AddParams != null)
                    holiday = new DataItemMegaItem()
                    {
                        Identifier = Guid.NewGuid(),
                        UpperIdentifier = AddParams.SemesterIdentifier,
                        MegaItemType = PowerPlannerSending.MegaItemType.Holiday
                    };

                else
                    throw new NullReferenceException("Either editing holiday or add holiday param must be initialized.");

                holiday.Name = name;

                holiday.Date = DateTime.SpecifyKind(StartDate.Date, DateTimeKind.Utc);

                if (!SqlDate.IsValid(holiday.Date))
                    holiday.Date = DateTime.Today;

                holiday.EndTime = DateTime.SpecifyKind(EndDate.Date, DateTimeKind.Utc);
                holiday.EndTime = holiday.EndTime.AddDays(1).AddSeconds(-1);

                if (!SqlDate.IsValid(holiday.EndTime))
                    holiday.EndTime = DateTime.Today;



                if (holiday.Date > holiday.EndTime)
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("EditSemesterPage_String_StartDateGreaterThanEndExplanation"), PowerPlannerResources.GetString("EditSemesterPage_String_InvalidStartDate")).ShowAsync();
                    return;
                }

                DataChanges changes = new DataChanges();
                changes.Add(holiday);

                TryStartDataOperationAndThenNavigate(async delegate
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                }, delegate
                {
                    this.RemoveViewModel();
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public async void ConfirmDelete()
        {
            if (await PowerPlannerApp.ConfirmDeleteAsync())
            {
                Delete();
            }
        }

        public async void Delete()
        {
            if (HolidayToEdit == null)
            {
                this.RemoveViewModel();
                return;
            }

            try
            {
                await MainScreenViewModel.DeleteItem(HolidayToEdit.Identifier);
            }

            catch { }
        }
    }
}
