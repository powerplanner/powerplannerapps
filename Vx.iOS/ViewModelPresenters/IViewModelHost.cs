using BareMvvm.Core.ViewModels;

namespace InterfacesiOS.ViewModelPresenters
{
    public interface IViewModelHost
    {
        BaseViewModel ViewModel { get; set; }
    }
}