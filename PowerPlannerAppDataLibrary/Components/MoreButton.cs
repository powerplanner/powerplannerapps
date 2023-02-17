using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    internal class MoreButton : VxComponent
    {
        /// <summary>
        /// The context menu to show when clicked
        /// </summary>
        public new Func<ContextMenu> ContextMenu { get; set; }

        private View _viewRef;

        protected override View Render()
        {
            return new TransparentContentButton
            {
                Content = new FontIcon
                {
                    Glyph = MaterialDesign.MaterialDesignIcons.MoreHoriz,
                    FontSize = 16,
                    Margin = new Thickness(12, 0, 12, 0)
                },
                ViewRef = v => _viewRef = v,
                Click = () => ContextMenu?.Invoke()?.Show(_viewRef)
            };
        }
    }
}
