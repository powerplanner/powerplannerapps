using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BareMvvm.Forms.ViewModels
{
    public class FormsViewViewModel : BaseViewModel
    {
        public View View { get; private set; }

        public FormsViewViewModel(BaseViewModel parent, View view) : base(parent)
        {
            View = view;

            var viewModelProp = view.GetType().GetProperty("ViewModel");
            if (viewModelProp != null)
            {
                viewModelProp.SetValue(view, this);
            }
        }
    }
}
