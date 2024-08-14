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
        return DataEntry(items: [PrimaryWidgetDataItem(name: "Bookwork", color: [155, 42, 155], date: Date())], date: Date(), configuration: Provider.Intent.init())
    }
    
    public func getSnapshot(for configuration: ConfigurationIntent, in context: Context, completion: @escaping (DataEntry) -> Void) {
        let entry = DataEntry(items: [PrimaryWidgetDataItem(name: "Bookwork", color: [155, 42, 155], date: Date())], date: Date(), configuration: configuration)
        completion(entry)
    }
    
    public func getTimeline(for configuration: ConfigurationIntent, in context: Context, completion: @escaping (Timeline<DataEntry>) -> Void) {
        var entries: [DataEntry] = []
        let calendar = Calendar.current
        let today = calendar.startOfDay(for: Date())

        var items = readPrimaryData()
        if (items.isEmpty) {
            items.append(PrimaryWidgetDataItem(
                        name: "No items present",
                        color: [0, 150, 250],  // Example RGB values normalized (0.0 to 1.0)
                        date: Date()
                    )
                )
        }
        
        // Make 14 timeline entries, one for each day (starting with today and continuing into the future), where each entry contains the items grouped by their relative due date relative to the date for that entry. Any overdue items (items due the day before) should be grouped
        // Assuming you want one entry per item in jsonData
        for i in 0..<14 {
            let dateForEntry = calendar.date(byAdding: .day, value: i, to: today)!;
            let entry = DataEntry(items: items, date: dateForEntry, configuration: configuration);
            entries.append(entry);
        }
        
        // Set the refresh policy.
        let timeline = Timeline(entries: entries, policy: .never)
        completion(timeline)
        
        
    }
    
    typealias Entry = DataEntry
    typealias Intent = ConfigurationIntent
}

struct DataEntry: TimelineEntry {
    public let items: [PrimaryWidgetDataItem]
    
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
    var entry: DataEntry

    var body: some View {
        let items = entry.items // Sorted by date ascending
        let today = entry.date
        
        // Render the items grouped by date headers. The first bucket should be an "Overdue" bucket of items due before today. The subsequent buckets should simply be bucketed by their date. Use relative names like "Today" and "Tomorrow" and "In two days", and then after that use "This Friday" or "Next Friday", and then after that just use the short date name like "Fri, Aug 30".
        // For example...
        // Overdue
        // Bookwork Pg 36
        // Essay 1
        // Tomorrow
        // Bookwork Pg 40
        // This Thursday
        // Essay 3
        // Quiz 1
        // Mon, Sep 1
        // Essay 4
        
        // Group items by their date category
        let groupedItems = Dictionary(grouping: items) { item in
            getDateCategory(for: item.date, today: today)
        }
        
        VStack(alignment: .leading) {
            ForEach(groupedItems.keys.sorted(), id: \.self) { category in
                if let itemsInCategory = groupedItems[category] {
                    Text(category).font(.headline)
                    ForEach(itemsInCategory.indices, id: \.self) { index in
                        Text(itemsInCategory[index].name)
                    }
                }
            }
        }
        .padding()
    }
    
    // Helper function to categorize the dates
    func getDateCategory(for date: Date, today: Date) -> String {
        let calendar = Calendar.current
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "E, MMM d" // Format for future dates like "Fri, Aug 30"
        
        if date < today {
            return "Overdue"
        } else if calendar.isDateInToday(date) {
            return "Today"
        } else if calendar.isDateInTomorrow(date) {
            return "Tomorrow"
        } else if let daysBetween = calendar.dateComponents([.day], from: today, to: date).day {
            switch daysBetween {
            case 2:
                return "In two days"
            case 3...6:
                return dateFormatter.string(from: date) // Return day of the week like "This Thursday"
            case 7...13:
                return "Next " + dateFormatter.string(from: date) // "Next Friday"
            default:
                return dateFormatter.string(from: date) // Short date name
            }
        }
        return dateFormatter.string(from: date)
    }
}

@main
struct NativeWidget: Widget {
    private let kind: String = "NativeWidget"

    public var body: some WidgetConfiguration {
        IntentConfiguration(kind: kind, intent: ConfigurationIntent.self, provider: Provider()) { entry in
            NativeWidgetEntryView(entry: entry)
            }
            .configurationDisplayName("Power Planner Agenda")
            .description("Displays your upcoming tasks and events.")
            .supportedFamilies([.systemSmall, .systemMedium, .systemLarge])
    }
}
