using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using InterfacesDroid.Helpers;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Droid;

namespace PowerPlannerAndroid.Views
{
    public class PopupComponentView : PopupViewHost<PopupComponentViewModel>, AndroidX.Core.View.IOnApplyWindowInsetsListener
    {
        public PopupComponentView(ViewGroup root) : base(root)
        {
        }

        public override WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat windowInsets)
        {
            UpdateNookInsets(windowInsets);

            return base.OnApplyWindowInsets(v, windowInsets);
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();


            var nativeView = ViewModel.Render();
            nativeView.LayoutParameters = new Android.Views.ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            AddNonInflatedView(nativeView);

            // Make sure title and commands are called after calling AddNonInflatedView, since that method creates the toolbar/etc
            BindingHost.SetBinding<PopupComponentViewModel, string>(nameof(ViewModel.Title), viewModel => viewModel.Title, t =>
            {
                Title = t;
            });
            BindingHost.SetBindings<PopupComponentViewModel>(
                [nameof(ViewModel.Commands), nameof(ViewModel.SecondaryCommands)],
                UpdateCommands);

            if (ViewModel.ImportantForAutofill)
            {
                AutofillHelper.EnableForAll(nativeView);
            }

            if (InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets != null)
            {
                UpdateNookInsets(InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets);
            }
        }

        private void UpdateCommands()
        {
            if (ViewModel.Commands != null)
            {
                Toolbar.PrimaryCommands = ViewModel.Commands.OfType<Vx.Views.MenuItem>().ToList();
            }
            else
            {
                Toolbar.PrimaryCommands = new();
            }

            if (ViewModel.SecondaryCommands != null)
            {
                Toolbar.SecondaryCommands = ViewModel.SecondaryCommands.OfType<Vx.Views.MenuItem>().ToList();
            }
            else
            {
                Toolbar.SecondaryCommands = new();
            }

            Toolbar.RenderOnDemand();
        }

        private void UpdateNookInsets(WindowInsetsCompat windowInsets)
        {
            var imeInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.Ime());

            var insets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());

            ViewModel.UpdateNookInsets(new Vx.Views.Thickness(
                ThemeHelper.FromPxPrecise(Context, insets.Left),
                0,
                ThemeHelper.FromPxPrecise(Context, insets.Right),
                imeInsets.Bottom == 0 ? ThemeHelper.FromPxPrecise(Context, insets.Bottom) : 0));
        }
    }
}