using Foundation;
using System;
using System.Threading.Tasks;
using ToolsPortable;
using UIKit;
using Vx.iOS;

namespace VxSampleApp.iOS
{
    public static class ViewExtensions
    {
        public static UIView StretchWidthAndHeight(this UIView view, UIView parentView, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            // https://gist.github.com/twostraws/a02d4cc09fc7bc16859c
            // http://commandshift.co.uk/blog/2013/01/31/visual-format-language-for-autolayout/
            //parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]-({right})-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("view", view)));
            //parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]-({bottom})-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("view", view)));

            view.StretchWidth(parentView, left: left, right: right);
            view.StretchHeight(parentView, top: top, bottom: bottom);

            return view;
        }
        public static UIView StretchWidth(this UIView view, UIView parentView, float left = 0, float right = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]-({right})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView StretchHeight(this UIView view, UIView parentView, float top = 0, float bottom = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]-({bottom})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }
    }
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            PortableDispatcher.ObtainDispatcherFunction = () => new IOSDispatcher(this);

            // create a new window instance based on the screen size
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            Window.RootViewController = new UIViewController();

            var view = new VxCombinedComponent().Render();
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            Window.RootViewController.View.AddSubview(view);
            view.StretchWidthAndHeight(Window.RootViewController.View, 48, 48, 48, 48);

            // make the window visible
            Window.MakeKeyAndVisible();

            return true;
        }

        internal class IOSDispatcher : PortableDispatcher
        {
            private UIApplicationDelegate _appDelegate;

            public IOSDispatcher(UIApplicationDelegate appDelegate)
            {
                _appDelegate = appDelegate;
            }

            public override Task RunAsync(Action codeToExecute)
            {
                TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

                Action actionWithAwait = delegate
                {
                    try
                    {
                        codeToExecute();
                        completionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                };

                // Asynchronously run it, and we'll flag when our code completed
                _appDelegate.BeginInvokeOnMainThread(actionWithAwait);

                return completionSource.Task;
            }
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
            // If your application supports background execution this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transition from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}


