using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos
{
    public class PromoPriceIncreaseViewModel : PopupComponentViewModel
    {
        private const string SETTING_HAS_PROMOTED_PRICE_INCREASE = "Promo-b0fb6337-ef8f-4950-9e3a-9ef217d036b8";
        private const string SETTING_FINAL_PRICE_INCREASE_PROMO = "Promo-0a9563d0-52ab-4fbc-9f77-6cc528cce5a5";
        public static DateTime LastDate = new DateTime(2025, 1, 24, 20, 0, 0, DateTimeKind.Local);

        public class Registration : PromoRegistration
        {
            public override BaseViewModel Create(AccountDataItem account, BaseViewModel parent)
            {
                bool finalReminder = PowerPlannerAppDataLibrary.Helpers.Settings.AppSettings.Contains(SETTING_HAS_PROMOTED_PRICE_INCREASE);

                return new PromoPriceIncreaseViewModel(parent, finalReminder);
            }

            public override void MarkShown(AccountDataItem account)
            {
                if (!Helpers.Settings.AppSettings.Contains(SETTING_HAS_PROMOTED_PRICE_INCREASE))
                {
                    Helpers.Settings.AppSettings.AddOrUpdateValue(SETTING_HAS_PROMOTED_PRICE_INCREASE, true);
                }
                else
                {
                    Helpers.Settings.AppSettings.AddOrUpdateValue(SETTING_FINAL_PRICE_INCREASE_PROMO, true);
                }
            }

            public override async Task<bool> ShouldShowAsync(AccountDataItem account)
            {
                if (DateTime.Now > LastDate)
                {
                    return false;
                }

                if (await App.PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    return false;
                }

                if (PowerPlannerAppDataLibrary.Helpers.Settings.AppSettings.Contains(SETTING_HAS_PROMOTED_PRICE_INCREASE))
                {
                    if (DateTime.Now > LastDate.AddDays(-3) && !PowerPlannerAppDataLibrary.Helpers.Settings.AppSettings.Contains(SETTING_FINAL_PRICE_INCREASE_PROMO))
                    {
                        return true;
                    }

                    return false;
                }

                return true;
            }
        }

        private bool _finalReminder;

        public PromoPriceIncreaseViewModel(BaseViewModel parent, bool finalReminder) : base(parent)
        {
            _finalReminder = finalReminder;

            Title = R.S(finalReminder ? "PromoPriceIncrease_Title_FinalReminder" : "PromoPriceIncrease_Title_FirstReminder");
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin, Theme.Current.PageMargin, Theme.Current.PageMargin / 2),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = R.S(_finalReminder ? "PromoPriceIncrease_Body_HeaderFinalReminder" : "PromoPriceIncrease_Body_HeaderFirstReminder"),
                                    FontWeight = FontWeights.Bold
                                },
                                new TextBlock
                                {
                                    Text = R.S("PromoPriceIncrease_Body_Description")
                                }
                            }
                        }
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin / 2, Theme.Current.PageMargin, Theme.Current.PageMargin),
                        Children =
                        {
                            new AccentButton
                            {
                                Text = R.S("PromoPriceIncrease_ButtonLearnMore"),
                                Click = () => App.PowerPlannerApp.Current.PromptPurchase(null),
                                Margin = new Thickness(0, 0, 6, 0)
                            }.LinearLayoutWeight(1),

                            new Button
                            {
                                Text = R.S("PromoPriceIncrease_ButtonNoThanks"),
                                Click = RemoveViewModel,
                                Margin = new Thickness(6, 0, 0, 0)
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };
        }

    }
}
