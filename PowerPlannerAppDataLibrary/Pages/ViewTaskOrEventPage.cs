using MaterialDesign;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.Views.PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PowerPlannerAppDataLibrary.Pages
{
    public class ViewTaskOrEventPage : VxViewModelPage<ViewTaskOrEventViewModel>
    {
        protected override View Render()
        {
            return new PopupWindow
            {
                Title = ViewModel.PageTitle,
                Commands = new PopupWindowCommand[]
                {
                    new PopupWindowCommand
                    {
                        Glyph = MaterialDesignIcons.Edit,
                        Title = "Edit",
                        Action = ViewModel.Edit
                    }
                },
                SecondaryCommands = new PopupWindowCommand[]
                {
                    new PopupWindowCommand
                    {
                        Title = "Convert to event"
                    },
                    new PopupWindowCommand
                    {
                        Title = "Delete"
                    }
                },
                Content = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Star },
                        new RowDefinition { Height = GridLength.Auto }
                    },

                    Children =
                    {
                        new ScrollView
                        {
                            Content = new StackLayout
                            {
                                Margin = new Thickness(12),
                                Children =
                                {
                                    new Label { Text = ViewModel.Item.Name, FontSize = 20 },

                                    new Label { Text = ViewModel.Item.Subtitle },

                                    new Label { Text = ViewModel.Item.Details }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
