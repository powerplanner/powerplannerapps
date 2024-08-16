using InterfacesUWP.ViewModelPresenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    internal class UwpPagedViewModelPresenterView : UwpView<Vx.Views.PagedViewModelPresenterView, PagedViewModelPresenter>
    {
        protected override void ApplyProperties(PagedViewModelPresenterView oldView, PagedViewModelPresenterView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.ViewModel = newView.ViewModel;
        }
    }
}
