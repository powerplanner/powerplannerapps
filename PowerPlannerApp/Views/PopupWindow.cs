using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Views
{
    public class PopupWindow : VxComponent
    {
        public string Title { get; set; }

        public new View Content { get; set; }

        public bool AutoScrollAndPad { get; set; }

        protected override View Render()
        {
            return new Grid
            {
                Children =
                {
                    new Xamarin.Forms.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(new Color(0, 0, 0, 0.3))
                    }.Tap(() => RemoveThisPage()),

                    new Grid()
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        MinimumHeightRequest = 300,
                        WidthRequest = 400,
                        BackgroundColor = Color.White,
                        RowDefinitions =
                        {
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition { Height = GridLength.Star }
                        },
                        Children =
                        {
                            RenderTitleBar(),

                            RenderContent().Row(1)
                        }
                    }
                }
            };
        }

        private View RenderContent()
        {
            if (AutoScrollAndPad)
            {
                Content.Margin = new Thickness(20);

                return new ScrollView
                {
                    Content = Content
                };
            }

            else
            {
                return Content;
            }
        }

        private View RenderTitleBar()
        {
            return new Grid
            {
                BackgroundColor = Color.DarkBlue,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    new Label
                    {
                        TextColor = Color.White,
                        Margin = new Thickness(12),
                        Text = Title
                    },

                    new Button
                    {
                        Text = "Close",
                        TextColor = Color.White,
                        Command = CreateCommand(RemoveThisPage)
                    }.Column(1)
                }
            };
        }
    }
}
