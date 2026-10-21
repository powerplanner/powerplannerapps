using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Views;
using System;
using Vx.Uwp;
using Vx.Uwp.Views;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace PowerPlannerUWP.Views
{
    internal class UwpFloatingAddItemButton : UwpView<FloatingAddItemButton, Grid>
    {
        private static readonly double MainSize = Vx.Views.FloatingActionButton.DefaultSize;
        private const double MiniSize = 40;
        private const double SlideDistance = 40;
        private const double OverlayOpacity = 0.9;
        private static readonly Duration AnimationDuration = new Duration(TimeSpan.FromMilliseconds(200));

        private sealed class Section
        {
            public StackPanel Panel;
            public TranslateTransform Transform;
        }

        private readonly Rectangle _overlay;
        private readonly Button _addButton;
        private readonly TextBlock _taskLabel;
        private readonly Section _eventSection;
        private readonly Section _holidaySection;

        private bool _isExpanded;
        private bool _supportsAddHoliday;
        private Storyboard _storyboard;

        public UwpFloatingAddItemButton() : base(new Grid())
        {
            _overlay = new Rectangle
            {
                Fill = GetBrush("ApplicationPageBackgroundThemeBrush", new SolidColorBrush(Colors.Black)),
                Opacity = 0,
                Visibility = Visibility.Collapsed
            };
            _overlay.Tapped += (s, e) =>
            {
                e.Handled = true;
                Collapse();
            };
            View.Children.Add(_overlay);

            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 16, 16)
            };
            View.Children.Add(stack);

            _holidaySection = CreateSection(R.S("String_AddHoliday"), () => VxView?.AddHoliday?.Invoke());
            _eventSection = CreateSection(R.S("String_AddEvent"), () => VxView?.AddEvent?.Invoke());
            stack.Children.Add(_holidaySection.Panel);
            stack.Children.Add(_eventSection.Panel);

            _taskLabel = CreateLabel(R.S("String_AddTask"));
            _taskLabel.Opacity = 0;
            _taskLabel.Visibility = Visibility.Collapsed;
            _taskLabel.Tapped += (s, e) =>
            {
                e.Handled = true;
                RequestAddTask();
            };

            _addButton = CreateButton(MainSize);
            _addButton.Click += (s, e) =>
            {
                if (_isExpanded)
                {
                    RequestAddTask();
                }
                else
                {
                    Expand();
                }
            };

            var taskPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            taskPanel.Children.Add(_taskLabel);
            taskPanel.Children.Add(_addButton);
            stack.Children.Add(taskPanel);
        }

        protected override void ApplyProperties(FloatingAddItemButton oldView, FloatingAddItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            _supportsAddHoliday = newView.AddHoliday != null;
        }

        private Section CreateSection(string text, Action invoke)
        {
            var label = CreateLabel(text);
            label.Tapped += (s, e) =>
            {
                e.Handled = true;
                invoke();
                Collapse();
            };

            var button = CreateButton(MiniSize);
            button.Click += (s, e) =>
            {
                invoke();
                Collapse();
            };

            var transform = new TranslateTransform { Y = SlideDistance };
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, (MainSize - MiniSize) / 2, 12),
                Opacity = 0,
                Visibility = Visibility.Collapsed,
                RenderTransform = transform
            };
            panel.Children.Add(label);
            panel.Children.Add(button);

            return new Section { Panel = panel, Transform = transform };
        }

        private static TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                FontWeight = Windows.UI.Text.FontWeights.SemiBold
            };
        }

        private static Button CreateButton(double size)
        {
            var accentColors = ThemeColorGenerator.Generate(Vx.Views.Theme.Current.AccentColor);
            var normal = Vx.Views.Theme.Current.AccentColor.ToUwpBrush();
            var hover = accentColors.PrimaryHover.ToUwpBrush();
            var pressed = accentColors.PrimaryDark.ToUwpBrush();
            var white = new SolidColorBrush(Colors.White);

            var button = new Button
            {
                Width = size,
                Height = size,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(size / 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                Content = new FontIcon
                {
                    FontFamily = UwpFontIcon.MaterialIconFontFamily,
                    Glyph = MaterialDesign.MaterialDesignIcons.Add,
                    FontSize = size == Vx.Views.FloatingActionButton.DefaultSize ? Vx.Views.FloatingActionButton.FontSize : size / 2.5
                }
            };

            // Override the default button visual state brushes
            button.Resources["ButtonBackground"] = normal;
            button.Resources["ButtonBackgroundPointerOver"] = hover;
            button.Resources["ButtonBackgroundPressed"] = pressed;
            button.Resources["ButtonForeground"] = white;
            button.Resources["ButtonForegroundPointerOver"] = white;
            button.Resources["ButtonForegroundPressed"] = white;
            button.Resources["ButtonBorderBrush"] = normal;
            button.Resources["ButtonBorderBrushPointerOver"] = hover;
            button.Resources["ButtonBorderBrushPressed"] = pressed;

            return button;
        }

        // Shared app brushes get mutated on theme change, so reusing them keeps colors current
        private static Brush GetBrush(string key, Brush fallback)
        {
            return Application.Current.Resources.TryGetValue(key, out object value) && value is Brush brush ? brush : fallback;
        }

        private void RequestAddTask()
        {
            VxView?.AddTask?.Invoke();
            Collapse();
        }

        private void Expand()
        {
            if (_isExpanded)
            {
                return;
            }

            _isExpanded = true;

            var sb = new Storyboard();

            _overlay.Visibility = Visibility.Visible;
            AddAnimation(sb, _overlay, "Opacity", OverlayOpacity);

            _taskLabel.Visibility = Visibility.Visible;
            AddAnimation(sb, _taskLabel, "Opacity", 1);

            ShowSection(sb, _eventSection);
            if (_supportsAddHoliday)
            {
                ShowSection(sb, _holidaySection);
            }

            Begin(sb);
        }

        private void Collapse()
        {
            if (!_isExpanded)
            {
                return;
            }

            _isExpanded = false;

            var sb = new Storyboard();

            AddAnimation(sb, _overlay, "Opacity", 0);
            AddAnimation(sb, _taskLabel, "Opacity", 0);
            HideSection(sb, _eventSection);
            HideSection(sb, _holidaySection);

            Begin(sb);
        }

        private static void ShowSection(Storyboard sb, Section section)
        {
            section.Panel.Visibility = Visibility.Visible;
            AddAnimation(sb, section.Panel, "Opacity", 1);
            AddAnimation(sb, section.Transform, "Y", 0);
        }

        private static void HideSection(Storyboard sb, Section section)
        {
            AddAnimation(sb, section.Panel, "Opacity", 0);
            AddAnimation(sb, section.Transform, "Y", SlideDistance);
        }

        private static void AddAnimation(Storyboard sb, DependencyObject target, string property, double to)
        {
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = AnimationDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);
            sb.Children.Add(animation);
        }

        private void Begin(Storyboard sb)
        {
            _storyboard = sb;
            sb.Completed += (s, e) =>
            {
                // Only the latest storyboard commits state; an interrupted one is discarded
                if (sb == _storyboard)
                {
                    ApplyFinalState(_isExpanded);
                }
                sb.Stop();
            };
            sb.Begin();
        }

        private void ApplyFinalState(bool expanded)
        {
            _overlay.Opacity = expanded ? OverlayOpacity : 0;
            _overlay.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;

            _taskLabel.Opacity = expanded ? 1 : 0;
            _taskLabel.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;

            ApplySectionFinalState(_eventSection, expanded);
            ApplySectionFinalState(_holidaySection, expanded && _supportsAddHoliday);
        }

        private static void ApplySectionFinalState(Section section, bool visible)
        {
            section.Panel.Opacity = visible ? 1 : 0;
            section.Panel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            section.Transform.Y = visible ? 0 : SlideDistance;
        }
    }
}
