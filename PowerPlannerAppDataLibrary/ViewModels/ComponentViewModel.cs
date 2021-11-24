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

        public Thickness NookInsets { get; protected set; }

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
            if (NookInsets != newInsets)
            {
                NookInsets = newInsets;
                MarkDirty();
            }
        }
    }
}
