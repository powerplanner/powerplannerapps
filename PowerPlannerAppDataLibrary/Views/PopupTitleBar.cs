using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace PowerPlannerAppDataLibrary.Views
{
    public class PopupTitleBar : Grid
    {
        public event EventHandler CloseClicked;

        private StackLayout _primaryCommandsLayout;
        private const double CommandSpacing = 2;

        public PopupTitleBar()
        {
            //Orientation = StackOrientation.Horizontal;
            //Children.Add(new Label() { Text = "PopupTitleForms: " });
            //Children.Add(new Label().Bind(Label.TextProperty, nameof(Title), source: this));

            // iOS will implement custom renderer
            if (Device.RuntimePlatform != Device.iOS)
            {
                // When on non-full-screen style (Windows or Chromebook or Android tablet), we want...
                // [Title]      [More] | [Primary | commands] | [Close]
                // Notice the commands should all have white divider lines between them in this view

                // When full-screen style (Android), we want...
                // [Back] [Title]             [Primary commands] [More]

                // Note that primary commands are always displayed in reverse (first command is on the right)

                // Sometimes we do have multiple primary commands (like the edit class or year dialogs also have a delete option).

                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // 0: Back button (when full-screen)
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star }); // 1: Title
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // 2: More button (when popup)
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // 3: Primary commands
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // 4: Close button (when popup) or More button (when full-screen)

                // TODO: Back button

                Children.Add(new Label() { LineBreakMode = LineBreakMode.NoWrap }
                    .Column(1)
                    .Bind(Label.TextProperty, nameof(Title), source: this));

                _primaryCommandsLayout = new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                    BackgroundColor = Color.White // TODO: Use theme for background (black or white)
                }
                .Column(3)
                .Bind(StackLayout.SpacingProperty, nameof(IsFullScreenStyle), convert: (bool isFullScreenStyle) => isFullScreenStyle ? 0d : CommandSpacing)
                .Bind(StackLayout.IsVisibleProperty, nameof(PrimaryCommands), convert: (IEnumerable<PopupCommandBarItem> primaryCommands) => (primaryCommands != null && primaryCommands.Any()));

                Children.Add(_primaryCommandsLayout);

                // Close button
                Children.Add(new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        new Xamarin.Forms.Shapes.Rectangle()
                        {
                            Fill = new SolidColorBrush(Color.White),
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            WidthRequest = CommandSpacing
                        },

                        CreatePrimaryCommand(new PopupCommandBarItem()
                        {
                            Title = "Close"
                        }.Click(() => CloseClicked?.Invoke(this, new EventArgs())))
                    }
                }
                .Column(4)
                .Bind(StackLayout.IsVisibleProperty, nameof(IsFullScreenStyle), convert: (bool isFullScreenStyle) => !isFullScreenStyle));
            }
        }

        public string Title
        {
            get => GetValue(TitleProperty) as string;
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(PopupTitleBar));

        public string BackText
        {
            get => GetValue(BackTextProperty) as string;
            set => SetValue(BackTextProperty, value);
        }

        public static readonly BindableProperty BackTextProperty = BindableProperty.Create(nameof(BackText), typeof(string), typeof(PopupTitleBar));

        /// <summary>
        /// Does not support observable lists, to change the commands set a new list
        /// </summary>
        public List<PopupCommandBarItem> PrimaryCommands
        {
            get => GetValue(PrimaryCommandsProperty) as List<PopupCommandBarItem>;
            set => SetValue(PrimaryCommandsProperty, value);
        }

        public static readonly BindableProperty PrimaryCommandsProperty = BindableProperty.Create(nameof(PrimaryCommands), typeof(List<PopupCommandBarItem>), typeof(PopupTitleBar), propertyChanged: PrimaryCommandsPropertyChanged);

        private static void PrimaryCommandsPropertyChanged(BindableObject sender, object oldVal, object newVal)
        {
            //(sender as PopupTitleBar).PrimaryCommandsPropertyChanged();
        }

        private void PrimaryCommandsPropertyChanged()
        {
            if (_primaryCommandsLayout == null)
            {
                return;
            }

            _primaryCommandsLayout.Children.Clear();

            if (PrimaryCommands != null)
            {
                if (PrimaryCommands.Any())
                {
                    // We add this blank view so we get a spacing on the far left
                    _primaryCommandsLayout.Children.Add(new ContentView());
                }

                IEnumerable<PopupCommandBarItem> primaryCommandsEnumerable = PrimaryCommands;

                foreach (var cmd in primaryCommandsEnumerable.Reverse())
                {
                    _primaryCommandsLayout.Children.Add(CreatePrimaryCommand(cmd));
                }
            }
        }

        private View CreatePrimaryCommand(PopupCommandBarItem item)
        {
            return new Button()
            {
                Text = item.Title
            };
            //.Click(() => item.InvokeClicked());
        }

        /// <summary>
        /// If this is set, the "more" button will be displayed. Set to null to not have the "more" button.
        /// </summary>
        public Func<IEnumerable<PopupCommandBarItem>> MoreCommands
        {
            get => GetValue(MoreCommandsProperty) as Func<IEnumerable<PopupCommandBarItem>>;
            set => SetValue(MoreCommandsProperty, value);
        }

        public static readonly BindableProperty MoreCommandsProperty = BindableProperty.Create(nameof(MoreCommands), typeof(Func<IEnumerable<PopupCommandBarItem>>), typeof(PopupTitleBar));

        public bool IsFullScreenStyle
        {
            get => (bool)GetValue(IsFullScreenStyleProperty);
            set => SetValue(IsFullScreenStyleProperty, value);
        }

        public static readonly BindableProperty IsFullScreenStyleProperty = BindableProperty.Create(nameof(IsFullScreenStyle), typeof(bool), typeof(PopupTitleBar), true);
    }

    public class PopupCommandBarItem : BindableBase
    {
        public event EventHandler Clicked;

        public string Title
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public ImageSource IconImageSource
        {
            get => GetValue<ImageSource>();
            set => SetValue(value);
        }

        public bool Destructive
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        internal void InvokeClicked()
        {
            Clicked?.Invoke(this, new EventArgs());
        }

        public PopupCommandBarItem Click(Action value)
        {
            Clicked += delegate { value(); };
            return this;
        }
    }
}
