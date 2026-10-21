using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ToolsPortable;

namespace Vx.Views
{
    public partial class Toolbar : VxComponent
    {
        public static readonly int ToolbarHeight = 48;
        public string Title { get; set; }
        /// <summary>
        /// CustomTitle is only supported on UWP right now
        /// </summary>
        public View CustomTitle { get; set; }
        public Action OnBack { get; set; }
        public string BackText { get; set; }
        public Color BackgroundColor { get; set; } = Theme.Current.ChromeColor;
        public Color ForegroundColor { get; set; } = Theme.Current.ForegroundColor;
        public List<MenuItem> PrimaryCommands { get; set; } = new List<MenuItem>();
        public List<MenuItem> SecondaryCommands { get; set; } = new List<MenuItem>();
        public Thickness NookInsets { get; set; }

        /// <summary>
        /// This is only supported on iOS.
        /// </summary>
        public bool AlignTitleToLeft { get; set; }

        /// <summary>
        /// Setting this displays a Close button on the far right (useful for popup toolbars). Currently only implemented on UWP.
        /// </summary>
        public Action OnClose { get; set; }

        /// <summary>
        /// When true, forces the toolbar to render using the cross-platform Vx <see cref="Vx.Components.OnlyForNativeLibraries.ToolbarComponent"/>
        /// instead of the native platform toolbar. Useful for in-place popups (e.g. Mac Catalyst) that want the Vx toolbar style. Only affects iOS.
        /// </summary>
        public bool ForceUseVx { get; set; }

        /// <summary>
        /// iOS reverses the primary commands, so the first one is displayed on the right. If you have relational-specific commands,
        /// like Previous and Next, use this to determine if you should reveres those command orders.
        /// </summary>
        public static readonly bool DisplaysPrimaryCommandsRightToLeft = VxPlatform.Current == Platform.iOS;

        protected override View Render()
        {
            // On iOS, the platform convention is a centered title with the close button on the
            // left and the remaining commands on the right.
            bool isIOS = VxPlatform.Current == Platform.iOS;

            var layout = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                BackgroundColor = BackgroundColor,
                Height = ToolbarHeight
            };

            if (OnBack != null)
            {
                layout.Children.Add(RenderButton(MaterialDesign.MaterialDesignIcons.ArrowBack, PortableLocalizedResources.GetString("String_Back"), () => OnBack()));
            }

            if (isIOS && OnClose != null)
            {
                layout.Children.Add(RenderButton(MaterialDesign.MaterialDesignIcons.Close, PortableLocalizedResources.GetString("String_Close"), OnClose));
            }

            layout.Children.Add(
                CustomTitle != null ? CustomTitle.LinearLayoutWeight(1) : (View)new TextBlock
                {
                    Text = Title ?? "",
                    TextColor = ForegroundColor,
                    FontSize = 20,
                    WrapText = false,
                    FontWeight = FontWeights.SemiLight,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = HorizontalAlignment.Left,
                    Margin = (isIOS && OnClose != null) ? new Thickness() : new Thickness(OnBack != null ? 6 : Theme.Current.PageMargin, 0, Theme.Current.PageMargin, 0)
                }.LinearLayoutWeight(1));

            foreach (var c in PrimaryCommands.Where(i => i != null))
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

            if (SecondaryCommands.Any(i => i != null))
            {
                layout.Children.Add(
                    RenderMenuButton(
                        MaterialDesign.MaterialDesignIcons.MoreHoriz,
                        PortableLocalizedResources.GetString("String_More"),
                        SecondaryCommands));
            }

            if (!isIOS && OnClose != null)
            {
                layout.Children.Add(
                    RenderButton(
                        MaterialDesign.MaterialDesignIcons.Close,
                        PortableLocalizedResources.GetString("String_Close"),
                        OnClose));
            }

            if (NookInsets.Top > 0 || NookInsets.Left > 0 || NookInsets.Right > 0)
            {
                return new Border
                {
                    BackgroundColor = BackgroundColor,
                    Content = layout,
                    Height = ToolbarHeight + NookInsets.Top,
                    Padding = new Thickness(NookInsets.Left, NookInsets.Top, NookInsets.Right, 0)
                };
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
                    Color = ForegroundColor,
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
                    Color = ForegroundColor,
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
