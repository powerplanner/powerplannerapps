using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes
{
    public class ClassesViewModel : BaseMainScreenViewModelChild
    {
        private bool _hasClasses;
        public bool HasClasses
        {
            get => _hasClasses;
            set => SetProperty(ref _hasClasses, value, nameof(HasClasses));
        }

        public ClassesViewModel(BaseViewModel parent) : base(parent)
        {
            MainScreenViewModel.Classes.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Classes_CollectionChanged).Handler;
        }

        private void Classes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasClasses = MainScreenViewModel.Classes.Any();
        }

        public void AddClass()
        {
            MainScreenViewModel.AddClass();
        }

        public void OpenClass(ViewItemClass viewItemClass)
        {
            MainScreenViewModel.OpenClass(viewItemClass);
        }
    }
}
