using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Xamarin.Forms;

namespace VxSampleApp
{
    public partial class MainPage : ContentPage
    {
        private VxState<string> _username = new VxState<string>("");
        private Entry _entry;
        private Label _label;
        private PopupTitleBar _titleBar = new PopupTitleBar();

        public MainPage()
        {
            InitializeComponent();

            //_username.ValueChanged += _username_ValueChanged;

            //_entry = new Entry
            //{
            //    Placeholder = "Username"
            //}.BindText(_username);

            //_label = new Label();

            //Content = new StackLayout
            //{
            //    Children =
            //    {
            //        _label,
            //        _entry
            //    }
            //};

            //Content = new LoginComponent()
            //{
            //    Margin = new Thickness(24)
            //};

            _titleBar = new PopupTitleBar()
            {
                Title = "Window A"
            };

            Content = _titleBar;

            Updater();
        }

        private async void Updater()
        {
            while (true)
            {
                await Task.Delay(1000);
                _titleBar.Title += "A";
            }
        }

        //private void _username_ValueChanged(object sender, EventArgs e)
        //{
        //    _label.Text = _username.Value;
        //    _entry.BindText(_username);
        //}
    }

    public class PopupTitleBar : VxComponentForms
    {
        public string Title
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        protected override View Render()
        {
            return new Grid
            {
                BackgroundColor = Color.DarkBlue,
                Children =
                {
                    new Label
                    {
                        TextColor = Color.White,
                        Margin = new Thickness(12),
                        Text = Title
                    }
                }
            };
        }
    }

    public class LoginComponent : VxComponentForms
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

    public class WelcomeComponent : VxComponentForms
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
                    Text = "Milliseconds: " + VxComponentForms.LastMillisecondsToRender + ", Username: " + _username.Value + ", DarkBlue: " + _darkBlue.Value,
                    TextColor = Color.White
                }
            };

            return new Label
            {
                Text = "Milliseconds: " + VxComponentForms.LastMillisecondsToRender
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
