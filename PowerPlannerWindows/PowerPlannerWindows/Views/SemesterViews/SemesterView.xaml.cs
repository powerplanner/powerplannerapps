﻿using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
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

namespace PowerPlannerUWP.Views.SemesterViews
{
    public sealed partial class SemesterView : UserControl
    {
        public event EventHandler<ViewItemSemester> OnRequestOpenSemester;
        public event EventHandler<ViewItemSemester> OnRequestEditSemester;

        private ViewItemSemester getSemester()
        {
            return DataContext as ViewItemSemester;
        }

        public SemesterView()
        {
            this.InitializeComponent();
        }

        private void buttonOpenSemester_Click(object sender, RoutedEventArgs e)
        {
            OpenSemester();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Clicking the main item will open the semester too, since that's
            // a more common task than editing. To edit, must click the semester name.
            OpenSemester();
        }

        private void OpenSemester()
        {
            ViewItemSemester s = getSemester();
            if (s == null)
                return;

            OnRequestOpenSemester?.Invoke(this, s);
        }

        private void ButtonName_Click(object sender, RoutedEventArgs e)
        {
            // This will edit the semester
            ViewItemSemester s = getSemester();
            if (s == null)
                return;

            OnRequestEditSemester?.Invoke(this, s);
        }
    }
}
