﻿using System;
using System.Collections.Generic;
using System.Linq;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
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
            protected set => Commands = new PopupCommand[] { value };
        }

        private PopupCommand[] _commands;
        public PopupCommand[] Commands
        {
            get => _commands;
            protected set => SetProperty(ref _commands, value, nameof(Commands));
        }

        private PopupCommand[] _secondaryCommands;
        public PopupCommand[] SecondaryCommands
        {
            get => _secondaryCommands;
            protected set => SetProperty(ref _secondaryCommands, value, nameof(SecondaryCommands));
        }

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
        protected View RenderGenericPopupContent(IEnumerable<View> views, Thickness margin)
        {
            var linearLayout = new LinearLayout
            {
                Margin = margin.Combine(NookInsets)
            };
            linearLayout.Children.AddRange(views);

            return new ScrollView
            {
                Content = linearLayout
            };
        }

        /// <summary>
        /// Renders a typical scroll view with padding and items arranged vertically
        /// </summary>
        /// <param name="views"></param>
        /// <returns></returns>
        protected View RenderGenericPopupContent(IEnumerable<View> views)
        {
            return RenderGenericPopupContent(views, new Thickness(Theme.Current.PageMargin));
        }

        /// <summary>
        /// Renders a typical scroll view with padding and items arranged vertically
        /// </summary>
        /// <param name="views"></param>
        /// <returns></returns>
        protected View RenderGenericPopupContent(params View[] views)
        {
            return RenderGenericPopupContent(views as IEnumerable<View>);
        }

        /// <summary>
        /// Renders a typical scroll view with padding and items arranged vertically
        /// </summary>
        /// <param name="views"></param>
        /// <returns></returns>
        protected View RenderGenericPopupContent(Thickness margin, params View[] views)
        {
            return RenderGenericPopupContent(views, margin);
        }
    }

    public enum PopupCommandStyle
    {
        Default,
        Destructive
    }

    public class PopupCommand
    {
        public string Text { get; set; }
        public Action Action { get; set; }
        public string Glyph { get; set; }
        public PopupCommandStyle Style { get; set; }
        public bool UseQuickConfirmDelete { get; set; }

        public PopupCommand() { }

        public PopupCommand(string text, Action action, PopupCommandStyle style = PopupCommandStyle.Default)
        {
            Text = text;
            Action = action;
            Style = style;
        }

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
                Action = action,
                Style = PopupCommandStyle.Destructive
            };
        }

        public static PopupCommand DeleteWithQuickConfirm(Action actualDeleteAction)
        {
            return new PopupCommand
            {
                Text = PowerPlannerResources.GetString("MenuItemDelete"),
                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                UseQuickConfirmDelete = true,
                Action = actualDeleteAction,
                Style = PopupCommandStyle.Destructive
            };
        }

        public static PopupCommand Edit(Action action)
        {
            return new PopupCommand
            {
                Text = PowerPlannerResources.GetString("AppBarButtonEdit.Label"),
                Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                Action = delegate { action?.Invoke(); }
            };
        }
    }
}
