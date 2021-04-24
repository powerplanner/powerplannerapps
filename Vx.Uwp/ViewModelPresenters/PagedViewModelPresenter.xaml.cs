using System;
using System.Collections.Generic;
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
        public PagedViewModelPresenter()
        {
            this.InitializeComponent();

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.VerticalContentAlignment = VerticalAlignment.Stretch;

            SetBinding(ContentProperty, new Binding()
            {
                Path = new PropertyPath(nameof(ViewModel) + "." + nameof(ViewModel.Content)),
                Source = this,
                Converter = new ViewModelToViewConverter()
            });
        }

        public PagedViewModel ViewModel
        {
            get { return (PagedViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PagedViewModel), typeof(PagedViewModelPresenter), new PropertyMetadata(null));
    }
}
