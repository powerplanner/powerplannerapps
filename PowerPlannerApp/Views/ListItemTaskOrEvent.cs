using PowerPlannerApp.Pages;
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
                        Spacing = 0,
                        Children =
                        {
                            new Label { Text = BindingContext.Name, MaxLines = 1, FontAttributes = FontAttributes.Bold },
                            new Label { Text = BindingContext.Subtitle, MaxLines = 1 },
                            new Label { Text = BindingContext.Details, IsVisible = !string.IsNullOrWhiteSpace(BindingContext.Details), MaxLines = 1 }
                        }

                    }.Column(1)
                }
            }.Tap(ShowItem);
        }

        private void ShowItem()
        {
            ShowPopup(new ViewTaskOrEventPage()
            {
                Item = BindingContext
            });
        }
    }
}
