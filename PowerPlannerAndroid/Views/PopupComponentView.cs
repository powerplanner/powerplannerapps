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
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateTitle();
            UpdateCommands();

            if (ViewModel.ImportantForAutofill)
            {
                AutofillHelper.EnableForAll(nativeView);
            }

            if (InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets != null)
            {
                UpdateNookInsets(InterfacesDroid.Windows.NativeDroidAppWindow.WindowInsets);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Commands):
                case nameof(ViewModel.SecondaryCommands):
                    UpdateCommands();
                    break;

                case nameof(ViewModel.Title):
                    UpdateTitle();
                    break;
            }
        }

        private void UpdateCommands()
        {
            Toolbar.Menu.Clear();

            if (ViewModel.Commands != null && ViewModel.Commands.Length > 0)
            {
                Toolbar.Menu.Add(ViewModel.Commands[0].Text);
                var item = Toolbar.Menu.GetItem(0);
                item.SetIcon(ToDroidIconResource(ViewModel.Commands[0].Glyph));
                item.SetShowAsAction(ShowAsAction.Always | ShowAsAction.WithText);

                foreach (var c in ViewModel.Commands.Skip(1))
                {
                    Toolbar.Menu.Add(c.Text);
                }
            }

            if (ViewModel.SecondaryCommands != null && ViewModel.SecondaryCommands.Length > 0)
            {
                foreach (var c in ViewModel.SecondaryCommands)
                {
                    Toolbar.Menu.Add(c.Text);
                }
            }
        }

        private static int ToDroidIconResource(string glyph)
        {
            switch (glyph)
            {
                case MaterialDesign.MaterialDesignIcons.Check:
                case MaterialDesign.MaterialDesignIcons.Save:
                    return Resource.Drawable.ic_check_white_36dp;

                case MaterialDesign.MaterialDesignIcons.Delete:
                    throw new NotImplementedException();

                case MaterialDesign.MaterialDesignIcons.Edit:
                    return Resource.Drawable.ic_edit_white_24dp;

                default:
                    throw new NotImplementedException();
            }
        }

        private void UpdateTitle()
        {
            Title = ViewModel.Title;
        }

        public override async void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            PopupCommand matching = null;

            if (ViewModel.Commands != null)
            {
                matching = ViewModel.Commands.FirstOrDefault(i => i.Text == e.Item.TitleFormatted.ToString());
            }

            if (matching == null && ViewModel.SecondaryCommands != null)
            {
                matching = ViewModel.SecondaryCommands.FirstOrDefault(i => i.Text == e.Item.TitleFormatted.ToString());
            }

            if (matching != null)
            {
                if (matching.UseQuickConfirmDelete)
                {
                    // In Android I didn't implement the quick confirm delete
                    if (await PowerPlannerAppDataLibrary.App.PowerPlannerApp.ConfirmDeleteAsync())
                    {
                        matching.Click?.Invoke();
                    }
                }
                else
                {
                    matching.Click?.Invoke();
                }
                return;
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