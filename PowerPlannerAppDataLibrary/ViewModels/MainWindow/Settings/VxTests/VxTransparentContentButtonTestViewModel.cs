using System;
using BareMvvm.Core.ViewModels;
using Vx.Views;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule.AddClassTimesViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.VxTests
{
	public class VxTransparentContentButtonTestViewModel : PopupComponentViewModel
	{
		public VxTransparentContentButtonTestViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override View Render()
        {
            // Breaks when the right is a button
            return RenderGenericPopupContent(
                RenderCollapsedOnlyLeftButton("Only left is button", "Mon, Wed, Fri", "CSC 321"),
                RenderCollapsedOnlyRightButton("Only right is button", "Mon, Wed, Fri", "CSC 321"),
                RenderCollapsedBothButton("Both are button", "Mon, Wed, Fri", "CSC 321"),
                RenderCollapsed("Full UI", "Tue, Thu", "ENGL 123"));
        }

        private View RenderCollapsedOnlyLeftButton(string text1, string text2, string text3)
        {
            // Horizontal linear layout with a dynamic height TransparentContentButton (due to vertical LinearLayout inside of it) doesn't work
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                    {
                        new TransparentContentButton
                        {
                            AltText = text1,
                            Content = new LinearLayout
                            {
                                Margin = new Thickness(12),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = text1,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = text2,
                                        WrapText = false
                                    },

                                    string.IsNullOrWhiteSpace(text3) ? null : new TextBlock
                                    {
                                        Text = text3,
                                        WrapText = false
                                    }
                                }
                            }
                        }.LinearLayoutWeight(1),

                        new FontIcon
                            {
                                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                FontSize = 24,
                                Margin = new Thickness(12)
                            }
                    }
            };
        }

        private View RenderCollapsedOnlyRightButton(string text1, string text2, string text3)
        {
            // Horizontal linear layout with a dynamic height TransparentContentButton (due to vertical LinearLayout inside of it) doesn't work
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                    {
                        new LinearLayout
                            {
                                Margin = new Thickness(12),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = text1,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = text2,
                                        WrapText = false
                                    },

                                    string.IsNullOrWhiteSpace(text3) ? null : new TextBlock
                                    {
                                        Text = text3,
                                        WrapText = false
                                    }
                                }
                            }.LinearLayoutWeight(1),

                        new TransparentContentButton
                        {
                            AltText = PowerPlannerResources.GetMenuItemDelete(),
                            Content = new TextBlock
                            {
                                Text = "Del",
                                WrapText = false
                            }
                        }
                    }
            };
        }

        private View RenderCollapsedBothButton(string text1, string text2, string text3)
        {
            // Horizontal linear layout with a dynamic height TransparentContentButton (due to vertical LinearLayout inside of it) doesn't work
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                    {
                        new TransparentContentButton
                        {
                            AltText = text1,
                            Content = new LinearLayout
                            {
                                Margin = new Thickness(12),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = text1,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = text2,
                                        WrapText = false
                                    },

                                    string.IsNullOrWhiteSpace(text3) ? null : new TextBlock
                                    {
                                        Text = text3,
                                        WrapText = false
                                    }
                                }
                            }
                        }.LinearLayoutWeight(1),

                        new TransparentContentButton
                        {
                            AltText = PowerPlannerResources.GetMenuItemDelete(),
                            Content = new FontIcon
                            {
                                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                FontSize = 24,
                                Margin = new Thickness(12)
                            }
                        }
                    }
            };
        }

        private View RenderCollapsed(string text1, string text2, string text3)
        {
            return new Border
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundAltColor,
                BorderThickness = new Thickness(1),
                BorderColor = Theme.Current.SubtleForegroundColor,
                Content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TransparentContentButton
                        {
                            AltText = text1,
                            Content = new LinearLayout
                            {
                                Margin = new Thickness(12),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = text1,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = text2,
                                        WrapText = false
                                    },

                                    string.IsNullOrWhiteSpace(text3) ? null : new TextBlock
                                    {
                                        Text = text3,
                                        WrapText = false
                                    }
                                }
                            }
                        }.LinearLayoutWeight(1),

                        new TransparentContentButton
                        {
                            AltText = PowerPlannerResources.GetMenuItemDelete(),
                            Content = new FontIcon
                            {
                                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                FontSize = 24,
                                Margin = new Thickness(12)
                            }
                        }
                    }
                }
            };
        }
    }
}

