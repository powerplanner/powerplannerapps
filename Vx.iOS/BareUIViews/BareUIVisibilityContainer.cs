using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUIVisibilityContainer : UIView
    {
        public BareUIVisibilityContainer()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
        }

        private bool _isVisible = false;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (value == _isVisible)
                {
                    return;
                }

                _isVisible = value;

                if (value)
                {
                    Add();
                }
                else
                {
                    Remove();
                }
            }
        }

        private UIView _child;
        public UIView Child
        {
            get { return _child; }
            set
            {
                Remove();

                _child = value;

                if (IsVisible)
                {
                    Add();
                }
            }
        }

        private void Add()
        {
            if (Child != null)
            {
                this.RemoveConstraints(this.Constraints);
                Add(Child);
                Child.TranslatesAutoresizingMaskIntoConstraints = false;
                Child.StretchWidthAndHeight(this);
            }
        }

        private void Remove()
        {
            if (Child != null)
            {
                Child.RemoveFromSuperview();
                this.RemoveConstraints(this.Constraints);
                this.SetHeight(0);
            }
        }
    }
}