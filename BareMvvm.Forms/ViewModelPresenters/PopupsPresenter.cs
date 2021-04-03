using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ToolsPortable;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace BareMvvm.Forms.ViewModelPresenters
{
    public class PopupsPresenter : Grid
    {
        private ContentView _popupsPresenter;

        public PopupsPresenter()
        {
            var backdropTap = new TapGestureRecognizer();
            backdropTap.Tapped += BackdropTap_Tapped;

            var backdrop = new BoxView()
            {
                BackgroundColor = new Color(0, 0, 0, 0.3),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                GestureRecognizers =
                {
                    backdropTap
                }
            };

            Children.Add(backdrop);

            // TODO: Should create an ItemsControl and have this keep the previously created views just like UWP does so when closing one popup we don't have to re-create the prev view
            _popupsPresenter = new ContentView();

            Children.Add(_popupsPresenter);
        }

        public PagedViewModelWithPopups ViewModel
        {
            get { return (PagedViewModelWithPopups)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PagedViewModelWithPopups), typeof(PopupsPresenter), null, propertyChanged: ViewModelPropertyChanged);

        private static void ViewModelPropertyChanged(BindableObject sender, object oldVal, object newVal)
        {
            (sender as PopupsPresenter).ViewModelPropertyChanged(oldVal as PagedViewModelWithPopups, newVal as PagedViewModelWithPopups);
        }

        private WeakEventHandler<NotifyCollectionChangedEventArgs> _handler;
        private void ViewModelPropertyChanged(PagedViewModelWithPopups oldVal, PagedViewModelWithPopups newVal)
        {
            if (oldVal != null)
            {
                if (_handler != null)
                {
                    oldVal.Popups.CollectionChanged -= _handler.Handler;
                }
            }

            if (newVal != null)
            {
                if (_handler == null)
                {
                    _handler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Popups_CollectionChanged);
                }

                newVal.Popups.CollectionChanged += _handler.Handler;
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
                base.IsVisible = false;
            }
            else
            {
                base.IsVisible = true;
                _popupsPresenter.Content = new GenericViewModelPresenter()
                {
                    ViewModel = ViewModel.Popups.Last()
                };
            }
        }

        private void BackdropTap_Tapped(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.TryClearPopupsViaUserInteraction();
            }
        }
    }
}
