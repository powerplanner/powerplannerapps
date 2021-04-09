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
using ToolsPortable;

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

    public static class Classes
    {
        public static ObservableCollection<ViewItemClass> Value = new ObservableCollection<ViewItemClass>
        {
            new ViewItemClass() { Name = "No class", Color = Color.Blue },
            new ViewItemClass() { Name = "Math", Color = Color.DarkBlue },
            new ViewItemClass() { Name = "Science", Color = Color.Red },
            new ViewItemClass { Name = "CSC 121", Color = Color.Green }
        };
    }

    public class VxMainPage : VxPageWithPopups
    {
        public static VxMainPage Current { get; private set; }

        public static ObservableCollection<ViewItemTask> Tasks { get; } = new ObservableCollection<ViewItemTask>()
        {
            new ViewItemTask()
            {
                Title = "Task 1",
                Class = Classes.Value[1]
            },

            new ViewItemTask()
            {
                Title = "Task 2",
                Class = Classes.Value[1]
            },

            new ViewItemTask()
            {
                Title = "Task 3",
                Class = Classes.Value[2]
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

    public class ViewItemTask : BindableBase
    {
        public string Title
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public ViewItemClass Class
        {
            get => GetValue<ViewItemClass>();
            set => SetValue(value);
        }
        public DateTime Date
        {
            get => GetValueOrDefault<DateTime>(DateTime.Today);
            set => SetValue(value);
        }

        public override string ToString()
        {
            return Title + " - " + Class.Name;
        }
    }

    public class ViewItemClass : BindableBase
    {
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public Color Color
        {
            get => GetValue<Color>();
            set => SetValue(value);
        }

        public override string ToString()
        {
            return Name + " (ToString)";
        }
    }

    public class ListItemTask : VxComponent
    {
        private ViewItemTask Task => BindingContext as ViewItemTask;

        protected override bool IsDependentOnBindingContext => true;

        protected override View Render()
        {
            return new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Xamarin.Forms.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(Task.Class.Color),
                        WidthRequest = 20,
                        HeightRequest = 20
                    },

                    new Label { Text = Task.Title + " - " + Task.Class.Name }
                },
                Margin = new Thickness(12)
            };
        }
    }

    public class AgendaPage : VxComponent
    {
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
                        ItemsSource = VxMainPage.Tasks,
                        SelectionMode = ListViewSelectionMode.None,
                        ItemTemplate = CreateViewCellItemTemplate<ViewItemTask>("taskItemTemplate", task =>
                        {
                            return new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal,
                                Children =
                                {
                                    new Xamarin.Forms.Shapes.Rectangle
                                    {
                                        Fill = new SolidColorBrush(task.Class.Color),
                                        WidthRequest = 20,
                                        HeightRequest = 20
                                    },

                                    new Label { Text = task.Title + " - " + task.Class.Name + " due "+ task.Date }
                                },
                                Margin = new Thickness(12)
                            };
                        })
                    }.ItemTap(TaskItemTap).Row(1)
                }
            };
        }

        private void TaskItemTap(ItemTappedEventArgs e)
        {
            ShowPopup(new ViewTaskPage()
            {
                Task = (ViewItemTask)e.Item
            });
        }

        private void AddTask()
        {
            ShowPopup(new AddTaskPage());
        }
    }

    public class ViewTaskPage : VxPage
    {
        [VxSubscribe]
        public ViewItemTask Task { get; set; }

        protected override View Render()
        {
            return new PopupWindow
            {
                Title = "View task",
                Content = new StackLayout
                {
                    Margin = new Thickness(24),
                    Children =
                    {
                        new Label { Text = Task.Title },
                        new Label { Text = Task.Class.Name },
                        new Button { Text = "Edit task", Command = CreateCommand(() => ShowPopup(new AddTaskPage { TaskToEdit = Task }))}
                    }
                }
            };
        }
    }

    public class AddTaskPage : VxPage
    {
        private VxSilentState<string> _title;
        private VxState<string> _titleError = new VxState<string>(null);
        private VxState<ViewItemClass> _class;
        private VxSilentState<DateTime> _date;
        private bool IsNoClassClass => _class.Value?.Name == "No class";

        public ViewItemTask TaskToEdit { get; set; }

        protected override void Initialize()
        {
            _title = new VxSilentState<string>(TaskToEdit?.Title ?? "");
            _class = new VxState<ViewItemClass>(TaskToEdit?.Class ?? Classes.Value[1]);
            _date = new VxSilentState<DateTime>(TaskToEdit?.Date ?? DateTime.Today);

            base.Initialize();
        }

        private Lazy<Binding> _pickerDisplayBinding = new Lazy<Binding>(() => new Binding()
        {
            Path = nameof(ViewItemClass.Name)
        });

        protected override View Render()
        {
            return new PopupWindow
            {
                Title = TaskToEdit != null ? "Edit task" : "Add task",
                Content = new StackLayout
                {
                    Margin = new Thickness(24),
                    Children =
                    {
                        new Label { Text = "Title" },
                        new Entry
                        {
                            
                        }.BindText(_title),
                        new Label { Text = _titleError.Value, TextColor = Color.Red, IsVisible = _titleError.Value != null },

                        new Picker
                        {
                            Title = "Class name",
                            Margin = new Thickness(0,12,0,0),
                            ItemsSource = Classes.Value,
                            ItemDisplayBinding = _pickerDisplayBinding.Value
                        }.BindSelectedItem(_class),

                        new Label { Text = "Include no class options", IsVisible = IsNoClassClass },

                        new Label { Text = "Date", Margin = new Thickness(0,12,0,0) },
                        new DatePicker
                        {
                            
                        }.BindDate(_date),

                        new Button
                        {
                            Text = "Save",
                            Command = CreateCommand(Save),
                            Margin = new Thickness(0,12,0,0)
                        }
                    }
                }
            };
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_title.Value))
            {
                _titleError.Value = "Title is required.";
                return;
            }

            if (_class.Value == null)
            {
                new PortableMessageDialog("Class is required").Show();
                return;
            }

            if (TaskToEdit != null)
            {
                TaskToEdit.Title = _title.Value;
                TaskToEdit.Class = _class.Value;
                TaskToEdit.Date = _date.Value;
            }
            else
            {
                VxMainPage.Tasks.Add(new ViewItemTask
                {
                    Title = _title.Value,
                    Class = _class.Value,
                    Date = _date.Value
                });
            }

            RemoveThisPage();
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
        public string Title { get; set; }

        public new View Content { get; set; }

        protected override View Render()
        {
            return new Grid
            {
                Children =
                {
                    new Xamarin.Forms.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(new Color(0, 0, 0, 0.3))
                    }.Tap(() => RemoveThisPage()),

                    new Grid()
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        MinimumHeightRequest = 300,
                        WidthRequest = 400,
                        BackgroundColor = Color.White,
                        RowDefinitions =
                        {
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition { Height = GridLength.Star }
                        },
                        Children =
                        {
                            RenderTitleBar(),

                            Content.Row(1)
                        }
                    }
                }
            };
        }

        private View RenderTitleBar()
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
                        Text = "Close",
                        TextColor = Color.White,
                        Command = CreateCommand(RemoveThisPage)
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
