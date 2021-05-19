using InterfacesUWP;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerUWP.Views
{
    public class PopupViewHostGeneric : ViewHostGeneric
    {
        private const double TOP_BAR_HEIGHT = 48;

        private Grid _content;

        private Border _mainContentContainer;

        private Border _secondaryOptionsButtonContainer;
        protected FrameworkElement SecondaryOptionsButtonContainer => _secondaryOptionsButtonContainer;
        private Border _buttonCloseContainer;

        private Grid _topTitleBar;
        private TextBlock _tbTitle;
        private StackPanel _topPrimaryCommands;

        private Border _fullScreenTitle;
        private TextBlock _tbFullScreenTitle;

        private CommandBar _commandBar;

        public PopupViewHostGeneric()
        {
            // Only on Mobile animate
            // Disabled animations for now since they briefly show the base content while animating between popups
            // (like when going from ViewTaskOrEvent to EditTaskOrEvent
            //if (IsFullScreenMode())
            //{
            //    this.Transitions = new TransitionCollection();
            //    this.Transitions.Add(new EntranceThemeTransition());
            //}

            PrimaryCommands = new ObservableCollection<AppBarButton>();
            PrimaryCommands.CollectionChanged += PrimaryCommands_CollectionChanged;

            SecondaryCommands = new ObservableCollection<AppBarButton>();
            SecondaryCommands.CollectionChanged += SecondaryCommands_CollectionChanged;

            _mainContentContainer = new Border();
            Grid.SetRow(_mainContentContainer, 1);

            _topTitleBar = new Grid()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }, // Title
                    new ColumnDefinition() { Width = GridLength.Auto }, // Secondary menu button
                    new ColumnDefinition() { Width = GridLength.Auto }, // Primary buttons
                    new ColumnDefinition() { Width = GridLength.Auto } // Close button
                },

                RequestedTheme = ElementTheme.Dark
            };

            // Title
            TextBlock tbTitle = new TextBlock()
            {
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Border borderTitle = new Border()
            {
                Child = tbTitle,
                Background = Application.Current.Resources["PowerPlannerBlue"] as Brush,
                Padding = new Thickness(16, 0, 0, 0)
            };
            _topTitleBar.Children.Add(borderTitle);
            _tbTitle = tbTitle;


            // Full screen title
            TextBlock tbFullScreenTitle = new TextBlock()
            {
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            _fullScreenTitle = new Border()
            {
                Child = tbFullScreenTitle,
                Background = Application.Current.Resources["PowerPlannerBlue"] as Brush,
                Padding = new Thickness(16, 12, 0, 12),
                Visibility = Visibility.Collapsed,
                RequestedTheme = ElementTheme.Dark
            };
            _tbFullScreenTitle = tbFullScreenTitle;


            // Secondary menu button
            AppBarButton secondaryOptionsButton = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.More),
                IsCompact = true
            };
            ToolTipService.SetToolTip(secondaryOptionsButton, LocalizedResources.GetString("String_MoreOptions"));
            secondaryOptionsButton.Click += SecondaryOptionsButton_Click;
            _secondaryOptionsButtonContainer = CreateBorderForAppBarButton(secondaryOptionsButton);
            _secondaryOptionsButtonContainer.Visibility = Visibility.Collapsed;
            Grid.SetColumn(_secondaryOptionsButtonContainer, 1);
            _topTitleBar.Children.Add(_secondaryOptionsButtonContainer);



            // Primary buttons
            _topPrimaryCommands = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Grid.SetColumn(_topPrimaryCommands, 2);
            _topTitleBar.Children.Add(_topPrimaryCommands);



            // Close button
            AppBarButton buttonClose = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Cancel),
                IsCompact = true,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            ToolTipService.SetToolTip(buttonClose, LocalizedResources.Common.GetStringClose());
            buttonClose.Click += ButtonClose_Click;
            _buttonCloseContainer = CreateBorderForAppBarButton(buttonClose);
            Grid.SetColumn(_buttonCloseContainer, 3);
            _topTitleBar.Children.Add(_buttonCloseContainer);



            _content = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition() { Height = GridLength.Auto }
                },

                Children =
                {
                    _fullScreenTitle,
                    _topTitleBar,
                    _mainContentContainer
                },

                Background = Application.Current.Resources["PopupBackground"] as Brush
            };


            base.Content = _content;

            UpdateAll();
        }

        private bool _showTitleAndCommands = true;
        public bool ShowTitleAndCommands
        {
            get { return _showTitleAndCommands; }
            set
            {
                _showTitleAndCommands = value;

                UpdateAll();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close the window/go back
                ViewModel.TryRemoveViewModelViaUserInteraction();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private Flyout _cachedSecondaryFlyout;

        private void SecondaryOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cachedSecondaryFlyout == null)
            {
                _cachedSecondaryFlyout = new Flyout();
                StackPanel sp = new StackPanel();
                _cachedSecondaryFlyout.Content = sp;

                foreach (var c in SecondaryCommands)
                {
                    c.Style = (Style)Application.Current.Resources["MyOverflowAppBarStyle"];
                    c.Click -= SecondaryCommandBarButton_Click;
                    c.Click += SecondaryCommandBarButton_Click;
                    sp.Children.Add(c);
                }
            }

            _cachedSecondaryFlyout.ShowAt(_secondaryOptionsButtonContainer);
        }

        private void SecondaryCommandBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cachedSecondaryFlyout != null)
                _cachedSecondaryFlyout.Hide();
        }

        private void DestroyCachedSecondaryFlyout()
        {
            if (_cachedSecondaryFlyout == null)
                return;

            StackPanel sp = (StackPanel)_cachedSecondaryFlyout.Content;

            // Clear all the children so the app bar buttons can be added elsewhere
            sp.Children.Clear();

            _cachedSecondaryFlyout = null;
        }

        private Border CreateBorderForAppBarButton(AppBarButton button)
        {
            // When NOT in full screen mode, remove the labels.
            // Otherwise, 1 out of 10 times they accidently appear for some reason
            // in the Anniversary Update of Windows 10.
            if (!IsFullScreenMode())
            {
                if (!string.IsNullOrWhiteSpace(button.Label))
                {
                    button.Tag = button.Label;
                    button.Label = "";
                }
            }
            else
            {
                // If we previously removed the label, restore it since we're no longer in full screen mode
                if (string.IsNullOrWhiteSpace(button.Label) && button.Tag is string)
                {
                    button.Label = button.Tag as string;
                }
            }

            return new Border()
            {
                Child = button,
                Margin = new Thickness(2, 0, 0, 0),
                Background = Application.Current.Resources["PowerPlannerBlue"] as Brush,
                Height = TOP_BAR_HEIGHT,
                Width = 68
            };
        }

        private void SecondaryCommands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsFullScreenMode())
            {
                if (_commandBar == null)
                    throw new NullReferenceException("_commandBar canont be null");

                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:


                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        {
                            foreach (AppBarButton b in e.OldItems.OfType<AppBarButton>())
                            {
                                _commandBar.SecondaryCommands.Remove(b);
                            }
                        }

                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        {
                            int indexToInsert = e.NewStartingIndex;
                            foreach (AppBarButton b in e.NewItems.OfType<AppBarButton>())
                            {
                                _commandBar.SecondaryCommands.Insert(indexToInsert, b);

                                indexToInsert++;
                            }
                        }

                        break;



                    default:

                        _commandBar.SecondaryCommands.Clear();

                        foreach (var c in SecondaryCommands)
                        {
                            _commandBar.SecondaryCommands.Add(c);
                        }

                        break;
                }

                UpdateBottomAppBar();
            }

            else
            {
                _secondaryOptionsButtonContainer.Visibility = SecondaryCommands.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                DestroyCachedSecondaryFlyout();
            }
        }

        private void PrimaryCommands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (AppBarButton a in e.NewItems.OfType<AppBarButton>())
                {
                    if (a.Label != null)
                        ToolTipService.SetToolTip(a, a.Label);
                }


            if (IsFullScreenMode())
            {
                if (_commandBar == null)
                    throw new NullReferenceException("_commandBar canont be null");

                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:


                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        {
                            foreach (AppBarButton b in e.OldItems.OfType<AppBarButton>())
                            {
                                _commandBar.PrimaryCommands.Remove(b);
                            }
                        }

                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        {
                            int indexToInsert = e.NewStartingIndex;
                            foreach (AppBarButton b in e.NewItems.OfType<AppBarButton>())
                            {
                                _commandBar.PrimaryCommands.Insert(indexToInsert, b);

                                indexToInsert++;
                            }
                        }

                        break;



                    default:

                        _commandBar.PrimaryCommands.Clear();

                        foreach (var c in PrimaryCommands)
                        {
                            _commandBar.PrimaryCommands.Add(c);
                        }

                        break;
                }

                UpdateBottomAppBar();
            }

            else
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:


                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        {
                            foreach (AppBarButton b in e.OldItems.OfType<AppBarButton>())
                            {
                                var container = _topPrimaryCommands.Children.OfType<Border>().FirstOrDefault(i => i.Child == b);

                                if (container != null)
                                {
                                    container.Child = null;
                                    _topPrimaryCommands.Children.Remove(container);
                                }
                            }
                        }

                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        {
                            int indexToInsert = e.NewStartingIndex;
                            foreach (AppBarButton b in e.NewItems.OfType<AppBarButton>())
                            {
                                b.IsCompact = true;

                                var container = CreateBorderForAppBarButton(b);

                                _topPrimaryCommands.Children.Insert(indexToInsert, container);

                                indexToInsert++;
                            }
                        }

                        break;



                    default:

                        foreach (var b in _topPrimaryCommands.Children.OfType<Border>())
                            b.Child = null;

                        _topPrimaryCommands.Children.Clear();

                        foreach (var c in PrimaryCommands)
                        {
                            c.IsCompact = true;
                            _topPrimaryCommands.Children.Add(CreateBorderForAppBarButton(c));
                        }

                        break;
                }
            }
        }

        public string Title
        {
            get => _tbTitle.Text;
            set
            {
                if (value == null)
                {
                    value = "";
                }

                _tbTitle.Text = value;
                _tbFullScreenTitle.Text = value;
            }
        }

        public ObservableCollection<AppBarButton> PrimaryCommands { get; }

        public ObservableCollection<AppBarButton> SecondaryCommands { get; }

        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register("MainContent", typeof(UIElement), typeof(PopupViewHostGeneric), new PropertyMetadata(null, OnMainContentChanged));

        private static void OnMainContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PopupViewHostGeneric).OnMainContentChanged(e);
        }

        private void OnMainContentChanged(DependencyPropertyChangedEventArgs e)
        {
            _mainContentContainer.Child = e.NewValue as UIElement;
        }

        public UIElement MainContent
        {
            get { return GetValue(MainContentProperty) as UIElement; }
            set { SetValue(MainContentProperty, value); }
        }

        private bool IsFullScreenMode()
        {
            switch (DeviceInfo.GetCurrentDeviceFormFactor())
            {
                case DeviceFormFactor.Mobile:
                    return true;

                default:
                    return false;
            }
        }

        public static readonly DependencyProperty MaxWindowSizeProperty = DependencyProperty.Register("MaxWindowSize", typeof(Size), typeof(PopupViewHostGeneric), new PropertyMetadata(new Size(550, 700), OnMaxWindowSizeChanged));

        public Size MaxWindowSize
        {
            get { return (Size)GetValue(MaxWindowSizeProperty); }
            set { SetValue(MaxWindowSizeProperty, value); }
        }

        private static void OnMaxWindowSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PopupViewHostGeneric).OnMaxWindowSizeChanged(e);
        }

        private void OnMaxWindowSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateMaxWindowSize();
        }

        private void UpdateMaxWindowSize()
        {
            if (IsFullScreenMode())
            {
                base.VerticalAlignment = VerticalAlignment.Stretch;
                base.MaxWidth = double.MaxValue;
                base.MaxHeight = double.MaxValue;
                base.Margin = new Thickness();
            }

            else
            {
                UpdateMaxWindowSizeForNonFullScreen();
            }
        }

        protected virtual void UpdateMaxWindowSizeForNonFullScreen()
        {
            Size maxWindowSize = MaxWindowSize;

            base.VerticalAlignment = VerticalAlignment.Center;
            base.MaxWidth = maxWindowSize.Width;
            base.MaxHeight = maxWindowSize.Height;
            base.Margin = new Thickness(24);
        }

        private void UpdateAll()
        {
            if (!ShowTitleAndCommands)
            {
                _topTitleBar.Visibility = Visibility.Collapsed;
                _fullScreenTitle.Visibility = Visibility.Collapsed;
            }

            else
            {
                if (IsFullScreenMode())
                {
                    if (_topTitleBar != null)
                        _topTitleBar.Visibility = Visibility.Collapsed;

                    _fullScreenTitle.Visibility = Visibility.Visible;
                }

                else
                {
                    if (_fullScreenTitle != null)
                        _fullScreenTitle.Visibility = Visibility.Collapsed;

                    _topTitleBar.Visibility = Visibility.Visible;
                }
            }

            UpdateBottomAppBar();
            UpdateMaxWindowSize();
        }

        private void UpdateBottomAppBar()
        {
            if (IsFullScreenMode() && ShowTitleAndCommands)
            {
                if (_commandBar == null)
                {
                    _commandBar = new CommandBar()
                    {
                        Style = (Style)Application.Current.Resources["PowerPlannerCommandBarStyle"]
                    };

                    Grid.SetRow(_commandBar, 2);
                    _content.Children.Add(_commandBar);
                }

                if (PrimaryCommands.Count > 0 || SecondaryCommands.Count > 0)
                    _commandBar.Visibility = Visibility.Visible;
                else
                    _commandBar.Visibility = Visibility.Collapsed;
            }

            else
            {
                if (_commandBar != null)
                {
                    _commandBar.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
