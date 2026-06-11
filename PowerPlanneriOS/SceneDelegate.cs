#if MACCATALYST
using Foundation;
using UIKit;

namespace PowerPlanneriOS
{
    [Register("SceneDelegate")]
    public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
    {
        [Export("window")]
        public UIWindow? Window { get; set; }

        [Export("scene:willConnectToSession:options:")]
        public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            if (scene is UIWindowScene windowScene)
            {
                Window ??= new UIWindow(windowScene);
                var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
                appDelegate?.RegisterWindowForScene(Window, connectionOptions.ShortcutItem);
            }
        }

        [Export("sceneDidDisconnect:")]
        public void DidDisconnect(UIScene scene) { }

        [Export("sceneDidBecomeActive:")]
        public void DidBecomeActive(UIScene scene) { }

        [Export("sceneWillResignActive:")]
        public void WillResignActive(UIScene scene) { }

        [Export("sceneWillEnterForeground:")]
        public void WillEnterForeground(UIScene scene) { }

        [Export("sceneDidEnterBackground:")]
        public void DidEnterBackground(UIScene scene) { }
    }
}
#endif
