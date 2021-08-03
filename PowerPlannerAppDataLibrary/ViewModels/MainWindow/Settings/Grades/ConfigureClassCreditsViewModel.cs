using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassCreditsViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        public ConfigureClassCreditsViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;

            Title = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header");
            UseCancelForBack();
            PrimaryCommand = PopupCommand.Save(Save);

            if (c.Credits == PowerPlannerSending.Grade.NO_CREDITS)
            {
                _credits = new VxState<double?>(null);
            }
            else
            {
                _credits = new VxState<double?>(c.Credits);
            }
        }

        private VxState<double?> _credits;

        protected override View Render()
        {
            return new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Children =
                {
                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header"),
                        FontWeight = FontWeights.Bold,
                        WrapText = false
                    },

                    new NumberTextBox
                    {
                        Number = _credits,
                        PlaceholderText = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.PlaceholderText")
                    }
                }
            };
        }

        public void Save()
        {
            TryStartDataOperationAndThenNavigate(delegate
            {
                double credits = _credits.Value != null ? _credits.Value.Value : PowerPlannerSending.Grade.NO_CREDITS;

                DataChanges changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    Credits = credits
                };

                changes.Add(c);

                return PowerPlannerApp.Current.SaveChanges(changes);

            }, delegate
            {
                this.RemoveViewModel();
            });
        }
    }
}
