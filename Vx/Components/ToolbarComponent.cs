using System;
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
    public partial class ToolbarComponent : VxComponent
    {
        public static readonly int ToolbarHeight = 48;
        public Toolbar Toolbar { get; set; }

        protected override View Render()
        {
            if (Toolbar == null)
            {
                return null;
            }

            // On iOS, the platform convention is a centered title with the close button on the
            // left and the remaining commands on the right.
            bool isIOS = VxPlatform.Current == Platform.iOS;

            var layout = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                BackgroundColor = Toolbar.BackgroundColor,
                Height = ToolbarHeight
            };

            if (Toolbar.OnBack != null)
            {
                layout.Children.Add(RenderButton(MaterialDesign.MaterialDesignIcons.ArrowBack, PortableLocalizedResources.GetString("String_Back"), () => Toolbar.OnBack()));
            }

            if (isIOS && Toolbar.OnClose != null)
            {
                layout.Children.Add(RenderButton(MaterialDesign.MaterialDesignIcons.Close, PortableLocalizedResources.GetString("String_Close"), Toolbar.OnClose));
            }

            layout.Children.Add(
                Toolbar.CustomTitle != null ? Toolbar.CustomTitle.LinearLayoutWeight(1) : (View)new TextBlock
                {
                    Text = Toolbar.Title ?? "",
                    TextColor = Toolbar.ForegroundColor,
                    FontSize = 20,
                    WrapText = false,
                    FontWeight = FontWeights.SemiLight,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = HorizontalAlignment.Left,
                    Margin = isIOS ? new Thickness() : new Thickness(Toolbar.OnBack != null ? 6 : Theme.Current.PageMargin,0,Theme.Current.PageMargin,0)
                }.LinearLayoutWeight(1));

            foreach (var c in Toolbar.PrimaryCommands.Where(i => i != null))
            {
                if (c.SubItems != null && c.SubItems.Any())
                {
                    layout.Children.Add(RenderMenuButton(c.Glyph, c.Text, c.SubItems));
                }
                else
                {
                    layout.Children.Add(RenderButton(c.Glyph, c.Text, c.Click));
                }
            }

            if (Toolbar.SecondaryCommands.Any(i => i != null))
            {
                layout.Children.Add(
                    RenderMenuButton(
                        MaterialDesign.MaterialDesignIcons.MoreHoriz,
                        PortableLocalizedResources.GetString("String_More"),
                        Toolbar.SecondaryCommands));
            }

            if (!isIOS && Toolbar.OnClose != null)
            {
                layout.Children.Add(
                    RenderButton(
                        MaterialDesign.MaterialDesignIcons.Close,
                        PortableLocalizedResources.GetString("String_Close"),
                        Toolbar.OnClose));
            }

            return layout;
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

        private View RenderMenuButton(string glyph, string title, IEnumerable<IMenuItem> menu)
        {
            return new TransparentContentMenuButton
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
                Menu = menu.Where(i => i != null).ToList(),
                TooltipText = title
            };
        }
    }
}
