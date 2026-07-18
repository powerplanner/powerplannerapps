using BareMvvm.Core.ViewModels;

namespace InterfacesDroid.ViewModelPresenters
{
    public interface IViewModelHost
    {
        BaseViewModel ViewModel { get; set; }
    }
}