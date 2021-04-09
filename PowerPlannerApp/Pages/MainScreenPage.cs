using PowerPlannerApp.App;
using PowerPlannerApp.Pages.SettingsPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Vx.Views;
using Xamarin.Forms;
using static PowerPlannerApp.NavigationManager;

namespace PowerPlannerApp.Pages
{
    public class MainScreenPage : VxPage
    {
        private VxState<MainMenuSelections> _selectedMenuItem = new VxState<MainMenuSelections>(MainMenuSelections.Settings);
        private ObservableCollection<MainMenuSelections> _availableMenuItems = new ObservableCollection<MainMenuSelections>()
        {
            MainMenuSelections.Calendar,
            MainMenuSelections.Schedule,
            MainMenuSelections.Classes,
            MainMenuSelections.Settings
        };

        protected override View Render()
        {
            return new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 200 },
                    new ColumnDefinition { Width = GridLength.Star }
                },

                Children =
                {
                    RenderSidebar(),

                    RenderContent().Column(1)
                }
            };
        }

        private View RenderSidebar()
        {
            return new Grid
            {
                BackgroundColor = PowerPlannerColors.PowerPlannerBlue,

                RowDefinitions =
                {
                    new RowDefinition { Height = 150 },
                    new RowDefinition { Height = GridLength.Star }
                },

                Children =
                {
                    new Image
                    {
                        Aspect = Aspect.AspectFit,
                        Margin = new Thickness(0, 24, 0, 0),
                        Source = new FileImageSource()
                        {
                            File = "Assets/Logo.png" // Note for Android/iOS, this will have to be different
                        }
                    },

                    new ListView
                    {
                        ItemsSource = _availableMenuItems,
                        ItemTemplate = CreateViewCellItemTemplate<MainMenuSelections>("sidebarMenuItemTemplate", s =>
                        {
                            return new ContentView
                            {
                                HeightRequest = 48,
                                Content = new Label
                                {
                                    Text = s.ToString(),
                                    TextColor = Color.White,
                                    FontSize = 20,
                                    LineBreakMode = LineBreakMode.NoWrap,
                                    Margin = new Thickness(36, 0, 0, 4),
                                    VerticalOptions = LayoutOptions.Center
                                }
                            };
                        })
                    }.Row(1).BindSelectedItem(_selectedMenuItem)
                }
            };
        }

        private View RenderContent()
        {
            switch (_selectedMenuItem.Value)
            {
                case MainMenuSelections.Settings:
                    return new SettingsPage();

                default:
                    return new Label { Text = _selectedMenuItem.Value.ToString() };
            }
        }
    }
}
