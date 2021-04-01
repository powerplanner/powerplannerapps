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
using BareMvvm.Core.App;
using System.Threading.Tasks;
using BareMvvm.Core.Windows;
using InterfacesDroid.Windows;
using InterfacesDroid.ViewModelPresenters;
using ToolsPortable;
using InterfacesDroid.Activities;
using InterfacesDroid.Extensions;
using System.Globalization;
using IO.Github.Inflationx.Viewpump;
using InterfacesDroid.Bindings;

namespace InterfacesDroid.App
{
    public abstract class NativeDroidApplication : Application
    {
        private static WeakReference<NativeDroidApplication> _current;
        public static NativeDroidApplication Current
        {
            get
            {
                if (_current == null)
                {
                    return null;
                }

                NativeDroidApplication answer;
                _current.TryGetTarget(out answer);
                return answer;
            }
        }

        protected NativeDroidApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            // Register the calling assembly (the app) as a ValueConverter source
            BareMvvm.Core.Bindings.BindingApplicator.RegisterAssembly(System.Reflection.Assembly.GetCallingAssembly());

            // These classes won't be linked away because of the code,
            // but we also won't have to construct unnecessarily either,
            // hence the if statement with (hopefully) impossible
            // runtime condition.
            //
            // This is to resolve crash at CultureInfo.CurrentCulture
            // when language is set to Thai. See
            // https://github.com/xamarin/Xamarin.Forms/issues/4037
            if (System.Environment.CurrentDirectory == "_never_POSSIBLE_")
            {
                new System.Globalization.ChineseLunisolarCalendar();
                new System.Globalization.HebrewCalendar();
                new System.Globalization.HijriCalendar();
                new System.Globalization.JapaneseCalendar();
                new System.Globalization.JapaneseLunisolarCalendar();
                new System.Globalization.KoreanCalendar();
                new System.Globalization.KoreanLunisolarCalendar();
                new System.Globalization.PersianCalendar();
                new System.Globalization.TaiwanCalendar();
                new System.Globalization.TaiwanLunisolarCalendar();
                new System.Globalization.ThaiBuddhistCalendar();
                new System.Globalization.UmAlQuraCalendar();
            }
        }

        public override void OnCreate()
        {
            // This will be called whenever anything happens for the first time - including a receiver or service being started.

            _current = new WeakReference<NativeDroidApplication>(this);

            ViewPump.Init(ViewPump.InvokeBuilder()
                .AddInterceptor(new BindingInterceptor())
                .Build());

            // Register the view model to view mappings
            foreach (var mapping in GetViewModelToViewMappings())
            {
                ViewModelToViewConverter.AddMapping(mapping.Key, mapping.Value);
            }

            // Register splash mappings
            foreach (var mapping in GetViewModelToSplashMappings())
            {
                ViewModelToViewConverter.AddSplashMapping(mapping.Key, mapping.Value);
            }

            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new AndroidDispatcher(); };
            
            // Register message dialog
            PortableMessageDialog.Extension = (messageDialog) => { AndroidMessageDialog.Show(messageDialog); return Task.FromResult(true); };

            PortableLocalizedResources.CultureExtension = GetCultureInfo;

            // Initialize the app
            PortableApp.InitializeAsync((PortableApp)Activator.CreateInstance(GetPortableAppType()));

            base.OnCreate();
        }

        // Hmm overriding AttachBaseContext isn't supported from Xamarin: https://xamarin.github.io/bugzilla-archives/11/11182/bug.html
        //protected override void AttachBaseContext(Context @base)
        //{
        //    base.AttachBaseContext(ViewPumpContextWrapper.Wrap(@base));
        //}

        public abstract Dictionary<Type, Type> GetViewModelToViewMappings();
        public abstract Dictionary<Type, Type> GetViewModelToSplashMappings();

        public abstract Type GetPortableAppType();

        private CultureInfo GetCultureInfo()
        {
            // For now, we're just going to leave it en-US for safety, since we're not sure it'll work well in other locales
            return new CultureInfo("en-US");

            // https://github.com/conceptdev/xamarin-forms-samples/blob/master/TodoL10nResx/PCL/Todo.Android/Locale_Android.cs
            //var androidLocale = Java.Util.Locale.Default;

            //string dotNetLocale = androidLocale.ToString().Replace('_', '-');

            //return new CultureInfo(dotNetLocale);
        }
    }
}
