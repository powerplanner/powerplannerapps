using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
