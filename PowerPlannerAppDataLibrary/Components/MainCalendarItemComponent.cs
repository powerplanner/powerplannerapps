using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class MainCalendarItemComponent : VxComponent
    {
        [VxSubscribe]
        public ViewItemTaskOrEvent Item { get; set; }
        public BaseMainScreenViewModelDescendant ViewModel { get; set; }
        public Action<ViewItemTaskOrEvent> ShowItem { get; set; }

        protected override View Render()
        {
            View content = new Border
            {
                BackgroundColor = Item.Class.Color.ToColor(),
                Content = new TextBlock
                {
                    Text = Item.Name,
                    Margin = new Thickness(6),
                    WrapText = false,
                    TextColor = Color.White,
                    Strikethrough = Item.IsComplete,
                    FontSize = Theme.Current.CaptionFontSize,
                    FontWeight = FontWeights.SemiBold
                }
            };

            if (Item.IsComplete)
            {
                content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    BackgroundColor = Item.Class.Color.ToColor(),
                    Opacity = 0.7f,
                    Children =
                        {
                            new Border
                            {
                                BackgroundColor = Color.Black,
                                Opacity = 0.3f,
                                Width = 12
                            },

                            content.LinearLayoutWeight(1)
                        }
                };
            }

            content.Tapped = () => ShowItem(Item);

            content.CanDrag = true;
            content.DragStarting = e =>
            {
                e.Data.Properties.Add("ViewItem", Item);
            };



            // Add context menu
            content.ContextMenu = GetContextMenu;

            return content;
        }

        private ContextMenu GetContextMenu()
        {
            return TaskOrEventContextMenu.Generate(Item, ViewModel);
        }
    }
}
