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
    public class ReminderSettingsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;

        public ReminderSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_Reminders_Header.Text");

            _account = base.FindAncestor<MainWindowViewModel>().CurrentAccount;
            if (_account != null)
            {
                _remindersDayBefore = _account.RemindersDayBefore;
                _remindersDayOf = _account.RemindersDayOf;

                _selectedClassReminderOption = _timeSpanToStringMappings.FirstOrDefault(i => i.Key == _account.ClassRemindersTimeSpan).Value ?? ClassReminderOptions.First();

                IsEnabled = true;
            }
        }

        private bool SupportsClassReminders = VxPlatform.Current != Platform.iOS;

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
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

                        RenderHeader(PowerPlannerResources.GetString("Settings_Reminders_TaskEventRemindersHeader.Text")),

                        new Switch
                        {
                            Title = PowerPlannerResources.GetString("Settings_Reminders_ToggleDayBefore.Header"),
                            IsOn = VxValue.Create(RemindersDayBefore, v => RemindersDayBefore = v),
                            Margin = new Thickness(0, 12, 0, 0),
                            IsEnabled = IsEnabled
                        },

                        RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_DayBeforeDescription.Text")),

                        new Switch
                        {
                            Title = PowerPlannerResources.GetString("Settings_Reminders_ToggleDayOf.Header"),
                            IsOn = VxValue.Create(RemindersDayOf, v => RemindersDayOf = v),
                            Margin = new Thickness(0, 12, 0, 0),
                            IsEnabled = IsEnabled
                        },

                        RenderCaption(PowerPlannerResources.GetString("Settings_Reminders_DayOfDescription.Text"))
                    }
                }
            };
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

        private bool _remindersDayOf;
        public bool RemindersDayOf
        {
            get { return _remindersDayOf; }
            set
            {
                if (_remindersDayOf != value)
                {
                    _remindersDayOf = value;
                    OnPropertyChanged(nameof(RemindersDayOf));
                }

                if (_account.RemindersDayOf != value)
                {
                    _account.RemindersDayOf = value;
                    SaveAndUpdate();
                }
            }
        }

        private bool _remindersDayBefore;
        public bool RemindersDayBefore
        {
            get { return _remindersDayBefore; }
            set
            {
                if (_remindersDayBefore != value)
                {
                    _remindersDayBefore = value;
                    OnPropertyChanged(nameof(RemindersDayBefore));
                }

                if (_account.RemindersDayBefore != value)
                {
                    _account.RemindersDayBefore = value;
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
