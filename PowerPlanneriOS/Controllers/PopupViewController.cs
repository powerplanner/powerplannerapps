using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public abstract class PopupViewController<T> : BareMvvmUIViewController<T> where T : BaseViewModel
    {
        public Vx.Views.Toolbar Toolbar { get; private set; }

        private UIView _contentView;
        /// <summary>
        /// Where your content should be placed. Displays below the nav bar.
        /// </summary>
        public virtual UIView ContentView { get { return _contentView; } }

        public PopupViewController()
        {
            base.View.BackgroundColor = Vx.Views.Theme.Current.BackgroundColor.ToUI();

            _contentView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            base.View.Add(_contentView);
            _contentView.StretchWidth(base.View);

            var statusBarView = UIStatusBarView.CreateAndAddTo(View);

            // Add the toolbar
            Toolbar = new Vx.Views.Toolbar
            {
                OnBack = BackButtonClicked
            };
            var renderedToolbar = Toolbar.Render();
            base.View.Add(renderedToolbar);
            renderedToolbar.StretchWidth(base.View);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[statusBar][navBar][contentView]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "statusBar", statusBarView,
                "navBar", renderedToolbar,
                "contentView", _contentView));
        }

        public override string Title
        {
            get { return base.Title; }
            set
            {
                Toolbar.Title = value;
                Toolbar.RenderOnDemand();
                base.Title = value;
            }
        }

        public string BackButtonText
        {
            get { return Toolbar.BackText; }
            set { Toolbar.BackText = value; }
        }

        protected virtual void BackButtonClicked()
        {
            ViewModel.GoBack();
        }
    }
}