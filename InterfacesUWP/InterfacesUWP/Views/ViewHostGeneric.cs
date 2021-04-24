using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP.Views
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
                foreach (var v in ACTIVE_VIEWHOSTS)
                {
                    result += v.GetType().Name + "\n";
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

    public class ViewHostGeneric : Page
    {
        public ViewHostGeneric()
        {
#if DEBUG
            ActiveViewHosts.Track(this);
            GC.Collect();
#endif
        }

        public BaseViewModel ViewModel
        {
            get { return DataContext as BaseViewModel; }
            set { DataContext = value; OnViewModelSet(); TriggerViewModelLoaded(value); }
        }

        private void OnViewModelSet()
        {
            ViewModel.ViewFocused += new WeakEventHandler<EventArgs>(ViewModel_ViewFocused).Handler;
            ViewModel.ViewLostFocus += new WeakEventHandler<EventArgs>(ViewModel_ViewLostFocus).Handler;

            if (ViewModel.IsFocused)
            {
                OnViewFocused();
            }
            
            OnViewModelSetOverride();
        }

        private void ViewModel_ViewLostFocus(object sender, EventArgs e)
        {
            OnViewLostFocus();
        }

        private void ViewModel_ViewFocused(object sender, EventArgs e)
        {
            OnViewFocused();
        }

        public virtual void OnViewModelSetOverride()
        {
            // Nothing
        }

        public virtual void OnViewModelLoadedOverride()
        {
            // Nothing
        }

        private async void TriggerViewModelLoaded(BaseViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync();
            }
            catch { return; }

            OnViewModelLoadedOverride();
        }

        protected virtual void OnViewFocused()
        {

        }

        protected virtual void OnViewLostFocus()
        {

        }
    }
}
