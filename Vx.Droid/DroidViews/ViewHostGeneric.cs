using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using BareMvvm.Core.ViewModels;
using InterfacesDroid.Helpers;
using InterfacesDroid.Themes;
using InterfacesDroid.ViewModelPresenters;
using InterfacesDroid.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Droid;

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

    public class ViewHostGeneric : InflatedViewWithBindingHost, IViewModelHost
    {
        public BaseViewModel ViewModel
        {
            get { return DataContext as BaseViewModel; }
            set
            {
                DataContext = value;

                if (value != null)
                {
                    UpdateNookInsets();
                }

                if (_renderViewModel && value != null)
                {
                    var nativeView = ViewModel.Render();
                    nativeView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                    AddView(nativeView);

                    if (ViewModel.ImportantForAutofill)
                    {
                        AutofillHelper.EnableForAll(nativeView);
                    }
                }

                OnViewModelSetOverride();
                TriggerViewModelLoaded(value);
            }
        }

        public ViewHostGeneric(int resourceId, ViewGroup root) : base(resourceId, root)
        {
            Initialize();
            OnViewCreated();
        }

        public ViewHostGeneric(ViewGroup root) : base(root.Context) { Initialize(); }

        private bool _renderViewModel;

        public ViewHostGeneric(ViewGroup root, bool renderViewModel) : base(root.Context)
        {
            _renderViewModel = renderViewModel;
            Initialize();
        }

        private void Initialize()
        {
            NativeDroidAppWindow.WindowInsetsChanged += new WeakEventHandler<WindowInsetsCompat>(NativeDroidAppWindow_WindowInsetsChanged).Handler;

#if DEBUG
            ActiveViewHosts.Track(this);
#endif
        }

        private void NativeDroidAppWindow_WindowInsetsChanged(object sender, WindowInsetsCompat e)
        {
            UpdateNookInsets();
        }

        private void UpdateNookInsets()
        {
            try
            {
                if (ViewModel != null && NativeDroidAppWindow.WindowInsets != null)
                {
                    var insets = NativeDroidAppWindow.WindowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
                    if (insets != null)
                    {
                        ViewModel.UpdateNookInsets(new Vx.Views.Thickness
                        {
                            Left = ThemeHelper.FromPxPrecise(Context, insets.Left),
                            Top = ThemeHelper.FromPxPrecise(Context, insets.Top),
                            Right = ThemeHelper.FromPxPrecise(Context, insets.Right),
                            Bottom = ThemeHelper.FromPxPrecise(Context, insets.Bottom)
                        });
                    }
                }
            }
            catch { }
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

        protected override void OnDetachedFromWindow()
        {
            ViewModel?.PauseRendering();
            base.OnDetachedFromWindow();
        }

        protected override void OnAttachedToWindow()
        {
            ViewModel?.ResumeRendering();
            base.OnAttachedToWindow();
        }
    }
}