﻿using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class AddClassViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        /// <summary>
        /// Views should set this to true to enable editing details
        /// </summary>
        public bool IncludesEditingDetails { get; set; }

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public override string GetPageName() => State == OperationState.Adding ? "AddClassView" : "EditClassView";

        public ViewItemClass ClassToEdit { get; private set; }

        public class AddParameter
        {
            public Guid SemesterIdentifier;

            public IEnumerable<ViewItemClass> Classes { get; set; }

            public bool NavigateToClassAfterAdd { get; set; } = true;

            public Action<DataItemClass> OnClassAddedAction;
        }

        public AddParameter AddParams { get; private set; }

        private AddClassViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            Title = PowerPlannerResources.GetString(state == OperationState.Adding ? "AddClassPage_AddTitle" : "AddClassPage_EditTitle");

            PrimaryCommand = PopupCommand.Save(Save);

            if (state == OperationState.Editing)
            {
                SecondaryCommands = new PopupCommand[]
                {
                    PopupCommand.Delete(ConfirmDelete)
                };
            }

            _colors.Add(new ColorItem("Pick custom color", new byte[] { 0, 0, 0 }));
        }

        public static AddClassViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            return new AddClassViewModel(parent, OperationState.Adding)
            {
                AddParams = addParams,
                Color = PickUnusedColor(addParams.Classes)
            };
        }

        public static AddClassViewModel CreateForEdit(BaseViewModel parent, ViewItemClass classToEdit)
        {
            var answer = new AddClassViewModel(parent, OperationState.Editing)
            {
                ClassToEdit = classToEdit,
                Name = classToEdit.Name,
                Color = classToEdit.Color,
                Details = classToEdit.Details
            };

            if (!PowerPlannerSending.DateValues.IsUnassigned(classToEdit.StartDate))
            {
                answer.StartDate = classToEdit.StartDate;
            }

            if (!PowerPlannerSending.DateValues.IsUnassigned(classToEdit.EndDate))
            {
                answer.EndDate = classToEdit.EndDate;
            }

            // If there's a custom start/end date, we check the partial semester box
            if (answer.StartDate != null || answer.EndDate != null)
            {
                answer.IsPartialSemesterClass = true;
            }

            return answer;
        }

        private ObservableCollection<ColorItem> _colors = new ObservableCollection<ColorItem>(ColorItem.DefaultColors);

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("AddClassPage_TextBoxName.Header"),
                    Text = VxValue.Create(Name, v => Name = v),
                    AutoFocus = State == OperationState.Adding,
                    OnSubmit = Save
                },

                new ColorPicker
                {
                    Header = PowerPlannerResources.GetString("AddClassPage_ColorPickerEditClassColor.Header"),
                    Color = VxValue.Create(Color.ToColor(), v => Color = new byte[] { v.R, v.G, v.B }),
                    Margin = new Thickness(0, 18, 0, 0)
                },

                new CheckBox
                {
                    Text = PowerPlannerResources.GetString("String_PartialSemesterClass"),
                    IsChecked = VxValue.Create(IsPartialSemesterClass, v => IsPartialSemesterClass = v),
                    Margin = new Thickness(0, 18, 0, 0)
                },

                IsPartialSemesterClass ? new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 18, 0, 0),
                    Children =
                    {
                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("EditSemesterPage_DatePickerStart.Header"),
                            Value = VxValue.Create(StartDate, v => StartDate = v),
                            Margin = new Thickness(0, 0, 9, 0)
                        }.LinearLayoutWeight(1),

                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("EditSemesterPage_DatePickerEnd.Header"),
                            Value = VxValue.Create(EndDate, v => EndDate = v),
                            Margin = new Thickness(9, 0, 0, 0)
                        }.LinearLayoutWeight(1)
                    }
                } : null,

                IncludesEditingDetails ? new MultilineTextBox
                {
                    Header = PowerPlannerResources.GetString("EditTaskOrEventPage_TextBoxDetails.Header"),
                    Text = VxValue.Create(Details, v => Details = v),
                    Height = 180, // For now we're just going to leave height as fixed height, haven't implemented dynamic height in iOS
                    Margin = new Thickness(0, 18, 0, 0)
                } : null

            );
        }

        private static byte[] PickUnusedColor(IEnumerable<ViewItemClass> currentClasses)
        {
            LinkedList<ColorItem> unused = new LinkedList<ColorItem>(ColorItem.DefaultColors);

            foreach (ViewItemClass c in currentClasses)
            {
                foreach (ColorItem i in unused)
                {
                    if (i.Equals(c.Color))
                    {
                        unused.Remove(i);
                        break;
                    }
                }
            }

            if (unused.Count > 0)
                return unused.First.Value.Color;

            return ColorItem.DefaultColors.First().Color;
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        private byte[] _color;
        public byte[] Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value, nameof(Color)); }
        }

        private bool _isPartialSemesterClass = false;
        public bool IsPartialSemesterClass
        {
            get { return _isPartialSemesterClass; }
            set { SetProperty(ref _isPartialSemesterClass, value, nameof(IsPartialSemesterClass)); }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value, nameof(StartDate)); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value, nameof(EndDate)); }
        }

        private string _details = "";
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        public async void Save()
        {
            try
            {
                string name = Name.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("String_NoClassNameMessageBody"), PowerPlannerResources.GetString("String_NoNameMessageHeader")).ShowAsync();
                    return;
                }

                if (IsPartialSemesterClass && EndDate != null && StartDate != null && EndDate.Value.Date < StartDate.Value.Date)
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("EditSemesterPage_String_StartDateGreaterThanEndExplanation"), PowerPlannerResources.GetString("EditSemesterPage_String_InvalidStartDate")).ShowAsync();
                    return;
                }

                await TryHandleUserInteractionAsync("SaveClass", async (cancellationToken) =>
                {
                    Action recordTelemetryAction = delegate
                    {
                        TelemetryExtension.Current?.TrackEvent("SavedClass", new Dictionary<string, string>
                        {
                            { "State", State.ToString() },
                            { "UsingCustomColor", (!ColorItem.DefaultColors.Any(i => i.Equals(Color))).ToString() }
                        });
                    };

                    if (ClassToEdit != null)
                    {
                        DataItemClass c = new DataItemClass()
                        {
                            Identifier = ClassToEdit.Identifier,
                            Name = name,
                            RawColor = Color
                        };

                        PopulateClassInfo(c);

                        DataChanges editChanges = new DataChanges();
                        editChanges.Add(c);
                        await PowerPlannerApp.Current.SaveChanges(editChanges);
                        recordTelemetryAction();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    else
                    {
                        // Get current semester
                        Guid semesterId = AddParams.SemesterIdentifier;

                        if (semesterId == Guid.Empty)
                            throw new ArgumentException("CurrentSemesterId was empty");

                        var newItems = AccountDataStore.GenerateNewDefaultClass(MainScreenViewModel.CurrentAccount, semesterId, name, Color);

                        PopulateClassInfo(newItems.OfType<DataItemClass>().First());

                        DataChanges changes = new DataChanges();

                        foreach (var i in newItems)
                            changes.Add(i);

                        await PowerPlannerApp.Current.SaveChanges(changes);
                        recordTelemetryAction();
                        cancellationToken.ThrowIfCancellationRequested();

                        AddParams.OnClassAddedAction?.Invoke((DataItemClass)newItems.First());

                        if (AddParams.NavigateToClassAfterAdd)
                        {
                            DataItemClass c = (DataItemClass)newItems.First();

                            var dontWait = MainScreenViewModel.SelectClass(c.Identifier);
                        }
                    }

                    this.RemoveViewModel();
                }, "Failed to save your class. Your error has been reported.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void PopulateClassInfo(DataItemClass c)
        {
            if (IncludesEditingDetails)
            {
                c.Details = Details;
            }

            if (IsPartialSemesterClass)
            {
                if (StartDate != null)
                {
                    c.StartDate = DateTime.SpecifyKind(StartDate.Value.Date, DateTimeKind.Utc);
                    if (!SqlDate.IsValid(c.StartDate))
                        c.StartDate = SqlDate.MinValue;
                }
                else
                {
                    c.StartDate = SqlDate.MinValue;
                }

                if (EndDate != null)
                {
                    c.EndDate = DateTime.SpecifyKind(EndDate.Value.Date, DateTimeKind.Utc);
                    if (!SqlDate.IsValid(c.EndDate))
                        c.EndDate = SqlDate.MinValue;
                }
                else
                {
                    c.EndDate = SqlDate.MinValue;
                }

                if (StartDate != null || EndDate != null)
                {
                    TelemetryExtension.Current?.TrackEvent("UsedClassStartEndDates");
                }
            }

            else
            {
                c.StartDate = SqlDate.MinValue;
                c.EndDate = SqlDate.MinValue;
            }
        }

        public async void ConfirmDelete()
        {
            if (await PowerPlannerApp.ConfirmDeleteAsync(PowerPlannerResources.GetString("String_ConfirmDeleteClassMessage"), PowerPlannerResources.GetString("String_ConfirmDeleteClassHeader"), useConfirmationCheckbox: true))
            {
                Delete();
            }
        }

        /// <summary>
        /// Assumes native view has already confirmed delete
        /// </summary>
        public void Delete()
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                if (ClassToEdit != null)
                {
                    await MainScreenViewModel.DeleteItem(ClassToEdit.Identifier);
                }

            }, delegate
            {
                RemoveViewModel();
            });
        }
    }
}
