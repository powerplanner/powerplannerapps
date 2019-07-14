using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Adapters;
using InterfacesDroid.DataTemplates;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using ToolsPortable;

namespace PowerPlannerAndroid.Views.Controls
{
    public class RecurrenceControl : InflatedViewWithBinding
    {
        private Spinner _spinnerRepeatOptions;
        private ItemsControlWrapper _dayCheckBoxesLeftSideWrapper, _dayCheckBoxesRightSideWrapper;

        public RecurrenceControl(ViewGroup root) : base(Resource.Layout.RecurrenceControl, root)
        {
            Initialize();
        }

        public RecurrenceControl(Context context, IAttributeSet attrs) : base(Resource.Layout.RecurrenceControl, context, attrs)
        {
            Initialize();
        }

        public RecurrenceControlViewModel ViewModel => DataContext as RecurrenceControlViewModel;

        private void Initialize()
        {
            _spinnerRepeatOptions = FindViewById<Spinner>(Resource.Id.SpinnerRepeatOptions);
            _spinnerRepeatOptions.ItemSelected += _spinnerRepeatOptions_ItemSelected;

            _dayCheckBoxesLeftSideWrapper = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.LinearLayoutDayCheckBoxesLeftSide))
            {
                ItemTemplate = new CustomDataTemplate<RecurrenceControlViewModel.DayCheckBox>(CreateDayCheckBoxView)
            };
            _dayCheckBoxesRightSideWrapper = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.LinearLayoutDayCheckBoxesRightSide))
            {
                ItemTemplate = new CustomDataTemplate<RecurrenceControlViewModel.DayCheckBox>(CreateDayCheckBoxView)
            };
        }

        private WeakReferenceList<BindedCheckBoxView> _dayCheckboxes = new WeakReferenceList<BindedCheckBoxView>();
        private View CreateDayCheckBoxView(ViewGroup root, RecurrenceControlViewModel.DayCheckBox dayCheckBox)
        {
            var view = new BindedCheckBoxView(root.Context, dayCheckBox)
            {
                Enabled = Enabled
            };
            _dayCheckboxes.Add(view);
            return view;
        }

        private class BindedCheckBoxView : CheckBox
        {
            private RecurrenceControlViewModel.DayCheckBox _dayCheckBox;
            public BindedCheckBoxView(Context context, RecurrenceControlViewModel.DayCheckBox dayCheckBox)
                : base(context)
            {
                _dayCheckBox = dayCheckBox;
                _dayCheckBox.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(_dayCheckBox_PropertyChanged).Handler;
                this.CheckedChange += BindedCheckBoxView_CheckedChange;
                UpdateViewProperties();
            }

            private void BindedCheckBoxView_CheckedChange(object sender, CheckedChangeEventArgs e)
            {
                _dayCheckBox.IsChecked = Checked;
            }

            private void _dayCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                UpdateViewProperties();
            }

            private void UpdateViewProperties()
            {
                Text = _dayCheckBox.DisplayName;
                Checked = _dayCheckBox.IsChecked;
            }
        }

        private void _spinnerRepeatOptions_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string s = ViewModel.RepeatOptionsAsStrings.ElementAtOrDefault(e.Position);

            if (s != null)
            {
                ViewModel.SelectedRepeatOptionAsString = s;
            }
        }

        private RadioButton _radioButtonEndsOn, _radioButtonEndsAfter;

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            _spinnerRepeatOptions.Adapter = ObservableAdapter.CreateSimple(ViewModel.RepeatOptionsAsStrings);
            _spinnerRepeatOptions.SetSelection(ViewModel.RepeatOptionsAsStrings.ToList().IndexOf(ViewModel.SelectedRepeatOptionAsString));

            _dayCheckBoxesLeftSideWrapper.ItemsSource = ViewModel.DayCheckBoxesLeftSide;
            _dayCheckBoxesRightSideWrapper.ItemsSource = ViewModel.DayCheckBoxesRightSide;

            FindViewById<EditText>(Resource.Id.EditTextOccurrences).FocusChange += EditTextOccurrences_FocusChange;

            // XML data binding breaks down for the radio buttons for some reason, so just gonna manually set these
            _radioButtonEndsOn = FindViewById<RadioButton>(Resource.Id.RadioButtonEndsOn);
            _radioButtonEndsOn.CheckedChange += RadioButtonEndsOn_CheckedChange;

            _radioButtonEndsAfter = FindViewById<RadioButton>(Resource.Id.RadioButtonEndsAfter);
            _radioButtonEndsAfter.CheckedChange += RadioButtonEndsAfter_CheckedChange;

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            UpdateRadioButtonsEnds();

            FindViewById<InlineDatePicker>(Resource.Id.DatePickerEndsOn).Click += DatePickerEndsOn_Click;

            base.OnDataContextChanged(oldValue, newValue);
        }

        private void DatePickerEndsOn_Click(object sender, EventArgs e)
        {
            ViewModel.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Date;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedEndOption):
                    UpdateRadioButtonsEnds();
                    break;
            }
        }

        private void UpdateRadioButtonsEnds()
        {
            switch (ViewModel.SelectedEndOption)
            {
                case RecurrenceControlViewModel.EndOptions.Date:
                    _radioButtonEndsOn.Checked = true;
                    _radioButtonEndsAfter.Checked = false;
                    break;

                default:
                    _radioButtonEndsOn.Checked = false;
                    _radioButtonEndsAfter.Checked = true;
                    break;
            }
        }

        private void RadioButtonEndsAfter_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                ViewModel.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Occurrences;
            }
        }

        private void RadioButtonEndsOn_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                ViewModel.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Date;
            }
        }

        private void EditTextOccurrences_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                ViewModel.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Occurrences;
            }
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != base.Enabled)
                {
                    base.Enabled = value;

                    FindViewById<View>(Resource.Id.EditTextRepeatInterval).Enabled = value;
                    FindViewById<View>(Resource.Id.SpinnerRepeatOptions).Enabled = value;
                    foreach (var checkBox in _dayCheckboxes)
                    {
                        checkBox.Enabled = value;
                    }
                    FindViewById(Resource.Id.RadioButtonEndsOn).Enabled = value;
                    FindViewById(Resource.Id.DatePickerEndsOn).Enabled = value;
                    FindViewById(Resource.Id.RadioButtonEndsAfter).Enabled = value;
                    FindViewById(Resource.Id.EditTextOccurrences).Enabled = value;
                }
            }
        }

        private void SetEnabled(LinearLayout layout, bool enabled)
        {
            foreach (var child in layout.GetAllChildren().OfType<CheckBox>())
            {
                child.Enabled = enabled;
            }
        }
    }
}