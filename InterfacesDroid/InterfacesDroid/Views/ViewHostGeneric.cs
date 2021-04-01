using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using ToolsPortable;

namespace InterfacesDroid.Views
{
#if DEBUG
    internal static class ActiveViewHosts
    {
        private static readonly WeakReferenceList<ViewHostGeneric> ACTIVE_VIEWHOSTS = new WeakReferenceList<ViewHostGeneric>();

        static ActiveViewHosts()
        {
            OutputActiveViewHosts();
        }

        public static void Track(ViewHostGeneric viewHost)
        {
            ACTIVE_VIEWHOSTS.Add(viewHost);
        }

        private static string _prevResult;
        private static async void OutputActiveViewHosts()
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(1500);

                string result = "";
                int count = 0;
                foreach (var c in ACTIVE_VIEWHOSTS)
                {
                    result += c.GetType().Name + "\n";
                    count++;
                }
                result = $"ACTIVE VIEW HOSTS ({count})\n" + result;
                if (_prevResult != result)
                {
                    _prevResult = result;
                    System.Diagnostics.Debug.WriteLine(result);
                }
            }
        }
    }
#endif

    public abstract class ViewHostGeneric : InflatedViewWithBinding
    {
        public BaseViewModel ViewModel
        {
            get { return DataContext as BaseViewModel; }
            set { DataContext = value; OnViewModelSetOverride(); TriggerViewModelLoaded(value); }
        }

        public ViewHostGeneric(int resourceId, ViewGroup root) : base(resourceId, root)
        {
            OnViewCreated();

#if DEBUG
            ActiveViewHosts.Track(this);
#endif
        }

        public virtual void OnViewModelSetOverride()
        {
            // Nothing
        }

        public virtual void OnViewModelLoadedOverride()
        {
            // Nothing
        }

        protected virtual void OnViewCreated()
        {
            // Nothing
        }

        private async void TriggerViewModelLoaded(BaseViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync();
            }
            catch
#if DEBUG
            (Exception ex)
#endif
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
#endif
                return;
            }

            OnViewModelLoadedOverride();
        }
    }
}