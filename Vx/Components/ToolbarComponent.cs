﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace Vx.Components.OnlyForNativeLibraries
{
    /// <summary>
    /// This is only for rendering a toolbar when the native platform doesn't have a good toolbar (like UWP)
    /// </summary>
    public class ToolbarComponent : VxComponent
    {
        public static readonly int ToolbarHeight = 48;
        public Toolbar Toolbar { get; set; }

        private View _moreButtonRef;

        protected override View Render()
        {
            if (Toolbar == null)
            {
                return null;
            }

            var layout = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                BackgroundColor = Toolbar.BackgroundColor,
                Height = ToolbarHeight,
                Children =
                {
                    Toolbar.OnBack != null ? RenderButton(MaterialDesign.MaterialDesignIcons.ArrowBack, PortableLocalizedResources.GetString("String_Back"), () => Toolbar.OnBack()) : null,

                    Toolbar.CustomTitle != null ? Toolbar.CustomTitle.LinearLayoutWeight(1) : (View)new TextBlock
                    {
                        Text = Toolbar.Title ?? "",
                        TextColor = Toolbar.ForegroundColor,
                        FontSize = 20,
                        WrapText = false,
                        FontWeight = FontWeights.SemiLight,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(Toolbar.OnBack != null ? 6 : Theme.Current.PageMargin,0,Theme.Current.PageMargin,0)
                    }.LinearLayoutWeight(1)
                }
            };

            foreach (var c in Toolbar.PrimaryCommands.Where(i => i != null))
            {
                View buttonRef = null;
                Action action = c.SubItems != null && c.SubItems.Any() ? () => ShowContextMenu(buttonRef, c.SubItems) : c.Click;
                layout.Children.Add(RenderButton(c.Glyph, c.Text, action, (view) => buttonRef = view));
            }

            if (Toolbar.SecondaryCommands.Any(i => i != null))
            {
                layout.Children.Add(
                    RenderButton(
                        MaterialDesign.MaterialDesignIcons.MoreHoriz,
                        PortableLocalizedResources.GetString("String_More"),
                        () => ShowContextMenu(_moreButtonRef, Toolbar.SecondaryCommands),

                        (moreButton) => _moreButtonRef = moreButton));
            }

            if (Toolbar.OnClose != null)
            {
                layout.Children.Add(
                    RenderButton(
                        MaterialDesign.MaterialDesignIcons.Close,
                        PortableLocalizedResources.GetString("String_Close"),
                        Toolbar.OnClose));
            }

            return layout;
        }

        private void ShowContextMenu(View viewRef, IEnumerable<IMenuItem> commands)
        {
            var cm = new ContextMenu();
            foreach (var c in commands.Where(i => i != null))
            {
                cm.Items.Add(c);
            }

            cm.Show(viewRef);
        }

        private View RenderButton(string glyph, string title, Action onClick, Action<View> viewRef = null)
        {
            return new TransparentContentButton
            {
                Width = ToolbarHeight,
                Content = new FontIcon
                {
                    FontSize = 20,
                    Glyph = glyph,
                    Color = Toolbar.ForegroundColor,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                AltText = title,
                Click = onClick,
                ViewRef = viewRef,
                TooltipText = title
            };
        }
    }
}
