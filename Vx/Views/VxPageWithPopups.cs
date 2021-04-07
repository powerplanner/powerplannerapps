using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public abstract class VxPageWithPopups : VxPage
    {
        private bool _hasRenderedContent;
        private Grid _rootGrid;

        protected override View PrepRenderedContentContainer()
        {
            _rootGrid = new Grid();
            return _rootGrid;
        }

        protected override View RenderedContent
        {
            get => _hasRenderedContent ? _rootGrid.Children[0] : null;
            set
            {
                if (_hasRenderedContent)
                {
                    if (value != null)
                    {
                        _rootGrid.Children[0] = value;
                    }
                    else
                    {
                        _rootGrid.Children.RemoveAt(0);
                        _hasRenderedContent = false;
                    }
                }
                else
                {
                    if (value != null)
                    {
                        _rootGrid.Children.Insert(0, value);
                        _hasRenderedContent = true;
                    }
                }
            }
        }

        protected override void ShowPopup(VxPage page)
        {
            page.IsRootComponent = true;

            _rootGrid.Children.Add(page);
        }

        protected override void RemovePage(VxPage page)
        {
            // If we have the page and remove it
            if (_rootGrid.Children.Remove(page))
            {
                // Stop
                return;
            }

            // Otherwise continue with sending it upward
            base.RemovePage(page);
        }
    }
}
