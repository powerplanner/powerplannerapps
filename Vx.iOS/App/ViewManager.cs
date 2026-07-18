using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using BareMvvm.Core.App;
using InterfacesiOS.ViewModelPresenters;

namespace InterfacesiOS.App
{
    public class ViewManager
    {
        public void AddMapping(Type viewModelType, Func<UIViewController> createView)
        {
            ViewModelToViewConverter.AddMapping(viewModelType, createView);
        }

        public void AddGenericMapping(Type viewModelType, Func<UIViewController> createView)
        {
            ViewModelToViewConverter.AddGenericMapping(viewModelType, createView);
        }

        public RootViewController RootViewController { get; private set; }

        private BaseViewModel _rootViewModel;
        public BaseViewModel RootViewModel
        {
            get { return _rootViewModel; }
            set
            {
                if (_rootViewModel == value)
                {
                    return;
                }

                _rootViewModel = value;

                if (RootViewController == null)
                {
                    RootViewController = new RootViewController();
                    NativeiOSApplication.Current.Window.RootViewController = RootViewController;
                }

                RootViewController.ViewController = value != null ? ViewModelToViewConverter.Convert(value) : null;
                //NativeiOSApplication.Current.Window.RootViewController = value != null ? ViewModelToViewConverter.Convert(value) : null;
            }
        }
    }
}