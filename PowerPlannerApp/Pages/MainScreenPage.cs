using PowerPlannerApp.App;
using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Exceptions;
using PowerPlannerApp.Pages.SettingsPages;
using PowerPlannerApp.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using Xamarin.Forms;
using static PowerPlannerApp.NavigationManager;

namespace PowerPlannerApp.Pages
{
    public class MainScreenPage : VxPage
    {
        private VxState<MainMenuSelections> _selectedMenuItem = new VxState<MainMenuSelections>(MainMenuSelections.Settings);
        private ObservableCollection<MainMenuSelections> _availableMenuItems = new ObservableCollection<MainMenuSelections>()
        {
            MainMenuSelections.Calendar,
            MainMenuSelections.Schedule,
            MainMenuSelections.Classes,
            MainMenuSelections.Settings
        };

        private VxState<ScheduleViewItemsGroup> _scheduleViewItemsGroup = new VxState<ScheduleViewItemsGroup>();
        private VxState<bool> _loading = new VxState<bool>(true);

        public ScheduleViewItemsGroup ScheduleViewItemsGroup => _scheduleViewItemsGroup.Value;

        public AccountDataItem Account => PowerPlannerVxApp.Current.GetCurrentAccount();

        protected override async void Initialize()
        {
            base.Initialize();

            await OnSemesterChanged();
        }

        protected override View Render()
        {
            if (_loading.Value)
            {
                return null;
            }

            if (_scheduleViewItemsGroup.Value == null)
            {
                MakeAvailableItemsLike(new MainMenuSelections[]
                {
                    MainMenuSelections.Years,
                    MainMenuSelections.Settings
                });
            }
            else if (_scheduleViewItemsGroup.Value.Classes.Count == 0)
            {
                MakeAvailableItemsLike(new MainMenuSelections[]
                {
                    MainMenuSelections.Schedule,
                    MainMenuSelections.Classes,
                    MainMenuSelections.Years,
                    MainMenuSelections.Settings
                });
            }
            else
            {
                MakeAvailableItemsLike(new MainMenuSelections[]
                {
                    MainMenuSelections.Calendar,
                    MainMenuSelections.Day,
                    MainMenuSelections.Agenda,
                    MainMenuSelections.Schedule,
                    MainMenuSelections.Classes,
                    MainMenuSelections.Years,
                    MainMenuSelections.Settings
                });
            }

            return new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 200 },
                    new ColumnDefinition { Width = GridLength.Star }
                },

                Children =
                {
                    RenderSidebar(),

                    RenderContent().Column(1)
                }
            };
        }

        private bool MakeAvailableItemsLike(params NavigationManager.MainMenuSelections[] desired)
        {
            bool answer = IListExtensions.MakeListLike(_availableMenuItems, desired);

            return answer;
        }

        private View RenderSidebar()
        {
            return new Grid
            {
                BackgroundColor = PowerPlannerColors.PowerPlannerBlue,

                RowDefinitions =
                {
                    new RowDefinition { Height = 150 },
                    new RowDefinition { Height = GridLength.Star }
                },

                Children =
                {
                    new Image
                    {
                        Aspect = Aspect.AspectFit,
                        Margin = new Thickness(0, 24, 0, 0),
                        Source = new FileImageSource()
                        {
                            File = "Assets/Logo.png" // Note for Android/iOS, this will have to be different
                        }
                    },

                    new ListView
                    {
                        ItemsSource = _availableMenuItems,
                        ItemTemplate = CreateViewCellItemTemplate<MainMenuSelections>("sidebarMenuItemTemplate", s =>
                        {
                            return new ContentView
                            {
                                HeightRequest = 48,
                                Content = new Label
                                {
                                    Text = s.ToString(),
                                    TextColor = Color.White,
                                    FontSize = 20,
                                    LineBreakMode = LineBreakMode.NoWrap,
                                    Margin = new Thickness(36, 0, 0, 4),
                                    VerticalOptions = LayoutOptions.Center
                                }
                            };
                        })
                    }.Row(1).BindSelectedItem(_selectedMenuItem)
                }
            };
        }

        private View RenderContent()
        {
            switch (_selectedMenuItem.Value)
            {
                case MainMenuSelections.Calendar:
                    return new CalendarPage();

                case MainMenuSelections.Agenda:
                    return new AgendaPage(this);

                case MainMenuSelections.Settings:
                    return new SettingsPage();

                default:
                    return new Label { Text = _selectedMenuItem.Value.ToString() };
            }
        }

        private async Task OnSemesterChanged()
        {
            _scheduleViewItemsGroup.Value = null;
            _loading.Value = true;

            // Restore the default stored items
            NavigationManager.RestoreDefaultMemoryItems();

            // Disconnect the previous
            //if (ScheduleViewItemsGroup != null)
            //{
            //    Classes.EndMakeThisACopyOf(ScheduleViewItemsGroup.Classes);
            //    if (_scheduleChangesOccurredHandler != null)
            //    {
            //        ScheduleViewItemsGroup.OnChangesOccurred -= _scheduleChangesOccurredHandler;
            //    }
            //    ScheduleViewItemsGroup = null;
            //}

            // Clear the current classes
            //Classes.Clear();

            var currAccount = PowerPlannerVxApp.Current.GetCurrentAccount();

            // If there's believed to be a semester (although that semester may not exist, this simply means Guid isn't empty)
            if (currAccount.CurrentSemesterId != Guid.Empty)
            {
                // Load the classes/schedules
                try
                {
                    _scheduleViewItemsGroup.Value = await ScheduleViewItemsGroup.LoadAsync(currAccount.LocalAccountId, currAccount.CurrentSemesterId);
                }
                catch (SemesterNotFoundException)
                {
                    // Semester didn't actually exist
                    _scheduleViewItemsGroup.Value = null;
                    _loading.Value = false;
                    return;
                }

                var CurrentSemester = _scheduleViewItemsGroup.Value.Semester;

                // Change the default date

                // If semester has already ended
                if (!PowerPlannerSending.DateValues.IsUnassigned(CurrentSemester.End) && DateTime.SpecifyKind(CurrentSemester.End, DateTimeKind.Local).Date < DateTime.Today)
                {
                    NavigationManager.SetDisplayMonth(DateTime.SpecifyKind(CurrentSemester.End, DateTimeKind.Local), preserveForever: true);
                    NavigationManager.SetSelectedDate(DateTime.SpecifyKind(CurrentSemester.End, DateTimeKind.Local), preserveForever: true);
                }

                // If semester hasn't started yet
                else if (!PowerPlannerSending.DateValues.IsUnassigned(CurrentSemester.Start) && DateTime.SpecifyKind(CurrentSemester.Start, DateTimeKind.Local).Date > DateTime.Today)
                {
                    NavigationManager.SetDisplayMonth(DateTime.SpecifyKind(CurrentSemester.Start, DateTimeKind.Local), preserveForever: true);
                    NavigationManager.SetSelectedDate(DateTime.SpecifyKind(CurrentSemester.Start, DateTimeKind.Local), preserveForever: true);
                }

                // Make this a copy of the Classes list
                //Classes.MakeThisACopyOf(ScheduleViewItemsGroup.Classes);

                //if (_scheduleChangesOccurredHandler == null)
                //{
                //    _scheduleChangesOccurredHandler = new WeakEventHandler<DataChangedEvent>(ViewModelSchedule_OnChangesOccurred).Handler;
                //}
                //ScheduleViewItemsGroup.OnChangesOccurred += _scheduleChangesOccurredHandler;
            }

            _loading.Value = false;
        }
    }
}
