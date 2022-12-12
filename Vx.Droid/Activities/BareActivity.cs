using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using AndroidX.AppCompat.App;
using IO.Github.Inflationx.Viewpump;
using Android.Window;
using AndroidX.Activity;

namespace InterfacesDroid.Activities
{
    public class BareActivity : AppCompatActivity
    {
        private class BackCallback : OnBackPressedCallback
        {
            public BareActivity Activity { get; private set; }
            
            public BackCallback(BareActivity activity) : base(true)
            {
                Activity = activity;
            }

            public override void HandleOnBackPressed()
            {
                CancelEventArgs args = new CancelEventArgs();

                Activity.BackPressed?.Invoke(this, args);
            }
        }

        private BackCallback _backCallback;
        public event EventHandler<CancelEventArgs> BackPressed;

        public BareActivity()
        {
            _backCallback = new BackCallback(this);
            OnBackPressedDispatcher.AddCallback(_backCallback);
        }

        public bool IsBackEnabled
        {
            get => _backCallback.Enabled;
            set => _backCallback.Enabled = value;
        }

        // Need to override within activity since overriding Application.AttachBaseContext isn't supported from Xamarin: https://xamarin.github.io/bugzilla-archives/11/11182/bug.html
        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(ViewPumpContextWrapper.Wrap(@base));
        }
    }
}