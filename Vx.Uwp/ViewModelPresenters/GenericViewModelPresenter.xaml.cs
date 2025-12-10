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
using WinRT;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP.ViewModelPresenters
{
    public sealed partial class GenericViewModelPresenter : UserControl
    {
        private static ViewModelToViewConverter _viewModelToViewConverter = new ViewModelToViewConverter();

        public GenericViewModelPresenter()
        {
            this.InitializeComponent();

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        public BaseViewModel ViewModel
        {
            get { return (BaseViewModel)GetValue(ViewModelProperty); }
            set
            {
                Content = _viewModelToViewConverter.Convert(value, typeof(UIElement), null, null) as UIElement;
                SetValue(ViewModelProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(BaseViewModel), typeof(GenericViewModelPresenter), new PropertyMetadata(null));
    }
}
