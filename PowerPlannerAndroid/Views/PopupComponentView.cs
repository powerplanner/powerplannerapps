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

            // Make sure title and primary commands are called after calling AddNonInflatedView, since that method creates the toolbar/etc
            Title = ViewModel.Title;

            if (ViewModel.Commands != null && ViewModel.Commands.Length > 0)
            {
                Toolbar.Menu.Add(ViewModel.Commands[0].Text);
                var item = Toolbar.Menu.GetItem(0);
                item.SetIcon(Resource.Drawable.ic_check_white_36dp);
                item.SetShowAsAction(ShowAsAction.Always | ShowAsAction.WithText);

                foreach (var c in ViewModel.Commands.Skip(1))
                {
                    Toolbar.Menu.Add(c.Text);
                }
            }

            if (ViewModel.ImportantForAutofill)
            {
                AutofillHelper.EnableForAll(nativeView);
            }

            if (InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets != null)
            {
                UpdateNookInsets(InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets);
            }
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            if (ViewModel.Commands != null)
            {
                ViewModel.Commands.FirstOrDefault(i => i.Text == e.Item.TitleFormatted.ToString())?.Action?.Invoke();
            }
        }

        private void UpdateNookInsets(WindowInsetsCompat windowInsets)
        {
            var imeInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.Ime());

            var insets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());

            ViewModel.UpdateNookInsets(new Vx.Views.Thickness(
                ThemeHelper.FromPxPrecise(Context, insets.Left),
                0,
                ThemeHelper.FromPxPrecise(Context, insets.Right),
                imeInsets.Bottom == 0 ? ThemeHelper.FromPxPrecise(Context, insets.Bottom) : 0));
        }
    }
}