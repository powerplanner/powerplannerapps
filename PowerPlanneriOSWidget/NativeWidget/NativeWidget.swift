//
//  NativeWidget.swift
//  NativeWidget
//
//  Created by Chris Hamons on 6/30/20.
//

import WidgetKit
import SwiftUI
import Intents

struct Provider: IntentTimelineProvider {
    public func placeholder(in context: Context) -> DataEntry {
        return DataEntry(value: "0.0", delta: "0.0", date: Date(), configuration: Provider.Intent.init())
    }
    
    public func getSnapshot(for configuration: ConfigurationIntent, in context: Context, completion: @escaping (DataEntry) -> Void) {
        let entry = DataEntry(value: "0.0", delta: "0.0", date: Date(), configuration: configuration)
        completion(entry)
    }
    
    public func getTimeline(for configuration: ConfigurationIntent, in context: Context, completion: @escaping (Timeline<DataEntry>) -> Void) {
        var entries: [DataEntry] = []

        let jsonData = readTestData();
        for entry in jsonData.data {
            let dateFormatter = DateFormatter()
            dateFormatter.locale = Locale(identifier: "en_US_POSIX") // set locale to reliable US_POSIX
            dateFormatter.dateFormat = "yyyy-MM-dd"
            let date = dateFormatter.date(from: entry.self.key);

            entries.append(DataEntry(value: entry.value.value, delta: entry.value.delta, date: date!, configuration: configuration))
        }
        
        let timeline = Timeline(entries: entries, policy: .after (Date().addingTimeInterval(15 * 60)))
        completion(timeline)
    }
    
    typealias Entry = DataEntry
    typealias Intent = ConfigurationIntent
}

struct DataEntry: TimelineEntry {
    public let value: String
    public let delta: String
    
    public let date: Date
    public let configuration: ConfigurationIntent
}

struct PlaceholderView : View {
    var body: some View {
        VStack {
            Text("")
        }
    }
}

struct NativeWidgetEntryView : View {
    var entry: Provider.Entry

    var body: some View {
        VStack {
            Text("Xamarin.iOS + SwiftUI")
            Text(entry.date, style: .date)
            Text(entry.value)
            Text(entry.delta)
        }
    }
}

@main
struct NativeWidget: Widget {
    private let kind: String = "NativeWidget"

    public var body: some WidgetConfiguration {
        IntentConfiguration(kind: kind, intent: ConfigurationIntent.self, provider: Provider()) { entry in
            NativeWidgetEntryView(entry: entry)
            }
            .configurationDisplayName("Xamarin.iOS Example")
            .description("This is an example widget embedded in Xamarin.iOS Container.")
            .supportedFamilies([.systemSmall, .systemMedium, .systemLarge])
    }
}
