using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class SyncErrorsViewModel : PopupComponentViewModel
    {
        public SyncErrorsViewModel(BaseViewModel parent, LoggedError[] syncErrors) : base(parent)
        {
            SyncErrors = syncErrors;
            Title = "Sync errors";
        }

        public LoggedError[] SyncErrors { get; private set; }

        protected override View Render()
        {
            var views = new List<View>();

            foreach (var error in SyncErrors)
            {
                views.Add(new TextBlock
                {
                    Text = error.Name,
                    IsTextSelectionEnabled = true,
                    Margin = new Thickness(0, views.Count > 0 ? 18 : 0, 0, 0)
                });

                views.Add(new TextBlock
                {
                    Text = error.Date.ToString(),
                    IsTextSelectionEnabled = true
                }.CaptionStyle());

                views.Add(new TextBlock
                {
                    Text = error.Message,
                    IsTextSelectionEnabled = true
                }.CaptionStyle());
            }

            return RenderGenericPopupContent(views);
        }
    }
}
