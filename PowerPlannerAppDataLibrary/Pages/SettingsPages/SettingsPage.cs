using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerAppDataLibrary.Pages.SettingsPages
{
    public class SettingsPage : VxViewModelPage
    {
        [VxSubscribe]
        private AccountDataItem _account;

        [VxSubscribe]
        private ViewItemSemester _semester;

        [VxSubscribe]
        private MainScreenViewModel _mainScreenViewModel;

        private bool HasAccount => _account != null;

        private bool IsOnlineAccount => HasAccount && _account.IsOnlineAccount;

        protected override void Initialize()
        {
            base.Initialize();

            _mainScreenViewModel = ViewModel.FindAncestor<MainScreenViewModel>();

            _account = _mainScreenViewModel.CurrentAccount;
            _semester = _mainScreenViewModel.CurrentSemester;
        }

        protected override View Render()
        {
            var stackLayout = new StackLayout
            {
                Margin = new Thickness(20),
                Spacing = 0
            };

            var followingItemMargin = new Thickness(0, 6, 0, 0);

            if (HasAccount)
            {
                stackLayout.Children.Add(new Label
                {
                    Text = _account.Username,
                    FontAttributes = FontAttributes.Bold
                });

                stackLayout.Children.Add(Separator());

                stackLayout.Children.Add(new Label
                {
                    Text = PowerPlannerResources.GetStringWithParameters("String_CurrentSemester", _semester?.Name ?? "None"),
                    MaxLines = 1,
                    Margin = followingItemMargin
                });

                stackLayout.Children.Add(new Button
                {
                    Text = PowerPlannerResources.GetString("String_ViewYearsAndSemesters"),
                    HorizontalOptions = LayoutOptions.Start,
                    Command = CreateCommand(() => _mainScreenViewModel.OpenYears()),
                    Margin = followingItemMargin
                });

                stackLayout.Children.Add(Separator());
            }

            if (IsOnlineAccount)
            {
                stackLayout.Children.Add(new Label
                {
                    Text = GetSyncStatusText(),
                    Margin = followingItemMargin
                });

                bool isDoneSyncing = _mainScreenViewModel.SyncState == MainScreenViewModel.SyncStates.Done;

                stackLayout.Children.Add(new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        new Button
                        {
                            Text = "View errors",
                            Command = CreateCommand(() => _mainScreenViewModel.ViewSyncErrors()),
                            IsVisible = _mainScreenViewModel.HasSyncErrors
                        },

                        new Button
                        {
                            Text = isDoneSyncing ? PowerPlannerResources.GetString("String_SyncNow") : PowerPlannerResources.GetString("String_Syncing"),
                            IsEnabled = isDoneSyncing,
                            Command = CreateCommand(() => _mainScreenViewModel.SyncCurrentAccount())
                        }
                    },
                    Margin = followingItemMargin
                });
            }

            stackLayout.Children.Add(new Button
            {
                Text = "Log out",
                Command = CreateCommand(LogOut)
            });

            return new ScrollView
            {
                Content = stackLayout
            };
        }

        public async void LogOut()
        {
            AccountsManager.SetLastLoginIdentifier(Guid.Empty);
            await ViewModel.FindAncestor<MainWindowViewModel>().SetCurrentAccount(null);
        }

        private View Separator()
        {
            return new Xamarin.Forms.Shapes.Rectangle
            {
                Fill = new SolidColorBrush(PowerPlannerColors.PowerPlannerBlue),
                HeightRequest = 1,
                Margin = new Thickness(0, 6, 0, 0)
            };
        }

        private string GetSyncStatusText()
        {
            if (_mainScreenViewModel.HasSyncErrors)
            {
                return PowerPlannerResources.GetString("String_SyncError");
            }
            else if (_mainScreenViewModel.IsOffline)
            {
                if (_account.LastSyncOn != DateTime.MinValue)
                {
                    return PowerPlannerResources.GetStringWithParameters("String_OfflineLastSync", FriendlyLastSyncTime(_account.LastSyncOn));
                }
                else
                {
                    return PowerPlannerResources.GetString("String_OfflineCouldntSync");
                }
            }
            else
            {
                if (_account.LastSyncOn != DateTime.MinValue)
                {
                    return PowerPlannerResources.GetStringWithParameters("String_LastSync", FriendlyLastSyncTime(_account.LastSyncOn));
                }
                else
                {
                    return PowerPlannerResources.GetString("String_SyncNeeded");
                }
            }
        }

        private static string FriendlyLastSyncTime(DateTime time)
        {
            if (time.Date == DateTime.Today)
            {
                return time.ToString("t");
            }
            else
            {
                return time.ToString("d");
            }
        }
    }
}
