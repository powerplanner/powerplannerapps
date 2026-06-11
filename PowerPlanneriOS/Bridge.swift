#if !targetEnvironment(macCatalyst)
import WidgetKit
#endif

@_cdecl("ReloadWidgets")
public func ReloadWidgets() {
    #if !targetEnvironment(macCatalyst)
    if #available(iOS 14, *) {
        WidgetCenter.shared.reloadAllTimelines()
    }
    #endif
}