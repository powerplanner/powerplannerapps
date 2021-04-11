using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Pages
{
    public abstract class VxViewModelPage : VxPage
    {
        [VxSubscribe]
        public BaseViewModel ViewModel { get; set; }
    }

    public abstract class VxViewModelPage<T> : VxViewModelPage where T : BaseViewModel
    {
        [VxSubscribe]
        public new T ViewModel
        {
            get => base.ViewModel as T;
            set => base.ViewModel = value;
        }
    }
}
