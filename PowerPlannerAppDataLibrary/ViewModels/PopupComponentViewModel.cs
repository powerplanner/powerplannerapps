using System;
using System.Linq;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels
{
    public class PopupComponentViewModel : ComponentViewModel
    {
        /// <summary>
        /// Optional, if null, typical back behavior will be used. Can use this to specify "Cancel" or other options.
        /// </summary>
        public Tuple<string, Action> BackOverride { get; protected set; }

        public PopupCommand PrimaryCommand
        {
            get => Commands?.FirstOrDefault();
            set => Commands = new PopupCommand[] { value };
        }

        public PopupCommand[] Commands { get; protected set; }

        public PopupComponentViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected void UseCancelForBack()
        {
            BackOverride = new Tuple<string, Action>("Cancel", null);
        }

        /// <summary>
        /// Renders a typical scroll view with padding and items arranged vertically
        /// </summary>
        /// <param name="views"></param>
        /// <returns></returns>
        protected View RenderGenericPopupContent(params View[] views)
        {
            var linearLayout = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
            };
            linearLayout.Children.AddRange(views);

            return new ScrollView
            {
                Content = linearLayout
            };
        }
    }

    public class PopupCommand
    {
        public string Text { get; set; }
        public Action Action { get; set; }
        public string Glyph { get; set; }

        public static PopupCommand Save(Action action)
        {
            return new PopupCommand
            {
                Text = PowerPlannerResources.GetStringSave(),
                Glyph = MaterialDesign.MaterialDesignIcons.Check,
                Action = action
            };
        }

        public static PopupCommand Delete(Action action)
        {
            return new PopupCommand
            {
                Text = PowerPlannerResources.GetString("MenuItemDelete"),
                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                Action = action
            };
        }
    }
}
