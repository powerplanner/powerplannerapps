using BareMvvm.Core.ViewModelPresenters;
using PowerPlannerAppDataLibrary.App;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerPlannerAppDataLibrary
{
    public partial class AppShell : Application
    {
        private GenericViewModelPresenter _windowContentPresenter;

        public AppShell()
        {
            InitializeComponent();

            _windowContentPresenter = new GenericViewModelPresenter();

            MainPage = new ContentPage()
            {
                Content = _windowContentPresenter
            };

            PowerPlannerApp.Current.Windows.CollectionChanged += Windows_CollectionChanged;

            UpdateWindowContent();
        }

        private void Windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateWindowContent();
        }

        private void UpdateWindowContent()
        {
            var window = PowerPlannerApp.Current.GetCurrentWindow();

            if (window == null)
            {
                _windowContentPresenter.RemoveBinding(GenericViewModelPresenter.ViewModelProperty);
            }
            else
            {
                _windowContentPresenter.SetBinding(GenericViewModelPresenter.ViewModelProperty, new Binding()
                {
                    Path = nameof(window.ViewModel),
                    Source = window
                });
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
