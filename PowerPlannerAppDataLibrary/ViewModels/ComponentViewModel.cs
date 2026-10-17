using System;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels
{
    public class ComponentViewModel : BaseMainScreenViewModelDescendant
    {
        private string _title;
        public string Title
        {
            get => _title;
            protected set => SetProperty(ref _title, value, nameof(Title));
        }

        private Thickness _platformNookInsets;

        /// <summary>
        /// Raw insets reported by the platform view, for view models that cover the whole screen (popups).
        /// </summary>
        protected Thickness PlatformNookInsets => _platformNookInsets;

        /// <summary>
        /// Insets for main screen content, which exclude edges already handled by the sidebar or bottom nav bar.
        /// </summary>
        public virtual Thickness NookInsets => MainScreenViewModel.ContentNookInsets;

        internal void RefreshNookInsets()
        {
            MarkDirty();
        }

        public ComponentViewModel(BaseViewModel parent) : base(parent)
        {
            base.PropertyChanged += ComponentViewModel_PropertyChanged;
        }

        private void ComponentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MarkDirty();
        }

        public void UpdateNookInsets(Thickness newInsets)
        {
            if (_platformNookInsets != newInsets)
            {
                _platformNookInsets = newInsets;
                MarkDirty();
            }
        }
    }
}
