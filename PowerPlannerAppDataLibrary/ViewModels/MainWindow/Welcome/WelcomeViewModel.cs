using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using Vx.Views;
using System.Drawing;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class WelcomeViewModel : BaseViewModel
    {
        public WelcomeViewModel(BaseViewModel parent) : base(parent) { }

        public void Login()
        {
            ShowPopup(new LoginViewModel(this));
        }

        public void CreateAccount()
        {
            ShowPopup(new CreateAccountViewModel(this));
        }

        public void OpenSettings()
        {
            ShowPopup(new MinimalSettingsViewModel(this, null));
        }

        public override bool DelayFirstRenderTillSizePresent => true;
        public override IEnumerable<float> SubscribeToWidthBreakpoints => [600];

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            base.OnSizeChanged(size, previousSize);
        }

        protected override View Render()
        {
            bool isCompact = Size.Width < 600;
            var buttonAlignment = isCompact ? VerticalAlignment.Bottom : VerticalAlignment.Top;

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundColor,
                Content = new FrameLayout
                {
                    Children =
                    {
                        new LinearLayout
                        {
                            Margin = new Thickness(Theme.Current.PageMargin).Combine(NookInsets),
                            Children =
                            {
                                new Border
                                {
                                    Content = new LinearLayout
                                    {
                                        VerticalAlignment = VerticalAlignment.Bottom,
                                        Children =
                                        {
                                            new TextBlock
                                            {
                                                Text = "Power Planner",
                                                FontSize = Theme.Current.TitleFontSize,
                                                FontWeight = FontWeights.SemiLight,
                                                TextAlignment = HorizontalAlignment.Center
                                            },

                                            new TextBlock
                                            {
                                                Text = R.S("WelcomePage_TextBlockSubtitle.Text"),
                                                TextAlignment = HorizontalAlignment.Center,
                                                TextColor = Theme.Current.SubtleForegroundColor
                                            }
                                        }
                                    }
                                }.LinearLayoutWeight(1),

                                new LinearLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Children =
                                    {
                                        new AccentButton
                                        {
                                            Text = R.S("Button_LogIn.Content"),
                                            Click = Login,
                                            Margin = new Thickness(0, 18, 6, 0),
                                            VerticalAlignment = buttonAlignment
                                        }.LinearLayoutWeight(1),

                                        new AccentButton
                                        {
                                            Text = R.S("CreateAccountPage_ButtonCreateOnlineAccount.Content"),
                                            Click = CreateAccount,
                                            Margin = new Thickness(6, 18, 0, 0),
                                            VerticalAlignment = buttonAlignment
                                        }.LinearLayoutWeight(1)
                                    },
                                    MaxWidth = 500
                                }.LinearLayoutWeight(1)
                            }
                        },

                        new TransparentContentButton
                        {
                            Margin = new Thickness(Theme.Current.PageMargin - Theme.Current.SmallPageMargin).Combine(NookInsets),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Click = OpenSettings,
                            Content = new LinearLayout
                            {
                                Orientation = Orientation.Horizontal,
                                Margin = new Thickness(Theme.Current.SmallPageMargin),
                                Children =
                                {
                                    new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Settings,
                                        FontSize = 20
                                    },

                                    new TextBlock
                                    {
                                        Text = R.S("MainMenuItem_Settings"),
                                        WrapText = false,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Margin = new Thickness(6, 0, 0, 0)
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
