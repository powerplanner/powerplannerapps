using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsListViewModel : ComponentViewModel
    {
        [VxSubscribe]
        public DataLayer.AccountDataItem Account { get; private set; }

        public new MainScreenViewModel MainScreenViewModel { get; set; }

        private VxState<bool> _isFullVersion = new VxState<bool>(true);

        public const string HelpUrl = "https://powerplanner.freshdesk.com/support/home";

        /// <summary>
        /// Android sets this to true to have subpages appear as popups
        /// </summary>
        public bool ShowAsPopups { get; set; }

        public SettingsListViewModel(BaseViewModel parent) : base(parent)
        {
            MainScreenViewModel = FindAncestor<MainScreenViewModel>();
            Account = MainScreenViewModel?.CurrentAccount;

            Title = PowerPlannerResources.GetString("String_More");

            UpdateIsFullVersion();
        }

        private bool _updatingIsFullVersion;
        private async void UpdateIsFullVersion()
        {
            if (_updatingIsFullVersion)
            {
                return;
            }

            _updatingIsFullVersion = true;
            try
            {
                _isFullVersion.Value = await PowerPlannerApp.Current.IsFullVersionAsync();
            }
            catch { }
            _updatingIsFullVersion = false;
        }

        public override void OnViewFocused()
        {
            UpdateIsFullVersion();

            base.OnViewFocused();
        }

        protected override View Render()
        {
            var layout = new LinearLayout()
            {
                Margin = NookInsets
            };

            if (IsViewYearsAndSemestersVisible)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = CurrentSemesterText,
                    Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin, Theme.Current.PageMargin, 0),
                    WrapText = false
                });

                layout.Children.Add(new TextButton
                {
                    Text = PowerPlannerResources.GetString("String_ViewYearsAndSemesters"),
                    Click = OpenYears,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, Theme.Current.PageMargin)
                });
            }

            if (IsSyncOptionsVisible)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = SyncStatusText,
                    Margin = new Thickness(Theme.Current.PageMargin, layout.Children.Count == 0 ? Theme.Current.PageMargin : 0, Theme.Current.PageMargin, 0),
                    WrapText = false
                });

                var syncButton = new TextButton
                {
                    Text = SyncButtonText,
                    IsEnabled = SyncButtonIsEnabled,
                    Click = StartSync,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                if (SyncHasError)
                {
                    layout.Children.Add(new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, Theme.Current.PageMargin),
                        Children =
                        {
                            syncButton,

                            new TextBlock
                            {
                                Text = "-",
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(6, 0, 6, 0),
                                WrapText = false
                            }.CaptionStyle(),

                            new TextButton
                            {
                                Text = "View sync errors",
                                IsEnabled = SyncButtonIsEnabled,
                                Click = ViewSyncErrors
                            }
                        }
                    });
                }
                else
                {
                    syncButton.Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, Theme.Current.PageMargin);
                    layout.Children.Add(syncButton);
                }
            }

            if (IsUpgradeToPremiumVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Shop,
                    PowerPlannerResources.GetString("Settings_MainPage_UpgradeToPremiumItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_UpgradeToPremiumItem.Subtitle"),
                    OpenPremiumVersion);
            }

            if (IsCreateAccountVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.AccountCircle,
                    PowerPlannerResources.GetString("Settings_MainPage_CreateAccountItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_CreateAccountItem.Subtitle"),
                    OpenCreateAccount);
            }

            if (IsLogInVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Input,
                    PowerPlannerResources.GetString("Settings_MainPage_LogInItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_LogInItem.Subtitle"),
                    OpenLogIn);
            }

            if (IsMyAccountVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.AccountCircle,
                    PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Subtitle"),
                    OpenMyAccount);
            }

            if (HasAccount && OpenWidgets != null)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Dashboard,
                    PowerPlannerResources.GetString("String_Widgets"),
                    PowerPlannerResources.GetString("Settings_MainPage_Widgets_Subtitle"),
                    () => OpenWidgets(this));
            }

            if (HasAccount && OpenLiveTiles != null)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Dashboard,
                    PowerPlannerResources.GetString("Settings_MainPage_LiveTilesItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_LiveTilesItem.Subtitle"),
                    () => OpenLiveTiles(this));
            }

            if (IsRemindersVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Notifications,
                    PowerPlannerResources.GetString("Settings_MainPage_RemindersItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_RemindersItem.Subtitle"),
                    OpenReminderSettings);
            }

            // Grade options
            if (HasAccount)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Calculate,
                    PowerPlannerResources.GetString("Settings_MainPage_DefaultGradeOptions.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_DefaultGradeOptions.Subtitle"),
                    OpenGradeOptions);
            }

            if (VxPlatform.Current != Platform.iOS && IsSyncOptionsVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Cached,
                    PowerPlannerResources.GetString("Settings_MainPage_SyncOptionsItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_SyncOptionsItem.Subtitle"),
                    () =>
                    {
                        if (VxPlatform.Current == Platform.Uwp)
                        {
                            OpenSyncOptions();
                        }
                        else
                        {
                            OpenSyncOptionsSimple();
                        }
                    });
            }

            if (IsGoogleCalendarIntegrationVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.EventAvailable,
                    PowerPlannerResources.GetString("Settings_MainPage_GoogleCalendarIntegrationItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_GoogleCalendarIntegrationItem.Subtitle"),
                    OpenGoogleCalendarIntegration);
            }

            if (HasAccount && VxPlatform.Current == Platform.Uwp)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.CalendarToday,
                    PowerPlannerResources.GetString("Settings_MainPage_CalendarIntegrationItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_CalendarIntegrationItem.Subtitle"),
                    OpenCalendarIntegration);
            }

            if (IsTwoWeekScheduleVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.DateRange,
                    PowerPlannerResources.GetString("Settings_MainPage_TwoWeekScheduleItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_TwoWeekScheduleItem.Subtitle"),
                    OpenTwoWeekScheduleSettings);
            }

            if (IsSchoolTimeZoneVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.AccessTime,
                    PowerPlannerResources.GetString("Settings_MainPage_SchoolTimeZoneItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_SchoolTimeZoneItem.Subtitle"),
                    OpenSchoolTimeZone);
            }

            if (IsSoundEffectsVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.VolumeUp,
                    PowerPlannerResources.GetString("Settings_MainPage_Sound.Title"),
                    PowerPlannerResources.GetEnabledDisabledString(Account.IsSoundEffectsEnabled),
                    OpenSoundSettings);
            }

            if (IsThemeVisible)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.DesignServices,
                    PowerPlannerResources.GetString("String_Theme"),
                    Helpers.Settings.ThemeOverride.ToLocalizedString(),
                    OpenThemeSettings);
            }

            RenderOption(
                layout,
                MaterialDesign.MaterialDesignIcons.Translate,
                PowerPlannerResources.GetString("Settings_MainPage_LanguageItem.Title"),
                PowerPlannerResources.GetString("Settings_MainPage_LanguageItem.Subtitle"),
                OpenLanguageSettings);

            if (VxPlatform.Current == Platform.Uwp)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.Code,
                    PowerPlannerResources.GetString("Settings_MainPage_ContributeItem.Title"),
                    PowerPlannerResources.GetString("Settings_MainPage_ContributeItem.Subtitle"),
                    () => OpenContribute());
            }

            RenderOption(
                layout,
                MaterialDesign.MaterialDesignIcons.Help,
                PowerPlannerResources.GetString("Settings_MainPage_HelpItem.Title"),
                PowerPlannerResources.GetString("Settings_MainPage_HelpItem.Subtitle"),
                OpenHelp);

            RenderOption(
                layout,
                MaterialDesign.MaterialDesignIcons.Info,
                PowerPlannerResources.GetString("Settings_MainPage_AboutItem.Title"),
                "BareBones Dev",
                OpenAbout);

