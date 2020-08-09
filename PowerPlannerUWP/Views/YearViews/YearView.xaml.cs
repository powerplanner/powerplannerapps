using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.YearViews
{
    public sealed partial class YearView : UserControl
    {
        /// <summary>
        /// Only new (not added) years will trigger this method when they need to be removed from the view
        /// 
        /// Triggered by
        ///  - Saving a new item
        ///  - Canceling a new item
        ///  - Deleting a new item
        /// </summary>
        public event EventHandler OnRequestRemove;

        public event EventHandler<ViewItemSemester> OnRequestOpenSemester;
        public event EventHandler<ViewItemSemester> OnRequestEditSemester;
        public event EventHandler<ViewItemYear> OnRequestAddSemester;
        public event EventHandler<ViewItemYear> OnRequestEditYear;

        public YearView()
        {
            this.InitializeComponent();
        }

        private ViewItemYear getYear()
        {
            return DataContext as ViewItemYear;
        }

        private bool isNewItem()
        {
            return (DataContext is ViewItemYear) == false;
        }
        
        private void requestRemove()
        {
            if (OnRequestRemove != null)
                OnRequestRemove(this, null);
        }
        
        private void buttonAddSemester_Click(object sender, RoutedEventArgs e)
        {
            var year = getYear();
            if (year == null)
                return;

            OnRequestAddSemester?.Invoke(this, year);
        }

        private void ButtonTitleAndGPA_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var year = getYear();
                if (year == null)
                    return;

                OnRequestEditYear?.Invoke(this, year);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void SemesterView_OnRequestEditSemester(object sender, ViewItemSemester e)
        {
            OnRequestEditSemester?.Invoke(this, e);
        }

        private void SemesterView_OnRequestOpenSemester(object sender, ViewItemSemester e)
        {
            OnRequestOpenSemester?.Invoke(this, e);
        }
    }
}
