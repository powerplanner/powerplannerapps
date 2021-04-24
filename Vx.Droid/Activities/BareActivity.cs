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

namespace InterfacesDroid.Activities
{
    public class BareActivity : AppCompatActivity
    {
        public event EventHandler<CancelEventArgs> BackPressed;

        // Need to override within activity since overriding Application.AttachBaseContext isn't supported from Xamarin: https://xamarin.github.io/bugzilla-archives/11/11182/bug.html
        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(ViewPumpContextWrapper.Wrap(@base));
        }

        public override void OnBackPressed()
        {
            CancelEventArgs args = new CancelEventArgs();

            BackPressed?.Invoke(this, args);

            if (!args.Cancel)
            {
                base.OnBackPressed();
            }
        }
    }
}