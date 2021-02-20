using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public class VxViewHostGeneric : VxView
    {
        public BaseViewModel ViewModel
        {
            get { return DataContext as BaseViewModel; }
            set { DataContext = value; OnViewModelSetOverride(); TriggerViewModelLoaded(value); }
        }

        public VxViewHostGeneric(Context context) : base(context)
        {
            OnViewCreated();

#if DEBUG
            //ActiveViewHosts.Track(this);
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