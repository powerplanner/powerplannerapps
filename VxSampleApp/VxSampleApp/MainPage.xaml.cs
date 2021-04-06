using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Collections.ObjectModel;

namespace VxSampleApp
{
    public partial class MainPage : ContentPage
    {
        private VxState<string> _username = new VxState<string>("");
        private Entry _entry;
        private Label _label;
        //private PopupWindow _popupWindow = new PopupWindow()
        //{
        //    Title = "Window A"
        //};

        public MainPage()
        {
            InitializeComponent();

            Content = new VxMainPage()
            {
                IsRootComponent = true
            };
        }

        private enum MenuItems
        {
            Calendar,
            Agenda,
            Settings
        }
    }

    public class VxMainPage : VxComponent
    {
        public static VxMainPage Current { get; private set; }

        public static ObservableCollection<ViewItemTask> Tasks { get; } = new ObservableCollection<ViewItemTask>()
        {
            new ViewItemTask()
            {
                Title = "Task 1",
                ClassName = "Class A"
            },

            new ViewItemTask()
            {
                Title = "Task 2",
                ClassName = "Class A"
            },

            new ViewItemTask()
            {
                Title = "Task 3",
                ClassName = "Class B"
            }
        };

        private enum MenuItems
        {
            Calendar,
            Agenda,
            Settings
        }

        private VxState<MenuItems> _selectedMenuItem = new VxState<MenuItems>(MenuItems.Settings);
        private MenuItems[] _availableMenuItems = new MenuItems[]
                        {
                            MenuItems.Calendar,
                            MenuItems.Agenda,
                            MenuItems.Settings
                        };

        private VxState<int> _width = new VxState<int>(200);

        protected override void Initialize()
        {
            Current = this;
            Updater();
        }

        public void ShowAddTask()
        {

        }

