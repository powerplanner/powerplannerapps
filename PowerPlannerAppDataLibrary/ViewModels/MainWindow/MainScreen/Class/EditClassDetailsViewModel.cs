using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class EditClassDetailsViewModel : BaseMainScreenViewModelChild
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ViewItemClass Class { get; private set; }

        private string _details = "";
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        public EditClassDetailsViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Details = c.Details != null ? c.Details : "";
        }

        public void Save()
        {
            TryStartDataOperationAndThenNavigate(delegate
            {
                string details = Details.Trim();
                
                DataItemClass c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    Details = details
                };

                DataChanges editChanges = new DataChanges();
                editChanges.Add(c);
                return PowerPlannerApp.Current.SaveChanges(editChanges);

            }, delegate
            {
                this.GoBack();
            });
        }
    }
}
