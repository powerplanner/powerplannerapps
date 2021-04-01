using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Foundation;
using UIKit;
using BareMvvm.Core.App;
using ToolsPortable;
using InterfacesiOS.Extensions;
using CoreGraphics;

namespace InterfacesiOS.App
{
    public abstract class NativeiOSApplication : UIApplicationDelegate
    {
        public static event EventHandler Activated;

        public override UIWindow Window
        {
            get;
            set;
        }

        private static WeakReference<NativeiOSApplication> _current;
        public static NativeiOSApplication Current
        {
            get
            {
                if (_current == null)
                {
                    return null;
                }

                NativeiOSApplication answer;
                _current.TryGetTarget(out answer);
                return answer;
            }
        }

        public ViewManager ViewManager { get; private set; } = new ViewManager();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // These classes won't be linked away because of the code,
            // but we also won't have to construct unnecessarily either,
            // hence the if statement with (hopefully) impossible
            // runtime condition.
            //
            // This is to resolve crash at CultureInfo.CurrentCulture
            // when language is set to Thai. See
            // https://github.com/xamarin/Xamarin.Forms/issues/4037
            if (Environment.CurrentDirectory == "_never_POSSIBLE_")
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
        
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            _current = new WeakReference<NativeiOSApplication>(this);

            // Register the view model to view mappings
            foreach (var mapping in GetViewModelToViewMappings())
            {
                ViewManager.AddMapping(mapping.Key, mapping.Value);
            }

            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new IOSDispatcher(); };

            // Register message dialog
            PortableMessageDialog.Extension = (messageDialog) => { IOSMessageDialog.Show(messageDialog); return Task.FromResult(true); };

            //PortableLocalizedResources.CultureExtension = GetCultureInfo;

            // Initialize the app
            PortableApp.InitializeAsync((PortableApp)Activator.CreateInstance(GetPortableAppType()));

            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
            Activated?.Invoke(null, null);
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }

        public abstract Dictionary<Type, Type> GetViewModelToViewMappings();

        public abstract Type GetPortableAppType();
    }
}
