using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    internal class BottomNavBar : VxComponent
    {
        public NavigationManager.MainMenuSelections? SelectedItem { get; set; }
        public Action<NavigationManager.MainMenuSelections> SetSelectedItem { get; set; }
        public MainScreenViewModel.SyncStates SyncState { get; set; } = MainScreenViewModel.SyncStates.Done;
        public double UploadImageProgress { get; set; } = 0;
        public bool IsOfflineOrHasSyncError { get; set; } = false;

        protected override View Render()
        {
            return new FrameLayout
            {
                BackgroundColor = Theme.Current.ChromeColor,
                Height = 64,
                Children =
                {
                    MainScreenViewModel.RenderSyncProgressBar(SyncState, UploadImageProgress, VerticalAlignment.Bottom),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            RenderItem(NavigationManager.MainMenuSelections.Calendar, MaterialDesign.MaterialDesignIcons.CalendarMonth),
                            RenderItem(NavigationManager.MainMenuSelections.Agenda, MaterialDesign.MaterialDesignIcons.Task),
                            RenderItem(NavigationManager.MainMenuSelections.Schedule, MaterialDesign.MaterialDesignIcons.Schedule),
                            RenderItem(NavigationManager.MainMenuSelections.Classes, MaterialDesign.MaterialDesignIcons.LibraryBooks),
                            RenderItem(NavigationManager.MainMenuSelections.Settings, MaterialDesign.MaterialDesignIcons.PersonOutline, IsOfflineOrHasSyncError)
                        }
                    }
                }
            };
        }

        private View RenderItem(NavigationManager.MainMenuSelections item, string glyph, bool error = false)
        {
            var isThisItem = item == SelectedItem;
            var title = item == NavigationManager.MainMenuSelections.Settings ? PowerPlannerResources.GetString("String_More") : MainScreenViewModel.MainMenuItemToString(item);
            var color = isThisItem ? System.Drawing.Color.White : System.Drawing.Color.LightGray;

            var icon = new FontIcon
            {
                Glyph = glyph,
                FontSize = 24,
                Color = color
            };

            return new TransparentContentButton
            {
                Content = new LinearLayout
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        error ? (View)new FrameLayout
                        {
                            Children =
                            {
                                icon,
                                new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Error,
                                    FontSize = 14,
                                    Color = color,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(24,0,0,24)
                                }
                            }
                        } : (View)icon,

                        isThisItem ? new TextBlock
                        {
                            Text = title,
                            TextColor = System.Drawing.Color.White,
                            TextAlignment = HorizontalAlignment.Center,
                            WrapText = false,
                            FontSize = 12
                        } : null
                    }
                },
                Click = () =>
                {
                    SetSelectedItem?.Invoke(item);
                },
                TooltipText = title
            }.LinearLayoutWeight(1);
        }
    }
}
