﻿using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassesView : MainScreenContentViewHostGeneric
    {
        public new ClassesViewModel ViewModel
        {
            get { return base.ViewModel as ClassesViewModel; }
            set { base.ViewModel = value; }
        }

        private AppBarButton _appBarAdd;
        public AppBarButton AppBarAdd
        {
            get
            {
                if (_appBarAdd == null)
                {
                    _appBarAdd = CreateAppBarButton(Symbol.Add, LocalizedResources.GetString("SchedulePage_ButtonAddClass/Content"), AppBarAdd_Click);
                }
                return _appBarAdd;
            }
        }

        public ClassesView()
        {
            this.InitializeComponent();

            SetCommandBarPrimaryCommands(AppBarAdd);
        }

        private void AppBarAdd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddClass();
        }
    }
}
