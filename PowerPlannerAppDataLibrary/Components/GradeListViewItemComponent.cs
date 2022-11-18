using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class GradeListViewItemComponent : VxComponent
    {
        [VxSubscribe] // Subscribe is needed for What If? mode
        public BaseViewItemMegaItem Item { get; set; }
        public Action OnRequestViewGrade { get; set; }
        public bool IsInWhatIfMode { get; set; }

        protected override View Render()
        {
            Color accentColor;

            if (IsInWhatIfMode)
            {
                if (Item is ViewItemTaskOrEvent t)
                {
                    accentColor = t.ColorWhenInWhatIfMode.ToColor();
                }
                else if (Item is ViewItemGrade g)
                {
                    accentColor = g.ColorWhenInWhatIfMode.ToColor();
                }
                else
                {
                    // I don't think this ever should get hit
                    accentColor = Theme.Current.AccentColor;
                }
            }
            else
            {
                var doneColor = Theme.Current.SubtleForegroundColor.Opacity(0.3);
                accentColor = Item.IsDropped ? doneColor : Item.WeightCategory.Class.Color.ToColor();
            }

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
                        CompletionBar(Item, accentColor),

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
                                            TextColor = accentColor
                                        }
                                    }
                                },

                                !string.IsNullOrWhiteSpace(Item.Details) ?  new TextBlock
                                {
                                    Text = Item.Details.Replace("\n", "  ").Trim(),
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                } : null
                            }
                        }.LinearLayoutWeight(1)
                    }
                }
            };
        }

        private View CompletionBar(BaseViewItemMegaItem item, Color accentColor)
        {
            const int width = 8;

            return new Border
            {
                Width = width,
                BackgroundColor = accentColor
            };
        }
    }
}
