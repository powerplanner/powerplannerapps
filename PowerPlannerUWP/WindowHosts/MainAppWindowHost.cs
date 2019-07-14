using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace PowerPlannerUWP.WindowHosts
{
    public class MainAppWindowHost
    {
        public Window Window { get; private set; }

        public MainAppWindowHost(Window window)
        {
            Window = window;
        }
    }
}
