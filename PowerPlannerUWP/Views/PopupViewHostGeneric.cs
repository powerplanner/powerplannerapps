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
using Vx.Uwp.Views;
using Windows.UI;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels;
using PowerPlannerAppDataLibrary.Components;
using Vx.Uwp;

namespace PowerPlannerUWP.Views
{
    public partial class PopupViewHostGeneric : ViewHostGeneric
    {
        private PopupViewHostComponent _popupViewHostComponent = new PopupViewHostComponent();

        public PopupViewHostGeneric()
        {
            _popupViewHostComponent.OnClose = ButtonClose_Click;
            _popupViewHostComponent.PrimaryCommands = PrimaryCommands;
            _popupViewHostComponent.SecondaryCommands = SecondaryCommands;
            base.Content = _popupViewHostComponent.Render();

            UpdateMaxWindowSize();
        }

        private void ButtonClose_Click()
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

        public string Title
        {
            get => _popupViewHostComponent.Title;
            set => _popupViewHostComponent.Title = value;
        }

        public ObservableCollection<PopupCommand> PrimaryCommands { get; } = new ObservableCollection<PopupCommand>();

        public ObservableCollection<PopupCommand> SecondaryCommands { get; } = new ObservableCollection<PopupCommand>();

        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register("MainContent", typeof(UIElement), typeof(PopupViewHostGeneric), new PropertyMetadata(null, OnMainContentChanged));

        private static void OnMainContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PopupViewHostGeneric).OnMainContentChanged(e);
        }

        private void OnMainContentChanged(DependencyPropertyChangedEventArgs e)
        {
            _popupViewHostComponent.NativeContent = e.NewValue;
            _popupViewHostComponent.RenderOnDemand();
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
    }
}
