using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using static PowerPlannerAppDataLibrary.Services.AiService;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    public class ScheduleWelcomeComponent : VxComponent
    {
        public ScheduleViewModel ScheduleViewModel { get; set; }
        public Thickness NookInsets { get; set; }

        private VxState<bool> _displayInScrollView = new VxState<bool>(false);

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            _displayInScrollView.Value = size.Height <= 500;

            base.OnSizeChanged(size, previousSize);
        }

        protected override View Render()
        {
            const int MAX_WIDTH = 700;

            var content = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
                                .Combine(NookInsets)
                                .Combine(new Thickness(0, 60, 0, 60)),
                Children =
                {
                    new TextBlock
                    {
                        Text = R.S("SchedulePage_TextBlockWelcomeTitle.Text"),
                        TextAlignment = HorizontalAlignment.Center,
                        MaxWidth = MAX_WIDTH,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = Theme.Current.TitleFontSize
                    },

                    new TextBlock
                    {
                        Text = R.S("SchedulePage_TextBlockWelcomeSubtitle.Text"),
                        TextAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 0),
                        TextColor = Theme.Current.SubtleForegroundColor,
                        MaxWidth = MAX_WIDTH,
                    },

                    new AccentButton
                    {
                        Text = R.S("ScheduleWelcome_ButtonAddWithAi"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Click = () => ScheduleViewModel.ShowPopup(new AddClassesAndScheduleWithAiViewModel(ScheduleViewModel)),
                        Margin = new Thickness(0, 12, 0, 0)
                    },

                    new TextBlock
                    {
                        Text = R.S("ScheduleWelcome_TextOrManually"),
                        TextAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 12, 0, 0),
                        TextColor = Theme.Current.SubtleForegroundColor
                    },

                    new AccentButton
                    {
                        Text = "+ " + R.S("SchedulePage_ButtonAddClass.Content"),
                        Click = ScheduleViewModel.AddClass,
                        MaxWidth = 180,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 0)
                    }
                }
            };

            View contentContainer;

            if (!_displayInScrollView.Value)
            {
                content.VerticalAlignment = VerticalAlignment.Center;
                contentContainer = content;
            }
            else
            {
                contentContainer = new ScrollView
                {
                    Content = content
                };
            }

            return new FrameLayout
            {
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                Children =
                {
                    contentContainer,

                    ScheduleViewModel.IsReturningUserVisible ? new LinearLayout
                    {
                        Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, 0).Combine(NookInsets),
                        BackgroundColor = Theme.Current.BackgroundAlt2Color,
                        VerticalAlignment = VerticalAlignment.Top,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = R.S("SchedulePage_TextBlockReturningUser.Text"),
                                TextAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0, Theme.Current.PageMargin + NookInsets.Top - 8, 0, 0)
                            },

                            new AccentButton
                            {
                                Text = R.S("WelcomePage_ButtonLogin.Content"),
                                Margin = new Thickness(0, 6, 0, 12),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Click = ScheduleViewModel.LogIn,
                                Width = 120
                            }
                        }
                    } : null

                }
            };
        }
    }
}
