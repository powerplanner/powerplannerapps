using PowerPlannerApp.ViewItems;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Views
{
    public class ListItemTaskOrEvent : VxBindingComponent<ViewItemTaskOrEvent>
    {
        protected override View Render()
        {
            return new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 6 },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                BackgroundColor = Color.LightGray,
                Children =
                {
                    new StackLayout
                    {
                        Children =
                        {
                            new Label { Text = BindingContext.Name },
                            new Label { Text = BindingContext.Subtitle },
                            new Label { Text = BindingContext.Details, IsVisible = !string.IsNullOrWhiteSpace(BindingContext.Details) }
                        }

                    }.Column(1)
                }
            };
        }
    }
}
