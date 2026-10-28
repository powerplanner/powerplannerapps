using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using InterfacesDroid.Views;

namespace InterfacesDroid.ViewModelPresenters
{
    public class GenericViewModelPresenter : FrameLayout
    {
        public GenericViewModelPresenter(Context context) : base(context)
        {
        }

        private BaseViewModel _viewModel;
        public BaseViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel == value)
                {
                    return;
                }

                _viewModel = value;

                UpdateContent();
            }
        }

        private void UpdateContent()
        {
            // Remove previous content
            base.RemoveAllViews();

            if (ViewModel != null)
            {
                // Create and set new content
                var view = ViewModelToViewConverter.Convert(this, ViewModel);
                base.AddView(view);
            }
        }
    }
}