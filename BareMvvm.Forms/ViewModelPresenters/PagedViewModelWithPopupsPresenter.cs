using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BareMvvm.Forms.ViewModelPresenters
{
    public class PagedViewModelWithPopupsPresenter : Grid
    {
        public PagedViewModelWithPopupsPresenter()
        {
            var pagedPresenter = new PagedViewModelPresenter();
            pagedPresenter.SetBinding(PagedViewModelPresenter.ViewModelProperty, new Binding()
            {
                Path = nameof(ViewModel),
                Source = this
            });

            var popupsPresenter = new PopupsPresenter();
            popupsPresenter.SetBinding(PopupsPresenter.ViewModelProperty, new Binding()
            {
                Path = nameof(ViewModel),
                Source = this
            });

            Children.Add(pagedPresenter);
            Children.Add(popupsPresenter);
        }

        public PagedViewModelWithPopups ViewModel
        {
            get { return (PagedViewModelWithPopups)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(PagedViewModelWithPopups), typeof(PagedViewModelWithPopupsPresenter), null);
    }
}
