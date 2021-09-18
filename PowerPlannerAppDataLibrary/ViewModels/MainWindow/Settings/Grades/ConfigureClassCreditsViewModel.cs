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
                Credits = null;
            }
            else
            {
                Credits = c.Credits;
            }
        }

        public double? Credits { get => GetState<double?>(); set => SetState(value); }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header"),
                    FontWeight = FontWeights.Bold,
                    WrapText = false
                },

                new NumberTextBox
                {
                    Number = VxValue.Create(Credits, v => Credits = v),
                    PlaceholderText = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.PlaceholderText")
                }
            );
        }

        public void Save()
        {
            TryStartDataOperationAndThenNavigate(delegate
            {
                double credits = Credits != null ? Credits.Value : PowerPlannerSending.Grade.NO_CREDITS;

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
