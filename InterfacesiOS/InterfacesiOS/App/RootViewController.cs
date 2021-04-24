using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Views;
using UIKit;

namespace InterfacesiOS.App
{
    public class RootViewController : UIViewController
    {
        public BareSnackbarPresenter SnackbarPresenter { get; private set; } = new BareSnackbarPresenter(new BareMvvm.Core.Snackbar.BareSnackbarManager())
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        //public BareSnackbarPresenter SnackbarPresenter => null;

        public RootViewController()
        {
        }

        public override void ViewDidLoad()
        {
            View.AddSubview(SnackbarPresenter);
            SnackbarPresenter.StretchWidthAndHeight(this.View);
            //SnackbarPresenter.PinToBottom(this.View);
            base.ViewDidLoad();
        }

        public UIViewController ViewController
        {
            get => ChildViewControllers.FirstOrDefault();
            set
            {
                try
                {
                    if (ChildViewControllers != null)
                    {
                        foreach (var childViewController in ChildViewControllers)
                        {
                            childViewController.RemoveFromParentViewController();
                            childViewController.View.RemoveFromSuperview();
                        }
                    }

                    if (value != null)
                    {
                        AddChildViewController(value);
                        View.InsertSubview(value.View, 0);
                    }
                }
                catch { }
            }
        }
    }
}