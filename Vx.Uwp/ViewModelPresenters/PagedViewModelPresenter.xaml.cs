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
    public sealed partial class PagedViewModelPresenter : UserControl
    {
        private ViewModelToViewConverter _viewModelToViewConverter = new ViewModelToViewConverter();
        private PagedViewModel _subscribedViewModel;

        public PagedViewModelPresenter()
        {
            this.InitializeComponent();

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        public PagedViewModel ViewModel
        {
            get { return (PagedViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PagedViewModel), typeof(PagedViewModelPresenter), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PagedViewModelPresenter).OnViewModelChanged(e.OldValue as PagedViewModel, e.NewValue as PagedViewModel);
        }

        private void OnViewModelChanged(PagedViewModel oldViewModel, PagedViewModel newViewModel)
        {
            if (oldViewModel != null)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _subscribedViewModel = newViewModel;

            if (newViewModel != null)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            UpdateContent();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PagedViewModel.Content))
            {
                UpdateContent();
            }
        }

        private void UpdateContent()
        {
            Content = _subscribedViewModel?.Content == null
                ? null
                : _viewModelToViewConverter.Convert(_subscribedViewModel.Content, typeof(UIElement), null, null) as UIElement;
        }
    }
}
