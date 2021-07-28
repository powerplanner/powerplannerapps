using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerUWP.Flyouts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    public sealed partial class TaskOrEventListViewItem : UserControl
    {
        public event EventHandler<ViewItemTaskOrEvent> OnClickItem;

        private const string IMAGE_ATTACHMENT_SYMBOL = "\uD83D\uDCF7";

        public TaskOrEventListViewItem()
        {
            this.InitializeComponent();

            this.DataContextChanged += TaskOrEventListViewItem_DataContextChanged;
        }

        private ViewItemTaskOrEvent _currItem;

        private void TaskOrEventListViewItem_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _currItem = args.NewValue as ViewItemTaskOrEvent;
            
            UpdateDisplayDetails();
            UpdateSubtitlePartTwo();
        }

        private ViewItemTaskOrEvent GetCurrentItem()
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

        private bool _includeClass = true;
        /// <summary>
        /// Gets or sets whether the class name should be displayed in the subtitle. When viewing from within a class, this should be set to false.
        /// </summary>
        public bool IncludeClass
        {
            get => _includeClass;
            set
            {
                if (_includeClass == value)
                {
                    return;
                }

                _includeClass = value;
                TextBoxClass.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                UpdateSubtitlePartTwo();
            }
        }

        private bool _includeDate = true;
        public bool IncludeDate
        {
            get { return _includeDate; }
            set
            {
                if (_includeDate == value)
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

            string txt;

            if (IncludeDate)
            {
                txt = _currItem.GetType().GetProperty("SubtitleDueDate").GetValue(_currItem) as string;
            }
            else
            {
                txt = _currItem.GetType().GetProperty("SubtitleDueTime").GetValue(_currItem) as string;
            }

            if (!IncludeClass)
            {
                txt = txt.Substring(" - ".Length);
            }

            RunSubtitlePartTwo.Text = txt;
        }

        private void Button_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // Dynamically create the context menu upon request
            // (This is to improve performance of large lists, so that
            // a context menu and all bindings aren't created until actually requested)
            MenuFlyout flyout = new TaskOrEventFlyout(_currItem, new TaskOrEventFlyoutOptions
            {
                ShowGoToClass = IncludeClass
            }).GetFlyout();
            
            // Show context flyout
            if (args.TryGetPosition(sender, out Point point))
            {
                flyout.ShowAt(sender as FrameworkElement, point);
            }
            else
            {
                flyout.ShowAt(sender as FrameworkElement);
            }
        }
    }
}
