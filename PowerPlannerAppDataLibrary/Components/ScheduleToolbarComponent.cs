using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class ScheduleToolbarComponent : VxComponent
    {
        public ScheduleViewModel ViewModel { get; set; }

        protected override View Render()
        {
            if (ViewModel == null)
            {
                return new Toolbar();
            }

            // Note that VxSubscribe prop only works if prop is already pre-set, so using Subscribe here instead
            Subscribe(ViewModel);

            bool hasScheduleContent = ViewModel.LayoutMode != ScheduleViewModel.LayoutModes.Welcome && ViewModel.LayoutMode != ScheduleViewModel.LayoutModes.FullEditing;

            return new Toolbar
            {
                Title = !hasScheduleContent ? MainScreenViewModel.MainMenuItemToString(NavigationManager.MainMenuSelections.Schedule) : null,
                CustomTitle = hasScheduleContent ? new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(Theme.Current.PageMargin, 0, 0, 0),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = ViewModel.DisplayStartDate.ToString(GetWeekDateConverterParameter()) + "-" +
                                ViewModel.DisplayEndDate.ToString(GetWeekDateConverterParameter()),
                            TextColor = Color.White,
                            FontSize = 20,
                            FontWeight = FontWeights.SemiLight,
                            WrapText = false
                        },
                        new TextBlock
                        {
                            Text = ViewModel.DisplayStartDate.ToString("yyyy") + (ViewModel.HasTwoWeekSchedule ? ", " + PowerPlannerResources.GetLocalizedWeek(ViewModel.CurrentWeek) : ""),
                            TextColor = Color.White,
                            Opacity = 0.7f,
                            FontSize = 16,
                            FontWeight = FontWeights.SemiLight,
                            WrapText = false,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Margin = new Thickness(6, 0, 0, 1)
                        }
                    }
                } : null,
                PrimaryCommands =
                {
                    hasScheduleContent ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_LastWeek"),
                        Glyph = MaterialDesign.MaterialDesignIcons.ChevronLeft,
                        Click = ViewModel.PreviousWeek
                    } : null,

                    hasScheduleContent ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_NextWeek"),
                        Glyph = MaterialDesign.MaterialDesignIcons.ChevronRight,
                        Click = ViewModel.NextWeek
                    } : null,

                    ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.SplitEditing || ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.FullEditing ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetStringClose(),
                        Glyph = MaterialDesign.MaterialDesignIcons.Close,
                        Click = ViewModel.ExitEditMode
                    } : new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_EditSchedule"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                        Click = ViewModel.EnterEditMode
                    }
                },
                SecondaryCommands =
                {
                    !ViewModel.IsPinned && ViewModel.RequestPinHandler != null && ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.Normal ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_PinToStart"),
                        Click = ViewModel.RequestPinHandler
                    } : null,

                    ViewModel.IsPinned && ViewModel.RequestUnpinHandler != null && ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.Normal ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_UnpinFromStart"),
                        Click = ViewModel.RequestUnpinHandler
                    } : null,

                    hasScheduleContent && ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.Normal && ViewModel.RequestExportToImage != null ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_ExportToImage"),
                        Click = ViewModel.RequestExportToImage
                    } : null
                }
            }.InnerToolbarThemed();
        }



        private static string GetWeekDateConverterParameter()
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("en"))
            {
                return "M/d";
            }
            else
            {
                return "d";
            }
        }
    }
}
