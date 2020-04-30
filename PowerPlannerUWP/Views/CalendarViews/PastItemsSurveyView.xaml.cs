using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Calendar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.CalendarViews
{
    public sealed partial class PastItemsSurveyView : PopupViewHostGeneric
    {
        public new PastItemsSurveyViewModel ViewModel
        {
            get => base.ViewModel as PastItemsSurveyViewModel;
            set => base.ViewModel = value;
        }

        public PastItemsSurveyView()
        {
            this.InitializeComponent();
        }

        private string GetDoYouMissResponse()
        {
            if (DoYouMissYes.IsChecked.GetValueOrDefault())
            {
                return "Yes";
            }

            if (DoYouMissNo.IsChecked.GetValueOrDefault())
            {
                return "No";
            }

            if (DoYouMissDidntNotice.IsChecked.GetValueOrDefault())
            {
                return "DidntNotice";
            }

            return null;
        }

        private string GetVisibilityOption()
        {
            if (VisibilityAlways.IsChecked.GetValueOrDefault())
            {
                return "Always";
            }

            if (VisibilityToggle.IsChecked.GetValueOrDefault())
            {
                return "Toggle";
            }

            return null;
        }

        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> customProperties = new Dictionary<string, string>();

            string doYouMissResponse = GetDoYouMissResponse();
            if (doYouMissResponse == null)
            {
                ShowIncomplete();
                return;
            }

            customProperties["DoYouMiss"] = doYouMissResponse;

            if (DoYouMissYes.IsChecked.GetValueOrDefault())
            {
                string visibilityOption = GetVisibilityOption();
                if (visibilityOption == null)
                {
                    ShowIncomplete();
                    return;
                }

                customProperties["VisibilityOption"] = visibilityOption;

                if (!string.IsNullOrWhiteSpace(Explanation.Text))
                {
                    customProperties["Explanation"] = Explanation.Text.Trim();
                }
            }

            TelemetryExtension.Current?.TrackEvent("PastItemsSurveyResponse", customProperties);

            ViewModel.FinishedSubmitting();
        }

        private void ShowIncomplete()
        {
            _ = new MessageDialog("Please answer all questsions", "Incomplete response").ShowAsync();
        }
    }
}
