using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public abstract class BaseMainScreenViewModelChild : BaseMainScreenViewModelDescendant
    {
        public BaseMainScreenViewModelChild(BaseViewModel parent) : base(parent) { }

        public override MainScreenViewModel MainScreenViewModel
        {
            get { return (MainScreenViewModel)Parent; }
        }

        private List<MainScreenViewModel.IDataChangeListener> _listeners = new List<MainScreenViewModel.IDataChangeListener>();
        protected MainScreenViewModel.ChangedItemListener ListenToItem(Guid itemIdentifier)
        {
            var listener = MainScreenViewModel.ListenToItem(itemIdentifier);

            // We add to an instance variable list, so that the reference won't get lost until the view model gets destroyed
            _listeners.Add(listener);

            return listener;
        }

        protected MainScreenViewModel.ItemsEditedLocallyListener<T> ListenToLocalEditsFor<T>() where T : BaseDataItem
        {
            var listener = MainScreenViewModel.ListenToLocalEditsFor<T>();

            // We add to an instance variable list, so that the reference won't get lost until the view model gets destroyed
            _listeners.Add(listener);

            return listener;
        }
    }
}
