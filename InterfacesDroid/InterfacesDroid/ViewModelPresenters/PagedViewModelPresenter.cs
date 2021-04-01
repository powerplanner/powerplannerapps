using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using InterfacesDroid.Helpers;
using System.ComponentModel;
using ToolsPortable;

namespace InterfacesDroid.ViewModelPresenters
{
    public class PagedViewModelPresenter : FrameLayout
    {
        public event EventHandler ContentChanged;
        private PropertyChangedEventHandler _viewModelPropertyChangedHandler;

        public PagedViewModelPresenter(Context context) : base(context)
        {
            _viewModelPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(_viewModel_PropertyChanged).Handler;
        }

        private PagedViewModel _viewModel;
        public PagedViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel == value)
                {
                    return;
                }

                // De-register old
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= _viewModelPropertyChangedHandler;
                }

                _viewModel = value;

                // Register new
                if (value != null)
                {
                    _viewModel.PropertyChanged += _viewModelPropertyChangedHandler;
                }

                UpdateContent();
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Content":
                    UpdateContent();
                    break;
            }
        }

        private void UpdateContent()
        {
            KeyboardHelper.HideKeyboard(this);

            // Remove previous content
            base.RemoveAllViews();

            if (ViewModel?.Content != null)
            {
                // Create and set new content
                var view = ViewModelToViewConverter.Convert(this, ViewModel.Content);
                base.AddView(view);
            }

            else if (ViewModel != null)
            {
                var splashView = ViewModelToViewConverter.GetSplash(this, ViewModel);
                if (splashView != null)
                {
                    base.AddView(splashView);
                }
            }

            ContentChanged?.Invoke(this, new EventArgs());
        }

        public View CurrentView
        {
            get
            {
                if (base.ChildCount == 0)
                    return null;

                return base.GetChildAt(base.ChildCount - 1);
            }
        }
    }
}