using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace VxSampleApp
{
    public class VxGradeOptionsComponent : VxComponent
    {
        private VxState<int> _count = new VxState<int>(0);
        private VxState<string> _selectedItem = new VxState<string>("Nothing selected");

        protected override async void Initialize()
        {
            base.Initialize();

            while (true)
            {
                await Task.Delay(1000);

                _count.Value = _count.Value + 1;
            }
        }
        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    RenderItem("Grade scale", "Edit the grade scale", MaterialDesign.MaterialDesignIcons.Calculate),
                    RenderItem("Average grades", "Disabled", MaterialDesign.MaterialDesignIcons.Refresh),
                    RenderItem("Round grades up", "Enabled", MaterialDesign.MaterialDesignIcons.ArrowUpward),

                    new ListItemButton
                    {
                        Content = new TextBlock
                        {
                            Text = "Count: " + _count.Value + ". Selected: " + _selectedItem.Value
                        }
                    },

                    new ListItemButton
                    {
                        Content = RenderChangingButtonView()
                    }
                }
            };
        }

        private View RenderChangingButtonView()
        {
            if (_count.Value % 2 == 0)
            {
                return new TextBox();
            }
            else
            {
                return new TextBlock { Text = "TextBlock" };
            }
        }

        private View RenderItem(string title, string subtitle, string glyph, Action click = null)
        {
            return new ListItemButton
            {
                Content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = glyph,
                            Width = 40,
                            Height = 40,
                            Color = System.Drawing.Color.DarkBlue,
                            FontSize = 30
                        },

                        new LinearLayout
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = title,
                                    FontWeight = FontWeights.SemiBold
                                },
                                new TextBlock
                                {
                                    Text = subtitle,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }
                            }
                        }.LinearLayoutWeight(1)
                    }
                },
                Margin = new Thickness(0, 0, 0, 12),
                Click = click ?? (() => _selectedItem.Value = title)
            };
        }
    }
}
