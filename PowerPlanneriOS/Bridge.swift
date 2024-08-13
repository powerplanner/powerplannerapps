import WidgetKit

@_cdecl("ReloadWidgets")
public func ReloadWidgets ()  {
    if #available(iOS 14.0, *) {
        WidgetCenter.shared.reloadAllTimelines()
    }
}