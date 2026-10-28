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

        /// <summary>
        /// Insets for main screen content, which exclude edges already handled by the sidebar or bottom nav bar.
        /// </summary>
        public override Thickness NookInsets => MainScreenViewModel?.ContentNookInsets ?? new Thickness(0);

        /// <summary>
        /// Includes nook insets
        /// </summary>
        public virtual Thickness ContentMargins => MainScreenViewModel?.ContentMargins ?? new Thickness(Theme.Current.PageMargin);

        internal void RefreshContentNookInsetsAndMargins()
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
    }
}
