using BareMvvm.Core.Snackbar;
using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibraryUnitTests
{
    public class DummyPowerPlannerWindow : INativeAppWindow
    {
        public IBareSnackbarManager SnackbarManager => null;

        public event EventHandler<CancelEventArgs> BackPressed;

        public void Register(PortableAppWindow portableWindow)
        {

        }
    }
}
