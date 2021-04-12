using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using Xamarin.Forms;

namespace BareMvvm.Core.Views
{
    public class ViewModelView : ContentView
    {
        public BaseViewModel ViewModel
        {
            get { return BindingContext as BaseViewModel; }
            set { BindingContext = value; OnViewModelSet(); TriggerViewModelLoaded(value); }
        }

        private void OnViewModelSet()
        {
            ViewModel.ViewFocused += new WeakEventHandler<EventArgs>(ViewModel_ViewFocused).Handler;
            ViewModel.ViewLostFocus += new WeakEventHandler<EventArgs>(ViewModel_ViewLostFocus).Handler;

            if (ViewModel.IsFocused)
            {
                OnViewFocused();
            }

            OnViewModelSetOverride();
        }

        private void ViewModel_ViewLostFocus(object sender, EventArgs e)
        {
            OnViewLostFocus();
        }

        private void ViewModel_ViewFocused(object sender, EventArgs e)
        {
            OnViewFocused();
        }

        public virtual void OnViewModelSetOverride()
        {
            // Nothing
        }

        public virtual void OnViewModelLoadedOverride()
        {
            // Nothing
        }

        private async void TriggerViewModelLoaded(BaseViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync();
            }
            catch { return; }

            OnViewModelLoadedOverride();
        }

        protected virtual void OnViewFocused()
        {

        }

        protected virtual void OnViewLostFocus()
        {

        }
    }
}
