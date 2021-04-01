using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BareMvvm.Forms.ViewModelPresenters
{
    public class GenericViewModelPresenter : ContentView
    {
        public GenericViewModelPresenter()
        {
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;

            SetBinding(ContentProperty, new Binding()
            {
                Path = nameof(ViewModel),
                Source = this,
                Converter = new ViewModelToViewConverter()
            });
        }

        public BaseViewModel ViewModel
        {
            get { return (BaseViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(BaseViewModel), typeof(GenericViewModelPresenter), null);
    }
}
