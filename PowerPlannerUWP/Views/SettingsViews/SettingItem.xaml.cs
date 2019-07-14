using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace PowerPlannerUWP.Views.SettingsViews
{
    public sealed partial class SettingItem : UserControl
    {
        public SettingItem()
        {
            this.InitializeComponent();
        }

        public string Header
        {
            get { return tbHeader.Text; }
            set { tbHeader.Text = value; }
        }

        public UIElement ExpandedContent
        {
            get { return expandedContentContainer.Child; }
            set { expandedContentContainer.Child = value; }
        }

        public bool IsExpanded
        {
            get { return expandedContentContainer.Visibility == Visibility.Visible; }
            set { expandedContentContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
        }
    }
}
