using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGpaTypeViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemClass Class { get; private set; }

        private GpaType m_gpaType;
        public GpaType GpaType
        {
            get { return m_gpaType; }
            set
            {
                if (m_gpaType != value)
                {
                    m_gpaType = value;
                    OnPropertyChanged(nameof(GpaType), nameof(IsStandard), nameof(IsPassFail));
                    Save();
                }
            }
        }

        public bool IsStandard
        {
            get { return GpaType == GpaType.Standard; }
            set
            {
                if (value)
                {
                    GpaType = GpaType.Standard;
                }
            }
        }

        public bool IsPassFail
        {
            get { return GpaType == GpaType.PassFail; }
            set
            {
                if (value)
                {
                    GpaType = GpaType.PassFail;
                }
            }
        }

        public bool IsEnabled { get; set; } = true;

        public ConfigureClassGpaTypeViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Class.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Class_PropertyChanged).Handler;
            m_gpaType = c.GpaType;

        }

        private void Class_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Class.GpaType):
                    SetProperty(ref m_gpaType, Class.GpaType, nameof(GpaType), nameof(IsStandard), nameof(IsPassFail));
                    break;
            }
        }

        private async void Save()
        {
            try
            {
                IsEnabled = false;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    GpaType = GpaType
                };

                changes.Add(c);

                await TryHandleUserInteractionAsync("Save", async delegate
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);

                }, "Failed to save. Your error has been reported.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }

            finally
            {
                IsEnabled = true;
            }
        }
    }
}
