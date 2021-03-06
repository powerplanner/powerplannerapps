﻿using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerUWP.Views.TaskOrEventViews;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using ToolsUniversal;
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
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AgendaView : MainScreenContentViewHostGeneric
    {
        public new AgendaViewModel ViewModel
        {
            get { return base.ViewModel as AgendaViewModel; }
            set { base.ViewModel = value; }
        }

        public AgendaView()
        {
            this.InitializeComponent();
        }

#if DEBUG
        ~AgendaView()
        {
            System.Diagnostics.Debug.WriteLine("AgendaView disposed");
        }
#endif

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                SetCommandBarCommands(new ICommandBarElement[]
                {
                AppBarAdd
                }, null);

                MainGridView.ItemsSource = ViewModel.ItemsWithHeaders;
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private AppBarButton _appBarAdd;
        private AppBarButton AppBarAdd
        {
            get
            {
                if (_appBarAdd == null)
                    _appBarAdd = CreateAppBarButton(Symbol.Add, LocalizedResources.Common.GetStringNewItem(), appBarAdd_Click);

                return _appBarAdd;
            }
        }

        private void appBarAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ShowFlyoutAddTaskOrEvent(
                    elToCenterFrom: sender as FrameworkElement,
                    addTaskAction: ViewModel.AddTask,
                    addEventAction: ViewModel.AddEvent);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
