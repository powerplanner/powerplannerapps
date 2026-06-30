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
                else
                {
                    // Ensure we collapse to zero height even when starting out hidden (the
                    // IsVisible setter only collapses on a true->false transition, which never
                    // happens if we're hidden from construction). Without this the container has an
                    // undetermined height and can steal vertical space from siblings inside a
                    // fixed-height parent (e.g. the multiline text box's text area).
                    Collapse();
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
                Collapse();
            }
        }

        private void Collapse()
        {
            this.RemoveConstraints(this.Constraints);
            this.SetHeight(0);
        }
    }
}