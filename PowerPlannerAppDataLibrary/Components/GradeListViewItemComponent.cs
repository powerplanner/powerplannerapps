using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class GradeListViewItemComponent : VxComponent
    {
        public BaseViewItemMegaItem Item { get; set; }
        public Action OnRequestViewGrade { get; set; }

        protected override View Render()
        {
            return new Border
            {
                Margin = new Thickness(0,2,0,0),
                BackgroundColor = Theme.Current.BackgroundAlt1Color,
                Tapped = () => OnRequestViewGrade?.Invoke(),
                Content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        CompletionBar(Item),

                        new LinearLayout
                        {
                            Margin = new Thickness(6, 6, 6, 8),
                            Children =
                            {
                                new LinearLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = Item.Name,
                                            FontWeight = FontWeights.SemiBold,
                                            WrapText = false,
                                            Strikethrough = Item.IsDropped
                                        }.LinearLayoutWeight(1),

                                        new TextBlock
                                        {
                                            Text = Item.GradeSubtitle,
                                            FontWeight = FontWeights.SemiBold,
                                            WrapText = false,
                                            TextColor = Item.WeightCategory.Class.Color.ToColor()
                                        }
                                    }
                                },

                                !string.IsNullOrWhiteSpace(Item.Details) ?  new TextBlock
                                {
                                    Text = Item.Details,
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                } : null
                            }
                        }.LinearLayoutWeight(1)
                    }
                }
            };
        }

        private View CompletionBar(BaseViewItemMegaItem item)
        {
            const int width = 8;
            var doneColor = Theme.Current.SubtleForegroundColor.Opacity(0.3);

            return new Border
            {
                Width = width,
                BackgroundColor = item.IsDropped ? doneColor : item.WeightCategory.Class.Color.ToColor()
            };
        }
    }
}
