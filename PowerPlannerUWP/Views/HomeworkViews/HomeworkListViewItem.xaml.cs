using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.HomeworkViews
{
    public sealed partial class HomeworkListViewItem : UserControl
    {
        public event EventHandler<BaseViewItemHomeworkExam> OnClickItem;

        private const string IMAGE_ATTACHMENT_SYMBOL = "\uD83D\uDCF7";

        public HomeworkListViewItem()
        {
            this.InitializeComponent();

            this.DataContextChanged += HomeworkListViewItem_DataContextChanged;
        }

        private BaseViewItemHomeworkExam _currItem;

        private void HomeworkListViewItem_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _currItem = args.NewValue as BaseViewItemHomeworkExam;
            
            UpdateDisplayDetails();
            UpdateSubtitlePartTwo();
        }

        private BaseViewItemHomeworkExam GetCurrentItem()
        {
            return _currItem;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnClickItem != null)
            {
                OnClickItem(sender, GetCurrentItem());
            }
            else
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(GetCurrentItem());
            }
        }

        private void UpdateDisplayDetails()
        {
            if (_currItem == null)
            {
                textBlockDetails.Visibility = Visibility.Collapsed;
                return;
            }

            // Has image attachment
            if (_currItem.ImageNames != null && _currItem.ImageNames.Length > 0)
            {
                if (string.IsNullOrWhiteSpace(_currItem.Details))
                    textBlockDetails.Text = IMAGE_ATTACHMENT_SYMBOL + " Image Attachment";
                else
                    textBlockDetails.Text = IMAGE_ATTACHMENT_SYMBOL + " " + _currItem.Details.Trim();
            }

            // No image attachment
            else
            {
                // No details
                if (string.IsNullOrWhiteSpace(_currItem.Details))
                {
                    textBlockDetails.Visibility = Visibility.Collapsed;
                    return;
                }

                textBlockDetails.Text = _currItem.Details.Trim();
            }

            // Make it visible since it has content
            textBlockDetails.Visibility = Visibility.Visible;
        }

        private bool _includeDate = true;
        public bool IncludeDate
        {
            get { return _includeDate; }
            set
            {
                if (_includeDate != value)
                {
                    return;
                }

                _includeDate = value;
                UpdateSubtitlePartTwo();
            }
        }

        private void UpdateSubtitlePartTwo()
        {
            if (_currItem == null)
            {
                return;
            }

            if (IncludeDate)
            {
                RunSubtitlePartTwo.Text = _currItem.GetType().GetProperty("SubtitleDueDate").GetValue(_currItem) as string;
            }
            else
            {
                RunSubtitlePartTwo.Text = _currItem.GetType().GetProperty("SubtitleDueTime").GetValue(_currItem) as string;
            }
        }
    }
}
