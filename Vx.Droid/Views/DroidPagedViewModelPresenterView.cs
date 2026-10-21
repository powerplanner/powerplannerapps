using InterfacesDroid.ViewModelPresenters;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidPagedViewModelPresenterView : DroidView<PagedViewModelPresenterView, PagedViewModelPresenter>
    {
        public DroidPagedViewModelPresenterView() : base(new PagedViewModelPresenter(VxDroidExtensions.ApplicationContext))
        {

        }

        protected override void ApplyProperties(PagedViewModelPresenterView oldView, PagedViewModelPresenterView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.ViewModel = newView.ViewModel;
        }
    }
}
