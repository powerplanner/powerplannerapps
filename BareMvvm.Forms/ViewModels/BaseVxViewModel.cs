using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Vx.Views;
using Xamarin.Forms;

namespace BareMvvm.Forms.ViewModels
{
    public abstract class BaseVxViewModel : FormsViewViewModel
    {
        public BaseVxViewModel(BaseViewModel parent) : base(parent)
        {
            View = new VxViewModelComponent(this, Render)
            {
                IsRootComponent = true
            };
        }

        private class VxViewModelComponent : VxComponent
        {
            private Func<View> _render;

            public VxViewModelComponent(BaseVxViewModel viewModel, Func<View> render)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                _render = render;
            }

            private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                MarkDirty();
            }

            protected override View Render()
            {
                return _render();
            }
        }

        protected abstract View Render();

        protected ICommand CreateCommand(Action action)
        {
            return new VxCommand(action);
        }

        private class VxCommand : ICommand
        {
            private Action _action;
            public VxCommand(Action action)
            {
                _action = action;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _action();
            }
        }
    }
}
