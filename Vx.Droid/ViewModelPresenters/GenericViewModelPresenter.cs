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
    public class GenericViewModelPresenter : FrameLayout, IGetSnackbarAnchorView
    {
        public GenericViewModelPresenter(Context context) : base(context)
        {
            Clickable = true;
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

        public virtual View GetSnackbarAnchorView()
        {
            if (base.ChildCount > 0)
            {
                return GetSnackbarAnchorView(base.GetChildAt(0));
            }

            return null;
        }

        public static View GetSnackbarAnchorView(View childView)
        {
            if (childView is IGetSnackbarAnchorView getAnchorView)
            {
                return getAnchorView.GetSnackbarAnchorView();
            }
            else if (childView is ViewGroup viewGroup)
            {
                var children = viewGroup.GetAllChildren().ToArray();
                foreach (var item in children.OfType<IGetSnackbarAnchorView>())
                {
                    return item.GetSnackbarAnchorView();
                }

                foreach (var item in children.Where(i => !(i is IGetSnackbarAnchorView)))
                {
                    var answer = GetSnackbarAnchorView(item);
                    if (answer != null)
                    {
                        return answer;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}