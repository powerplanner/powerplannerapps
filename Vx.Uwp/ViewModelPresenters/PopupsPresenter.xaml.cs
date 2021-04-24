using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InterfacesUWP.ViewModelPresenters
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PopupsPresenter : UserControl
    {
        public PopupsPresenter()
        {
            this.InitializeComponent();

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Changed;
            inputPane.Hiding += InputPane_Changed;

            UpdateInputPaneOcclusion();
        }

        public PagedViewModelWithPopups ViewModel
        {
            get { return (PagedViewModelWithPopups)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PagedViewModelWithPopups), typeof(PopupsPresenter), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PopupsPresenter).OnViewModelChanged(e);
        }

        private WeakEventHandler<NotifyCollectionChangedEventArgs> _handler;
        private void OnViewModelChanged(DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.OldValue as PagedViewModelWithPopups;
            if (viewModel != null)
            {
                if (_handler != null)
                {
                    viewModel.Popups.CollectionChanged -= _handler.Handler;
                }
            }

            _handler = null;

            viewModel = e.NewValue as PagedViewModelWithPopups;
            if (viewModel != null)
            {
                _handler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Popups_CollectionChanged);
                viewModel.Popups.CollectionChanged += _handler.Handler;
            }

            UpdateVisibility();
        }

        private void Popups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (ViewModel == null || ViewModel.Popups.Count == 0)
            {
                base.Visibility = Visibility.Collapsed;
            }
            else
            {
                base.Visibility = Visibility.Visible;
            }
        }

        private void RectangleBackdrop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.TryClearPopupsViaUserInteraction();
                e.Handled = true;
            }
        }

        private void InputPane_Changed(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            UpdateInputPaneOcclusion();
        }

        private void UpdateInputPaneOcclusion()
        {
            try
            {
                var inputPane = InputPane.GetForCurrentView();

                if (inputPane.OccludedRect.Height > 0)
                {
                    if (inputPane.OccludedRect.Top != 0)
                    {
                        KeyboardOcclusion.MinHeight = inputPane.OccludedRect.Height;
                        return;
                    }
                }

                // All other cases, act as if it's hidden
                KeyboardOcclusion.MinHeight = 0;
            }
            catch { }
        }
    }
}
