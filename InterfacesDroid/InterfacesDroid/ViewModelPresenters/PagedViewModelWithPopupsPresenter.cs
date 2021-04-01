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
using InterfacesDroid.Themes;
using InterfacesDroid.Helpers;
using ToolsPortable;
using Android.Util;

namespace InterfacesDroid.ViewModelPresenters
{
    public class PopupsPresenter : FrameLayout
    {
        private System.Collections.Specialized.NotifyCollectionChangedEventHandler _popupsCollectionChangedHandler;

        public PopupsPresenter(Context context) : base(context)
        {
            Initialize();
        }

        public PopupsPresenter(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            Visibility = ViewStates.Gone;
            _popupsCollectionChangedHandler = new WeakEventHandler<System.Collections.Specialized.NotifyCollectionChangedEventArgs>(Popups_CollectionChanged).Handler;
        }

        private PagedViewModelWithPopups _viewModel;
        public PagedViewModelWithPopups ViewModel
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
                    _viewModel.Popups.CollectionChanged -= _popupsCollectionChangedHandler;
                }

                _viewModel = value;

                // Listen to popup changes
                if (value != null)
                {
                    _viewModel.Popups.CollectionChanged += _popupsCollectionChangedHandler;
                }

                InitializePopups();
            }
        }

        private void Popups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            KeyboardHelper.HideKeyboard(this);

            InitializePopups();
        }

        public class ModelAndView
        {
            public BaseViewModel Model { get; private set; }
            public View View { get; private set; }

            public ModelAndView(BaseViewModel model, View view)
            {
                Model = model;
                View = view;
            }
        }

        private List<ModelAndView> _modelAndViews = new List<ModelAndView>();

        private void InitializePopups()
        {
            try
            {
                if (ViewModel == null || ViewModel.Popups == null || ViewModel.Popups.Count == 0)
                {
                    _modelAndViews.Clear();
                    this.RemoveAllViews();
                    return;
                }

                // Remove any that don't exist anymore
                for (int i = 0; i < _modelAndViews.Count; i++)
                {
                    if (!ViewModel.Popups.Contains(_modelAndViews[i].Model))
                    {
                        _modelAndViews.RemoveAt(i);
                        this.RemoveViewAt(i);
                        i--;
                    }
                }

                // Now we just have new or moved views

                for (int i = 0; i < ViewModel.Popups.Count; i++)
                {
                    var viewModel = ViewModel.Popups[i];

                    // If it's already there, good
                    if (i < _modelAndViews.Count && viewModel == _modelAndViews[i].Model)
                    {
                        continue;
                    }

                    // Otherwise it potentially needs to be moved forward
                    int indexOfExisting = _modelAndViews.FindIndex(x => x.Model == viewModel);
                    if (indexOfExisting != -1)
                    {
                        var existingModelAndView = _modelAndViews[indexOfExisting];
                        _modelAndViews.RemoveAt(indexOfExisting);
                        _modelAndViews.Insert(i, existingModelAndView);
                        this.RemoveViewAt(indexOfExisting);
                        this.AddView(existingModelAndView.View, i);
                        continue;
                    }

                    // Otherwise, it's new
                    var newModelAndView = new ModelAndView(viewModel, CreateViewFromPopup(viewModel));
                    _modelAndViews.Insert(i, newModelAndView);
                    this.AddView(newModelAndView.View, i);
                }
            }

#if DEBUG
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                throw ex;
            }
#endif

            finally
            {
                UpdatePopupContainerVisibility();
            }
        }

        private void UpdatePopupContainerVisibility()
        {
            this.Visibility = this.ChildCount > 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        private View CreateViewFromPopup(BaseViewModel viewModel)
        {
            var view = new GenericViewModelPresenter(Context)
            {
                ViewModel = viewModel
            };

            view.SetBackgroundColor(ThemeHelper.GetThemeColor(Context, Android.Resource.Attribute.WindowBackground));

            return view;
        }
    }

    public class PagedViewModelWithPopupsPresenter : FrameLayout
    {
        private PagedViewModelPresenter _pagedViewModelPresenter;
        private PopupsPresenter _popupsPresenter;

        public PagedViewModelWithPopupsPresenter(Context context) : base(context)
        {
            Initialize();
        }

        public PagedViewModelWithPopupsPresenter(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _pagedViewModelPresenter = new PagedViewModelPresenter(Context);
            base.AddView(_pagedViewModelPresenter);

            _popupsPresenter = new PopupsPresenter(Context);

            base.AddView(_popupsPresenter);
        }

        private PagedViewModelWithPopups _viewModel;
        public PagedViewModelWithPopups ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel == value)
                {
                    return;
                }
                
                _viewModel = value;

                // Set it on the paged too
                _pagedViewModelPresenter.ViewModel = value;

                // And popups presenter
                _popupsPresenter.ViewModel = value;
            }
        }

        
    }
}