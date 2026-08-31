using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.ViewModels;
using ToolsPortable;
using UIKit;
using Vx.iOS;
using Vx.iOS.Views;

namespace PowerPlanneriOS.Views
{
    /// <summary>
    /// Hosts a <see cref="PopupComponentViewModel"/> as an in-place popup, using the shared
    /// <see cref="PopupViewHostComponent"/> to render the popup chrome (title bar + commands)
    /// around the view model's content. Analogous to the UWP PopupComponentView, but a plain
    /// <see cref="UIView"/> rather than a view controller.
    /// </summary>
    public class iOSPopupComponentView : UIView
    {
        private readonly PopupComponentViewModel _viewModel;
        private readonly PopupViewHostComponent _hostComponent;
        private readonly iOSNativeComponent _nativeComponent;
        private NSLayoutConstraint _heightConstraint;

        public iOSPopupComponentView(PopupComponentViewModel viewModel)
        {
            _viewModel = viewModel;

            _hostComponent = new PopupViewHostComponent
            {
                OnClose = OnClose,
                PrimaryCommands = new ObservableCollection<PopupCommand>(),
                SecondaryCommands = new ObservableCollection<PopupCommand>(),

                // View models are themselves derivatives of VxComponent, so the view model can be
                // passed directly as the popup's content.
                Content = viewModel
            };

            UpdateTitle();
            UpdateCommands();
            UpdateSecondaryCommands();

            _viewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            _nativeComponent = _hostComponent.Render();
            _nativeComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(_nativeComponent);
            _nativeComponent.StretchWidthAndHeight(this);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Size ourselves to the natural height of the content (measured at our current width,
            // growing vertically). This lets the popup hug short content while a taller popup is
            // capped by the presenter's required top/bottom constraints and scrolls internally.
            nfloat width = Bounds.Width;
            if (width <= 0)
            {
                return;
            }

            var desired = _nativeComponent.MeasureContent(new CGSize(width, UIViewWrapper.UnboundedSize));

            if (_heightConstraint == null)
            {
                _heightConstraint = HeightAnchor.ConstraintEqualTo(desired.Height);

                // Below the presenter's required top/bottom caps, so a tall popup is limited to
                // the available window height rather than overflowing it.
                _heightConstraint.Priority = (float)UILayoutPriority.DefaultHigh;
                _heightConstraint.Active = true;
            }
            else if (Math.Abs((double)(_heightConstraint.Constant - desired.Height)) > 0.5)
            {
                _heightConstraint.Constant = desired.Height;
            }
        }

        private void OnClose()
        {
            _viewModel.TryRemoveViewModelViaUserInteraction();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PopupComponentViewModel.Title):
                    UpdateTitle();
                    break;

                case nameof(PopupComponentViewModel.Commands):
                    UpdateCommands();
                    break;

                case nameof(PopupComponentViewModel.SecondaryCommands):
                    UpdateSecondaryCommands();
                    break;
            }
        }

        private void UpdateTitle()
        {
            _hostComponent.Title = _viewModel.Title?.ToUpper();
        }

        private void UpdateCommands()
        {
            _hostComponent.PrimaryCommands.Clear();
            if (_viewModel.Commands != null)
            {
                foreach (var c in _viewModel.Commands.Where(i => i != null))
                {
                    _hostComponent.PrimaryCommands.Add(c);
                }
            }
        }

        private void UpdateSecondaryCommands()
        {
            _hostComponent.SecondaryCommands.Clear();
            if (_viewModel.SecondaryCommands != null)
            {
                foreach (var c in _viewModel.SecondaryCommands.Where(i => i != null))
                {
                    _hostComponent.SecondaryCommands.Add(c);
                }
            }
        }
    }
}