#if DEBUG
            RenderOption(
                layout,
                MaterialDesign.MaterialDesignIcons.Code,
                "Vx Tests",
                "View Vx UI tests",
                () => ShowPopup(new VxTests.VxTestsViewModel(this)));
#endif

            return new ScrollView(layout);
        }

        private void RenderOption(LinearLayout layout, string icon, string title, string subtitle, Action action)
        {
            layout.Children.Add(RenderOption(icon, title, subtitle, action));
        }

        internal static View RenderOption(string icon, string title, string subtitle, Action action)
        {
            return new ListItemButton
            {
                AltText = title,
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, 12),
                    Orientation = Orientation.Horizontal,
                    Children =
                        {
                            new FontIcon
                            {
                                Glyph = icon,
                                FontSize = 40,
                                Color = Theme.Current.AccentColor
                            },

                            new LinearLayout
                            {
                                Margin = new Thickness(6, 0, 0, 0),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = title,
                                        FontWeight = FontWeights.Bold,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = subtitle,
                                        TextColor = Theme.Current.SubtleForegroundColor,
                                        WrapText = false
                                    }
                                }
                            }.LinearLayoutWeight(1)
                        }
                },
                Click = action
            };
        }

        /// <summary>
        /// This is only false if they enter the settings page from the login page. This will still be true for default offline accounts.
        /// </summary>
        public bool HasAccount => Account != null;

        public bool IsOnlineAccount => HasAccount && Account.IsOnlineAccount;

        public bool IsDefaultOfflineAccount => HasAccount && Account.IsDefaultOfflineAccount;

        public bool IsCreateAccountVisible => IsDefaultOfflineAccount;

        public bool IsLogInVisible => IsDefaultOfflineAccount;

        public bool IsUpgradeToPremiumVisible => !_isFullVersion.Value;

        /// <summary>
        /// We hide when it's the default account, only options for them are create or log in
        /// </summary>
        public bool IsMyAccountVisible => HasAccount && !IsDefaultOfflineAccount;

        /// <summary>
        /// Should be visible for default offline account too, clicking it will tell users they need to create an account first
        /// </summary>
        public bool IsGoogleCalendarIntegrationVisible => IsOnlineAccount || IsDefaultOfflineAccount;

        public bool IsRemindersVisible => HasAccount;

        public bool IsSyncOptionsVisible => IsOnlineAccount;

        public bool IsTwoWeekScheduleVisible => HasAccount;

        public bool IsSchoolTimeZoneVisible => HasAccount;

        public bool IsSoundEffectsVisible => HasAccount && VxPlatform.Current == Platform.Uwp;

        public bool IsThemeVisible => ThemeExtension.Current != null;

        public bool IsViewYearsAndSemestersVisible => HasAccount && MainScreenViewModel != null;

        private bool _initializedCurrentSemesterName;
        public string CurrentSemesterName
        {
            get
            {
                if (!_initializedCurrentSemesterName)
                {
                    _initializedCurrentSemesterName = true;

                    if (MainScreenViewModel != null && MainScreenViewModel.CurrentSemester != null)
                    {
                        MainScreenViewModel.CurrentSemester.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(CurrentSemester_PropertyChanged).Handler;
                    }
                }

                return MainScreenViewModel?.CurrentSemester?.Name;
            }
        }

        public string CurrentSemesterText => PowerPlannerResources.GetStringWithParameters("String_CurrentSemester", CurrentSemesterName);

        private void CurrentSemester_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewItemSemester.Name))
            {
                MarkDirty();
            }
        }

        private bool _initializedSyncStatus;
        private string _syncStatusText;
        public string SyncStatusText
        {
            get
            {
                if (!_initializedSyncStatus)
                {
                    _initializedSyncStatus = true;

                    if (MainScreenViewModel != null)
                    {
                        MainScreenViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(MainScreenViewModel_PropertyChanged).Handler;
                    }

                    UpdateSyncStatus();
                }

                return _syncStatusText;
            }
            set => SetProperty(ref _syncStatusText, value, nameof(SyncStatusText));
        }

        private string _syncButtonText;
        public string SyncButtonText
        {
            get => _syncButtonText;
            set => SetProperty(ref _syncButtonText, value, nameof(SyncButtonText));
        }

        private bool _syncButtonIsEnabled;
        public bool SyncButtonIsEnabled
        {
            get => _syncButtonIsEnabled;
            set => SetProperty(ref _syncButtonIsEnabled, value, nameof(SyncButtonIsEnabled));
        }

        private bool _syncHasError;
        public bool SyncHasError
        {
            get => _syncHasError;
            set => SetProperty(ref _syncHasError, value, nameof(SyncHasError));
        }

        private void UpdateSyncStatus()
        {
            var account = MainScreenViewModel?.CurrentAccount;

            if (account == null)
            {
                SyncStatusText = PowerPlannerResources.GetString("String_NoAccountToSync");
                SyncButtonText = PowerPlannerResources.GetString("String_SyncNow");
                SyncButtonIsEnabled = false;
            }

            if (MainScreenViewModel.SyncState == MainScreenViewModel.SyncStates.Done)
            {
                SyncButtonText = PowerPlannerResources.GetString("String_SyncNow");
                SyncButtonIsEnabled = true;
            }
            else
            {
                SyncButtonText = PowerPlannerResources.GetString("String_Syncing");
                SyncButtonIsEnabled = false;
            }

            if (MainScreenViewModel.HasSyncErrors)
            {
                SyncHasError = true;
                SyncStatusText = PowerPlannerResources.GetString("String_SyncError");
            }
            else if (MainScreenViewModel.IsOffline)
            {
                SyncHasError = false;

                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SyncStatusText = PowerPlannerResources.GetStringWithParameters("String_OfflineLastSync", FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SyncStatusText = PowerPlannerResources.GetString("String_OfflineCouldntSync");
                }
            }
            else
            {
                SyncHasError = false;

                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SyncStatusText = PowerPlannerResources.GetStringWithParameters("String_LastSync", FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SyncStatusText = PowerPlannerResources.GetString("String_SyncNeeded");
                }
            }
        }

        private static string FriendlyLastSyncTime(DateTime time)
        {
            if (time.Date == DateTime.Today)
            {
                return DateTimeFormatterExtension.Current.FormatAsShortTime(time);
            }
            else
            {
                return time.ToString("d");
            }
        }

        private void MainScreenViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainScreenViewModel.SyncState):
                case nameof(MainScreenViewModel.HasSyncErrors):
                case nameof(MainScreenViewModel.IsOffline):
                    UpdateSyncStatus();
                    MarkDirty();
                    break;
            }
        }

        /// <summary>
        /// Android initializes this
        /// </summary>
        public static Action<SettingsListViewModel> OpenWidgets { get; set; }

        /// <summary>
        /// UWP initializes this
        /// </summary>
        public static Action<SettingsListViewModel> OpenLiveTiles { get; set; }

        public async void OpenHelp()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_OpenHelp");

                await BrowserExtension.Current?.OpenUrlAsync(new Uri(HelpUrl));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                var dontWait = new PortableMessageDialog("Failed to open web browser", "Error").ShowAsync();
            }
        }

        public void OpenYears()
        {
            MainScreenViewModel?.OpenOrShowYears();
        }

        public void StartSync()
        {
            MainScreenViewModel?.SyncCurrentAccount();
        }

        public void OpenMyAccount()
        {
            ShowPopup(MyAccountViewModel.Load(ParentForSubviews));
        }

        public void OpenAbout()
        {
            ShowPopup(new AboutViewModel(ParentForSubviews));
        }

        public void OpenGradeOptions()
        {
            ShowPopup(new ConfigureDefaultGradesListViewModel(ParentForSubviews));
        }

        public void OpenPremiumVersion()
        {
            PowerPlannerApp.Current.PromptPurchase(null);
        }

        public void OpenReminderSettings()
        {
            ShowPopup(new ReminderSettingsViewModel(ParentForSubviews));
        }

        public void OpenSyncOptions()
        {
            Show(new SyncOptionsViewModel(ParentForSubviews));
        }

        /// <summary>
        /// If UI app doesn't want to use the split model view model, then use this approach
        /// </summary>
        public void OpenSyncOptionsSimple()
        {
            Show(new SyncOptionsSimpleViewModel(ParentForSubviews));
        }

        public void OpenCalendarIntegration()
        {
            Show(new CalendarIntegrationViewModel(ParentForSubviews));
        }

        public void OpenTwoWeekScheduleSettings()
        {
            ShowPopup(new TwoWeekScheduleSettingsViewModel(ParentForSubviews));
        }

        /// <summary>
        /// Android sets this
        /// </summary>
        public static Action<SettingsListViewModel> CustomOpenGoogleCalendarIntegration { get; set; }

        public void OpenGoogleCalendarIntegration()
        {
            TelemetryExtension.Current?.TrackEvent("Action_OpenGoogleCalendarIntegration");

            try
            {
                if (AlertIfGoogleCalendarIntegrationNotPossible())
                {
                    return;
                }
                else
                {
                    if (VxPlatform.Current == Platform.iOS)
                    {
                        BrowserExtension.Current.OpenUrlAsync(new Uri(GoogleCalendarIntegrationViewModel.Url));
                    }
                    else if (CustomOpenGoogleCalendarIntegration != null)
                    {
                        CustomOpenGoogleCalendarIntegration(this);
                    }
                    else
                    {
                        MainScreenViewModel.ShowPopup(new GoogleCalendarIntegrationViewModel(MainScreenViewModel, Account));
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public bool AlertIfGoogleCalendarIntegrationNotPossible()
        {
            if (IsDefaultOfflineAccount)
            {
                new PortableMessageDialog(PowerPlannerResources.GetString("Settings_GoogleCalendar_NoAccountMessage"), PowerPlannerResources.GetString("String_NoAccount")).Show();
                return true;
            }

            return false;
        }

        private const string ContributeUrl = "https://powerplanner.net/contribute";

        /// <summary>
        /// Returns the url, caller must navigate to the url
        /// </summary>
        /// <returns></returns>
        public async void OpenContribute()
        {
            TelemetryExtension.Current?.TrackEvent("Action_OpenContribute");

            try
            {
                await BrowserExtension.Current?.OpenUrlAsync(new Uri(ContributeUrl));
            }
            catch { }
        }

        public void OpenCreateAccount()
        {
            try
            {
                ShowPopup(CreateAccountViewModel.CreateForUpgradingDefaultAccount(this, Account));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void OpenLogIn()
        {
            try
            {
                ShowPopup(new LoginViewModel(this)
                {
                    Message = PowerPlannerResources.GetString("Settings_LogInFromDefaultAccountMessage"),
                    DefaultAccountToDelete = Account.IsDefaultOfflineAccount ? Account : null // It should always be the default account, but just in case
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void OpenSchoolTimeZone()
        {
            ShowPopup(new SchoolTimeZoneSettingsViewModel(ParentForSubviews));
        }

        public void OpenSoundSettings()
        {
            ShowPopup(new SoundEffectsViewModel(ParentForSubviews));
        }

        public void OpenThemeSettings()
        {
            ShowPopup(new ThemeSettingsViewModel(ParentForSubviews));
        }

        public void OpenLanguageSettings()
        {
            Show(new LanguageSettingsViewModel(ParentForSubviews));
        }

        public void ViewSyncErrors()
        {
            if (MainScreenViewModel != null && MainScreenViewModel.HasSyncErrors)
            {
                MainScreenViewModel.ViewSyncErrors();
            }
        }

        private BaseViewModel ParentForSubviews => GetParentForSubviews(this);

        public static BaseViewModel GetParentForSubviews(BaseViewModel thisViewModel)
        {
            if (PowerPlannerApp.ShowSettingsPagesAsPopups)
            {
                return thisViewModel.FindAncestor<PagedViewModelWithPopups>();
            }
            else
            {
                return thisViewModel.FindAncestor<PagedViewModel>();
            }
        }

        public static void Show(BaseViewModel viewModel)
        {
            if (PowerPlannerApp.ShowSettingsPagesAsPopups)
            {
                viewModel.Parent.ShowPopup(viewModel);
            }
            else
            {
                viewModel.Parent.FindAncestorOrSelf<PagedViewModel>()?.Navigate(viewModel);
            }
        }
    }
}
