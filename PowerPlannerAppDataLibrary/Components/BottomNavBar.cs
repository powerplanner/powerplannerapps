using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    internal partial class BottomNavBar : VxComponent
    {
        public NavigationManager.MainMenuSelections? SelectedItem { get; set; }
        public Action<NavigationManager.MainMenuSelections> SetSelectedItem { get; set; }
        public MainScreenViewModel.SyncStates SyncState { get; set; } = MainScreenViewModel.SyncStates.Done;
        public double UploadImageProgress { get; set; } = 0;
        public bool IsOfflineOrHasSyncError { get; set; } = false;
        public bool HasClasses { get; set; }
        public Thickness NookInsets { get; set; } = new Thickness(0, 0, 0, 0);
        public float BottomNookInset => Math.Max(0, NookInsets.Bottom - 15);

        public static float FinalHeight(Thickness nookInsets)
        {
            return 64 + Math.Max(0, nookInsets.Bottom - 15);
        }

        protected override View Render()
        {
            return new FrameLayout
            {
                BackgroundColor = Theme.Current.ChromeColor,
                Height = 64 + BottomNookInset,
                Children =
                {
                    MainScreenViewModel.RenderSyncProgressBar(SyncState, UploadImageProgress, VerticalAlignment.Bottom),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            HasClasses ? RenderItem(NavigationManager.MainMenuSelections.Calendar, MaterialDesign.MaterialDesignIcons.CalendarMonth, isFirst: HasClasses) : null,
                            HasClasses ? RenderItem(NavigationManager.MainMenuSelections.Agenda, MaterialDesign.MaterialDesignIcons.Task) : null,
                            RenderItem(NavigationManager.MainMenuSelections.Schedule, MaterialDesign.MaterialDesignIcons.Schedule, isFirst: !HasClasses),
                            RenderItem(NavigationManager.MainMenuSelections.Classes, MaterialDesign.MaterialDesignIcons.LibraryBooks),
                            RenderItem(NavigationManager.MainMenuSelections.Settings, MaterialDesign.MaterialDesignIcons.PersonOutline, IsOfflineOrHasSyncError, isLast: true)
                        }
                    }
                }
            };
        }

        private View RenderItem(NavigationManager.MainMenuSelections item, string glyph, bool error = false, bool isFirst = false, bool isLast = false)
        {
            var isThisItem =
                item == SelectedItem ||
                (SelectedItem == NavigationManager.MainMenuSelections.Years && item == NavigationManager.MainMenuSelections.Settings) ||
                (SelectedItem == NavigationManager.MainMenuSelections.Day && item == NavigationManager.MainMenuSelections.Calendar);

            var title = item == NavigationManager.MainMenuSelections.Settings ? PowerPlannerResources.GetString("String_More") : MainScreenViewModel.MainMenuItemToString(item);
            var color = isThisItem ? Theme.Current.ForegroundColor : Theme.Current.SubtleForegroundColor;

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
                    Margin = new Thickness(isFirst ? NookInsets.Left + Theme.Current.SmallPageMargin : 0, 0, isLast ? NookInsets.Right + Theme.Current.SmallPageMargin : 0, BottomNookInset),
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
                            TextColor = color,
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