        protected override View Render()
        {
            return new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = _width.Value },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                Children =
                {
                    new ListView
                    {
                        ItemsSource = _availableMenuItems,
                        Margin = new Thickness(24)
                    }.BindSelectedItem(_selectedMenuItem),

                    CreateContent(_selectedMenuItem.Value).Column(1)
                }
            };
        }

        private async void Updater()
        {
            while (true)
            {
                await Task.Delay(1000);
                _width.Value = new Random().Next(150, 250);
            }
        }

        private View CreateContent(MenuItems menuItem)
        {
            switch (menuItem)
            {
                case MenuItems.Calendar:
                    return new CalendarPage();

                case MenuItems.Agenda:
                    return new AgendaPage();

                case MenuItems.Settings:
                    return new SettingsPage()
                    {
                        SidebarWidth = _width.Value
                    };

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class CalendarPage : VxComponent
    {
        protected override View Render()
        {
            return new Label
            {
                Text = "Calendar",
                Margin = new Thickness(24)
            };
        }
    }

    public class ViewItemTask
    {
        public string Title { get; set; }
        public string ClassName { get; set; }

        public override string ToString()
        {
            return Title + " - " + ClassName;
        }
    }

    public class AgendaPage : VxComponent
    {
        private VxState<ViewItemTask> _selectedTask = new VxState<ViewItemTask>();

        protected override View Render()
        {
            return new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                Children =
                {
                    new Button
                    {
                        Text = "Add task",
                        Command = CreateCommand(AddTask)
                    },

                    new ListView
                    {
                        ItemsSource = VxMainPage.Tasks
                    }.BindSelectedItem(_selectedTask).Row(1)
                }
            };
        }

        private void AddTask()
        {
            VxMainPage.Current.ShowAddTask();
        }
    }

    public class SettingsPage : VxComponent
    {
        public int SidebarWidth { get; set; }

        protected override View Render()
        {
            Debug.WriteLine("Render SettingsPage");

            return new StackLayout
            {
                Margin = new Thickness(24),
                Children =
                {
                    new Label
                    {
                        Text = "Settings... Sidebar width: " + SidebarWidth
                    },
                    new SettingsItem
                    {
                        Title = "My Account... Sidebar width: " + SidebarWidth
                    },
                    new SettingsUsername()
                }
            };
        }
    }

    public class SettingsItem : VxComponent
    {
        public string Title { get; set; }

        protected override View Render()
        {
            Debug.WriteLine("Render SettingsItem");

            return new Label { Text = "SettingsItem: " + Title };
        }
    }

    public class SettingsUsername : VxComponent
    {
        private VxState<string> _username = new VxState<string>("");

        protected override View Render()
        {
            return new StackLayout
            {
                Children =
                {
                    new Entry
                    {
                        Placeholder = "Username"
                    }.BindText(_username),

                    new Label
                    {
                        Text = "Username: " + _username.Value
                    }
                }
            };
        }
    }

    public class Page : VxPage
    {
        protected override View Render()
        {
            return new Label
            {
                Text = "Calendar"
            };
        }
    }

    public class PopupWindow : VxComponent
    {
        //public PopupTitleBar TitleBar { get; private set; } = new PopupTitleBar();
        private VxState<string> _contentText = new VxState<string>("Content");

        public string Title { get; set; }

        protected override View Render()
        {
            return new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                Children =
                {
                    // The problem is when we set this title bar, since it's an existing view reference it removes the existing view from the existing displayed view, since views can only have one parent... We have to return new references...
                    new PopupTitleBar
                    {
                        Title = Title
                    },
                    new Label
                    {
                        Text = _contentText.Value
                    }.Row(1)
                }
            };
        }

        protected override void Initialize()
        {
            Updater();
        }

        private async void Updater()
        {
            while (true)
            {
                await Task.Delay(1000);
                _contentText.Value += ".";
            }
        }
    }

    public class PopupTitleBar : VxComponent
    {
        public string Title { get; set; }

        protected override View Render()
        {
            bool swap = Title.Length % 2 == 1;

            return new Grid
            {
                BackgroundColor = Color.DarkBlue,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    new Label
                    {
                        TextColor = Color.White,
                        Margin = new Thickness(12),
                        Text = Title
                    }.Column(swap ? 1 : 0),
                    new Button
                    {
                        Text = "Close"
                    }.Column(swap ? 0 : 1)
                }
            };
        }
    }

    public class LoginComponent : VxComponent
    {
        private VxState<string> _username = new VxState<string>("");
        private VxState<string> _password = new VxState<string>("");

        protected override View Render()
        {
            string usernameError = null;

            if (_username.Value.Contains("@"))
            {
                usernameError = "Your username cannot contain @ symbols";
            }

            return new StackLayout
            {
                Children =
                {
                    new Label { Text = "Perf: " + LastMillisecondsToRender, Margin = new Thickness(0,0,0,12) },

                    new Label { Text = "Username" },
                    new Entry
                    {
                        Placeholder = "Username"
                    }.BindText(_username),

                    new Label
                    {
                        Text = usernameError,
                        IsVisible = usernameError != null,
                        Margin = new Thickness(0,0,0,12),
                        TextColor = Color.Red
                    },

                    new Label { Text = "Password: " + _password.Value },
                    new Entry
                    {
                        Placeholder = "Password"
                    }.BindText(_password),

                    new Button
                    {
                        Text = "Log in",
                        Command = CreateCommand(Login)
                    }
                }
            };
        }

        private void Login()
        {

        }
    }

    public class WelcomeComponent : VxComponent
    {
        private VxState<string> _username = new VxState<string>("");
        private VxState<bool> _darkBlue = new VxState<bool>(true);
        private VxState<string[]> _users = new VxState<string[]>(new string[] { "84382" });

        public WelcomeComponent()
        {
            //Update();
        }

        protected override View Render()
        {
            var sl = new StackLayout()
            {
                Children =
                {
                    new Label { Text = "Users: " + _users.Value.Length },
                    new Label { Text = "Render time: " + LastMillisecondsToRender }
                }
            };

            foreach (var user in _users.Value)
            {
                sl.Children.Add(new Label
                {
                    Text = user
                });
            }

            sl.Children.Add(new Button
            {
                Text = "Add user",
                Command = CreateCommand(AddUser)
            });

            return sl;

            return new ContentView
            {
                BackgroundColor = _darkBlue.Value ? Color.DarkBlue : Color.Blue,
                Content = new Label
                {
                    Text = "Milliseconds: " + VxComponent.LastMillisecondsToRender + ", Username: " + _username.Value + ", DarkBlue: " + _darkBlue.Value,
                    TextColor = Color.White
                }
            };

            return new Label
            {
                Text = "Milliseconds: " + VxComponent.LastMillisecondsToRender
            };
        }

        private void AddUser()
        {
            var users = _users.Value.ToList();
            users.Add(new Random().Next(10000, 99999).ToString());
            _users.Value = users.ToArray();
        }

        private async void Update()
        {
            while (true)
            {
                await Task.Delay(1000);

                var users = _users.Value.ToList();
                users.Add(new Random().Next(10000, 99999).ToString());
                _users.Value = users.ToArray();

                _username.Value = _username.Value + "a";
                _darkBlue.Value = !_darkBlue.Value;
            }
        }
    }
}
