using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BareMvvm.Core.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP.ViewModelPresenters
{
    public sealed partial class PagedViewModelWithPopupsPresenter : UserControl
    {
        public PagedViewModelWithPopupsPresenter()
        {
            this.InitializeComponent();

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        public PagedViewModelWithPopups ViewModel
        {
            get { return (PagedViewModelWithPopups)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PagedViewModelWithPopups), typeof(PagedViewModelWithPopupsPresenter), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PagedViewModelWithPopupsPresenter).OnViewModelChanged(e.OldValue as PagedViewModelWithPopups, e.NewValue as PagedViewModelWithPopups);
        }

        private void OnViewModelChanged(PagedViewModelWithPopups oldViewModel, PagedViewModelWithPopups newViewModel)
        {
            if (oldViewModel != null)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (newViewModel != null)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            ThisPagedViewModelPresenter.ViewModel = newViewModel;
            ThisPopupsPresenter.ViewModel = newViewModel;
            UpdateFullScreenPopup(newViewModel);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PagedViewModelWithPopups.FullScreenPopup))
            {
                UpdateFullScreenPopup(sender as PagedViewModelWithPopups);
            }
        }

        private void UpdateFullScreenPopup(PagedViewModelWithPopups viewModel)
        {
            ThisFullScreenPopupPresenter.ViewModel = viewModel?.FullScreenPopup;
            ThisFullScreenPopupPresenter.Visibility = viewModel?.FullScreenPopup == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
