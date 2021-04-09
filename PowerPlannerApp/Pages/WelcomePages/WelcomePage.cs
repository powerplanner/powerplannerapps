using PowerPlannerApp.App;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages.WelcomePages
{
    public class WelcomePage : VxPage
    {
        protected override View Render()
        {
            return new Grid
            {
                BackgroundColor = PowerPlannerColors.PowerPlannerBlue,

                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },

                Children =
                {
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = "Power Planner",
                                HorizontalTextAlignment = TextAlignment.Center,
                                TextColor = Color.White
                            },

                            new Label
                            {
                                Text = "The ultimate homework planner",
                                HorizontalTextAlignment = TextAlignment.Center,
                                TextColor = Color.White
                            }
                        }
                    },

                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Star }
                        },

                        Children =
                        {
                            new Button
                            {
                                Text = "Log in",
                                Command = CreateCommand(Login)
                            },

                            new Button
                            {
                                Text = "Create account",
                                Command = CreateCommand(CreateAccount)
                            }.Column(1)
                        }
                    }.Row(1)
                }
            };
        }

        private void Login()
        {
            ShowPopup(new LoginPage());
        }

        private void CreateAccount()
        {

        }
    }
}
