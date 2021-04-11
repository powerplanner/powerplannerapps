using MaterialDesign;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.Views.PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PowerPlannerAppDataLibrary.Pages
{
    public class ViewTaskOrEventPage : VxPopupWindowPage<ViewTaskOrEventViewModel>
    {
        protected override string Title => ViewModel.PageTitle;

        protected override PopupWindowCommand[] Commands => new PopupWindowCommand[]
        {
            new PopupWindowCommand
            {
                Glyph = MaterialDesignIcons.Edit,
                Title = "Edit",
                Action = ViewModel.Edit
            }
        };

        protected override PopupWindowCommand[] SecondaryCommands => new PopupWindowCommand[]
        {
            new PopupWindowCommand
            {
                Title = ViewModel.ConvertTypeButtonText,
                Action = ViewModel.ConvertType
            },
            new PopupWindowCommand
            {
                Title = "Delete",
                Action = ViewModel.Delete
            }
        };

        protected override View RenderContent()
        {
            return new Grid
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
            };
        }
    }
}
