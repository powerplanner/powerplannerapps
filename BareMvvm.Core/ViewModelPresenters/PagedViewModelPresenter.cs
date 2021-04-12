using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BareMvvm.Core.ViewModelPresenters
{
    public class PagedViewModelPresenter : ContentView
    {
        public PagedViewModelPresenter()
        {
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;

            SetBinding(ContentProperty, new Xamarin.Forms.Binding()
            {
                Path = nameof(ViewModel) + "." + nameof(ViewModel.Content),
                Source = this,
                Converter = new ViewModelToViewConverter()
            });
        }

        public PagedViewModel ViewModel
        {
            get { return (PagedViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(PagedViewModel), typeof(PagedViewModelPresenter), null);
    }
}
