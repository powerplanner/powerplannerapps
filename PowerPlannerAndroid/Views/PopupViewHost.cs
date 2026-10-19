using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using BareMvvm.Core.ViewModels;
using PowerPlannerAndroid.Helpers;
using AndroidX.Core.View;
using PowerPlannerAppDataLibrary.Helpers;
using Vx.Droid;

namespace PowerPlannerAndroid.Views
{
    public class PopupViewHost<TViewModel> : InterfacesDroid.Views.PopupViewHost<TViewModel>, AndroidX.Core.View.IOnApplyWindowInsetsListener where TViewModel : BaseViewModel
    {
        public Vx.Views.Toolbar Toolbar { get; private set; }

        public PopupViewHost(int resourceId, ViewGroup root) : base(resourceId, root)
        {
        }

        public PopupViewHost(ViewGroup root) : base(root)
        {
        }

        protected void AddNonInflatedView(View content)
        {
            View popupView = LayoutInflater.FromContext(Context).Inflate(Resource.Layout.PopupView, null);

            popupView.FindViewById<ViewGroup>(Resource.Id.ContentFrame).AddView(content);

            InitializePopupView(popupView);

            AddView(popupView);
        }

        protected override View CreateView(LayoutInflater inflater, int resourceId, ViewGroup root)
        {
            View popupView = inflater.Inflate(Resource.Layout.PopupView, null);

            // Use the base method so that it participates in binding correctly
            View content = base.CreateView(inflater, resourceId, root);

            popupView.FindViewById<ViewGroup>(Resource.Id.ContentFrame).AddView(content);

            InitializePopupView(popupView);

            return popupView;
        }

        private View _keyboardSpacer;
        private View _statusBarSpacer;

        private void InitializePopupView(View popupView)
        {
            // Add the toolbar
            Toolbar = new Vx.Views.Toolbar
            {
                OnBack = () => ViewModel.GoBack()
            };
            var renderedToolbar = Toolbar.Render();
            renderedToolbar.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            popupView.FindViewById<FrameLayout>(Resource.Id.Toolbar).AddView(renderedToolbar);

            _keyboardSpacer = popupView.FindViewById(Resource.Id.KeyboardSpacer);
            _statusBarSpacer = popupView.FindViewById(Resource.Id.StatusBarSpacer);

            if (InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets != null)
            {
                OnApplyWindowInsets(popupView, InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets);
            }

            ViewCompat.SetOnApplyWindowInsetsListener(popupView, this);

            // Apply current theme colors (subscription moved to OnAttachedToWindow)
            ApplyThemeColors(DroidThemeColorApplier.Current);
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            DroidThemeColorApplier.ThemeChanged += ApplyThemeColors;
        }

        protected override void OnDetachedFromWindow()
        {
            DroidThemeColorApplier.ThemeChanged -= ApplyThemeColors;
            base.OnDetachedFromWindow();
        }

        private void ApplyThemeColors(ThemeColors colors)
        {
            try
            {
                _statusBarSpacer?.SetBackgroundColor(DroidThemeColorApplier.ToDroid(Vx.Views.Theme.Current.ChromeColor));
                Toolbar?.BackgroundColor = Vx.Views.Theme.Current.ChromeColor;
                Toolbar?.ForegroundColor = Vx.Views.Theme.Current.ForegroundColor;
                Toolbar?.RenderOnDemand();
            }
            catch (Exception ex)
            {
                PowerPlannerAppDataLibrary.Extensions.TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public virtual WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat windowInsets)
        {
            var imeInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.Ime());
            var insets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());

            var keyboardSpacerLp = _keyboardSpacer.LayoutParameters;
            keyboardSpacerLp.Height = imeInsets.Bottom;
            if (!(this is PopupComponentView) && imeInsets.Bottom == 0)
            {
                // For the old views (non-Vx), we also handle bottom inset
                keyboardSpacerLp.Height += insets.Bottom;
            }
            _keyboardSpacer.LayoutParameters = keyboardSpacerLp;

            var statusBarSpacerLp = _statusBarSpacer.LayoutParameters;
            statusBarSpacerLp.Height = insets.Top;
            _statusBarSpacer.LayoutParameters = statusBarSpacerLp;

            Toolbar.NookInsets = new Vx.Views.Thickness(
                InterfacesDroid.Themes.ThemeHelper.FromPxPrecise(Context, insets.Left),
                0,
                InterfacesDroid.Themes.ThemeHelper.FromPxPrecise(Context, insets.Right),
                0);
            Toolbar.RenderOnDemand();

            // Return CONSUMED if you don't want want the window insets to keep being
            // passed down to descendant views.
            return WindowInsetsCompat.Consumed;
        }

        public string Title
        {
            get { return Toolbar.Title; }
            set { Toolbar.Title = value; Toolbar.RenderOnDemand(); }
        }
    }
}