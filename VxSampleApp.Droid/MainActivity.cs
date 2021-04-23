using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using System;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Droid;

namespace VxSampleApp.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            PortableDispatcher.ObtainDispatcherFunction = () => new DroidDispatcher(this);
            VxDroidExtensions.ApplicationContext = this;

            var nativeView = new VxGradeOptionsComponent().Render();
            nativeView.LayoutParameters = new Android.Views.ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            SetContentView(nativeView);
        }

        private class DroidDispatcher : PortableDispatcher
        {
            private Activity _activity;
            public DroidDispatcher(Activity activity)
            {
                _activity = activity;
            }

            public override Task RunAsync(Action codeToExecute)
            {
                _activity.RunOnUiThread(codeToExecute);
                return Task.FromResult(true);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}