using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public partial class ReminderSettingsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;

        public ReminderSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_Reminders_Header.Text");

            _account = base.FindAncestor<MainWindowViewModel>().CurrentAccount;
            if (_account != null)
            {
                _taskRemindersDayBefore = _account.TaskRemindersDayBefore;
                _taskRemindersDayOf = _account.TaskRemindersDayOf;
                _eventRemindersDayBefore = _account.EventRemindersDayBefore;
                _eventRemindersDayOf = _account.EventRemindersDayOf;

                _selectedClassReminderOption = _timeSpanToStringMappings.FirstOrDefault(i => i.Key == _account.ClassRemindersTimeSpan).Value ?? ClassReminderOptions.First();

                IsEnabled = true;
            }
        }

        private bool SupportsClassReminders = true;

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_RemindersWithClasses_Description.Text")
                },

                SupportsClassReminders ? RenderHeader(PowerPlannerResources.GetString("Settings_Reminders_ClassRemindersHeader.Text")) : null,

                SupportsClassReminders ? new ComboBox
                {
                    Header = PowerPlannerResources.GetString("String_RemindMe"),
                    Items = ClassReminderOptions,
                    SelectedItem = VxValue.Create<object>(SelectedClassReminderOption, i => SelectedClassReminderOption = i as string),
                    IsEnabled = IsEnabled,
                    Margin = new Thickness(0, 6, 0, 0)
                } : null,

                SupportsClassReminders ? RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_ClassRemindersDescription.Text")) : null,

                RenderHeader(PowerPlannerResources.GetString("Settings_Reminders_TaskRemindersHeader.Text")),

                new Switch
                {
                    Title = PowerPlannerResources.GetString("Settings_Reminders_ToggleDayBefore.Header"),
                    IsOn = VxValue.Create(TaskRemindersDayBefore, v => TaskRemindersDayBefore = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsEnabled
                },

                RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_TaskDayBeforeDescription.Text")),

                new Switch
                {
                    Title = PowerPlannerResources.GetString("Settings_Reminders_ToggleDayOf.Header"),
                    IsOn = VxValue.Create(TaskRemindersDayOf, v => TaskRemindersDayOf = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsEnabled
                },

                RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_TaskDayOfDescription.Text")),

                RenderHeader(PowerPlannerResources.GetString("Settings_Reminders_EventRemindersHeader.Text")),

                new Switch
                {
                    Title = PowerPlannerResources.GetString("Settings_Reminders_ToggleDayBefore.Header"),
                    IsOn = VxValue.Create(EventRemindersDayBefore, v => EventRemindersDayBefore = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsEnabled
                },

                RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_EventDayBeforeDescription.Text")),

                new Switch
                {
                    Title = PowerPlannerResources.GetString("Settings_Reminders_ToggleDayOf.Header"),
                    IsOn = VxValue.Create(EventRemindersDayOf, v => EventRemindersDayOf = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsEnabled
                },

                RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_EventDayOfDescription.Text"))
            );
        }

        private TextBlock RenderCaption(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = Theme.Current.CaptionFontSize,
                TextColor = Theme.Current.SubtleForegroundColor,
                Margin = new Thickness(0, 6, 0, 0)
            };
        }

        private TextBlock RenderHeader(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = Theme.Current.TitleFontSize,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 24, 0, 0)
            };
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, nameof(IsEnabled)); }
        }

        private string _selectedClassReminderOption;
        public string SelectedClassReminderOption
        {
            get => _selectedClassReminderOption;
            set
            {
                if (_selectedClassReminderOption != value)
                {
                    _selectedClassReminderOption = value;
                    OnPropertyChanged(nameof(SelectedClassReminderOption));
                }

                var timeSpan = _timeSpanToStringMappings.FirstOrDefault(i => i.Value == value).Key;

                if (_account.ClassRemindersTimeSpan != timeSpan)
                {
                    _account.ClassRemindersTimeSpan = timeSpan;
                    SaveAndUpdateClassReminders();
                }
            }
        }

        private static KeyValuePair<TimeSpan?, string>[] _timeSpanToStringMappings = new KeyValuePair<TimeSpan?, string>[]
        {
            new KeyValuePair<TimeSpan?, string>(null, PowerPlannerResources.GetString("String_Never")),
            GenerateMapping(0),
            GenerateMapping(5),
            GenerateMapping(10),
            GenerateMapping(15),
            GenerateMapping(30),
            GenerateMapping(60)
        };

        private static KeyValuePair<TimeSpan?, string> GenerateMapping(int minutesBefore)
        {
            return new KeyValuePair<TimeSpan?, string>(TimeSpan.FromMinutes(minutesBefore), string.Format(PowerPlannerResources.GetString("String_XMinutesBefore"), minutesBefore));
        }

        public string[] ClassReminderOptions { get; private set; } = _timeSpanToStringMappings.Select(i => i.Value).ToArray();

        private bool _taskRemindersDayOf;
        public bool TaskRemindersDayOf
        {
            get { return _taskRemindersDayOf; }
            set
            {
                if (_taskRemindersDayOf != value)
                {
                    _taskRemindersDayOf = value;
                    OnPropertyChanged(nameof(TaskRemindersDayOf));
                }

                if (_account.TaskRemindersDayOf != value)
                {
                    _account.TaskRemindersDayOf = value;
                    SaveAndUpdate();
                }
            }
        }

        private bool _taskRemindersDayBefore;
        public bool TaskRemindersDayBefore
        {
            get { return _taskRemindersDayBefore; }
            set
            {
                if (_taskRemindersDayBefore != value)
                {
                    _taskRemindersDayBefore = value;
                    OnPropertyChanged(nameof(TaskRemindersDayBefore));
                }

                if (_account.TaskRemindersDayBefore != value)
                {
                    _account.TaskRemindersDayBefore = value;
                    SaveAndUpdate();
                }
            }
        }

        private bool _eventRemindersDayOf;
        public bool EventRemindersDayOf
        {
            get { return _eventRemindersDayOf; }
            set
            {
                if (_eventRemindersDayOf != value)
                {
                    _eventRemindersDayOf = value;
                    OnPropertyChanged(nameof(EventRemindersDayOf));
                }

                if (_account.EventRemindersDayOf != value)
                {
                    _account.EventRemindersDayOf = value;
                    SaveAndUpdate();
                }
            }
        }

        private bool _eventRemindersDayBefore;
        public bool EventRemindersDayBefore
        {
            get { return _eventRemindersDayBefore; }
            set
            {
                if (_eventRemindersDayBefore != value)
                {
                    _eventRemindersDayBefore = value;
                    OnPropertyChanged(nameof(EventRemindersDayBefore));
                }

                if (_account.EventRemindersDayBefore != value)
                {
                    _account.EventRemindersDayBefore = value;
                    SaveAndUpdate();
                }
            }
        }

        private async void SaveAndUpdate()
        {
            try
            {
                IsEnabled = false;

                System.Diagnostics.Debug.WriteLine("Reminder settings changed, saving...");
                await AccountsManager.Save(_account);
                await RemindersExtension.Current?.ResetReminders(_account, await AccountDataStore.Get(_account.LocalAccountId));
                System.Diagnostics.Debug.WriteLine("Reminder settings changed, saved.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                IsEnabled = true;
            }
        }

        private async void SaveAndUpdateClassReminders()
        {
            try
            {
                IsEnabled = false;

                System.Diagnostics.Debug.WriteLine("Class reminder settings changed, saving...");
                await AccountsManager.Save(_account);

                if (_account.AreClassRemindersEnabled())
                {
                    // Request permission when enabling class reminders
                    RemindersExtension.Current?.RequestReminderPermission();
                    
                    await ClassRemindersExtension.Current?.ResetAllRemindersAsync(_account);
                }
                else
                {
                    ClassRemindersExtension.Current?.RemoveAllReminders(_account);
                }

                System.Diagnostics.Debug.WriteLine("Class reminder settings changed, saved.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                IsEnabled = true;
            }
        }
    }
}
