using BareMvvm.Core.Snackbar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareMvvm.Core.Windows
{
    public interface INativeAppWindow
    {
        event EventHandler<CancelEventArgs> BackPressed;

        void Register(PortableAppWindow portableWindow);

        BareSnackbarManager SnackbarManager { get; }
    }
}
