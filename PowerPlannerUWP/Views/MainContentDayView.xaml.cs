using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerUWP.Views.DayViews;
using System;
using ToolsPortable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainContentDayView : MainScreenContentViewHostGeneric
    {
        public new DayViewModel ViewModel
        {
            get { return base.ViewModel as DayViewModel; }
            set { base.ViewModel = value; }
        }

        public MainContentDayView()
        {
            this.InitializeComponent();

            base.Visibility = Visibility.Collapsed;
        }

#if DEBUG
        ~MainContentDayView()
        {
            System.Diagnostics.Debug.WriteLine("MainContentDayView disposed");
        }
#endif

        private DayView _dayView;

        public override void OnViewModelLoadedOverride()
        {
            try
            {
                base.OnViewModelLoadedOverride();
                
                _dayView = new DayView(ViewModel);
                MainGrid.Children.Add(_dayView);

                base.Visibility = Visibility.Visible;

                ViewModel.BackRequested += new WeakEventHandler<System.ComponentModel.CancelEventArgs>(ViewModel_BackRequested).Handler;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ViewModel_BackRequested(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_dayView.CloseExpandedEvents())
            {
                e.Cancel = true;
            }
        }
    }
}
