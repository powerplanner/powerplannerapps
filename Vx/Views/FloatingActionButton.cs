using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ToolsPortable;

namespace Vx.Views
{
    /// <summary>
    /// Only implemented on Android
    /// </summary>
    public partial class FloatingActionButton : VxComponent
    {
        public static readonly float DefaultSize = 50;
        public static readonly float FontSize = 24;

        public Action Click { get; set; }

        /// <summary>
        /// Defaults to the Add icon
        /// </summary>
        public string Glyph { get; set; } = MaterialDesign.MaterialDesignIcons.Add;

        protected override View Render()
        {
            return new Border
            {
                Tapped = Click,
                BackgroundColor = IsMouseDown ?
                    ColorHelper.AdjustLightness(Theme.Current.AccentColor, 0.06f) :
                    IsMouseOver ? ColorHelper.AdjustLightness(Theme.Current.AccentColor, 0.12f) :
                    Theme.Current.AccentColor,
                Width = DefaultSize,
                Height = DefaultSize,
                CornerRadius = DefaultSize / 2f,
                Content = new FontIcon
                {
                    Glyph = Glyph,
                    FontSize = FontSize,
                    Color = Color.White
                }
            };
        }

        public override bool SubscribeToMouseEvents => true;
    }
}
