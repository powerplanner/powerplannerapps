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

struct PPAgendaWidgetView: View {
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
        

        // Group items by their date category while preserving order
        let groupedItems: [(String, [PrimaryWidgetDataItem])] = items.reduce(into: []) { result, item in
            let category = getDateCategory(for: item.date, today: today)

            if let index = result.firstIndex(where: { $0.0 == category }) {
                result[index].1.append(item)
            } else {
                result.append((category, [item]))
            }
        }
        
        GeometryReader { geometry in
            VStack(alignment: .leading, spacing: 0) {
                // Header bar
                HStack {
                    if #available(iOSApplicationExtension 15.0, *) {
                        Text("Agenda")
                            .padding(.horizontal)
                            .padding(.vertical, 8)
                            .foregroundStyle(.white)
                    } else {
                        Text("Agenda")
                            .padding(.horizontal)
                            .padding(.vertical, 8)
                            .foregroundColor(.white)
                    }
                    Spacer()
                    Image("PowerPlannerIcon")
                        .resizable()
                        .frame(width: 16, height: 27)
                        .aspectRatio(contentMode: .fit)
                        .foregroundColor(.white)
                        .padding(.horizontal)
                        .padding(.vertical, 8)
                }
                .background(Color(red: 46/255, green: 54/255, blue: 109/255))
                
                // Items
                VStack(alignment: .leading, spacing: 4) {
                    
                    ForEach(groupedItems, id: \.0) { category, items in
                        dateHeaderView(date: category)
                        ForEach(items.indices, id: \.self) { index in
                            itemView(title: items[index].name, color: items[index].color)
                        }
                        Rectangle().fill(Color.white.opacity(0)).frame(height: 4)
                    }
                }
                .frame(minHeight: 0, maxHeight: /*@START_MENU_TOKEN@*/.infinity/*@END_MENU_TOKEN@*/, alignment: .top)
                .padding(.top, 8)
            }
            .frame(maxHeight: /*@START_MENU_TOKEN@*/.infinity/*@END_MENU_TOKEN@*/, alignment: .top)
        }.background(Color(red: 46/255, green: 54/255, blue: 109/255, opacity: 0.1))
    }
    
    // Date Header View
    func dateHeaderView(date: String) -> some View {
        Text(date)
            .font(.headline)
            .padding(.leading)
            .lineLimit(1)
    }
    
    // Item View with color pillar
    func itemView(title: String, color: [UInt8]) -> some View {

        HStack(spacing: 0) {
            Rectangle().frame(width: 0).padding(.leading)
            Rectangle()
                .fill(Color(red: CGFloat(color[0]) / 255.0, green: CGFloat(color[1]) / 255.0, blue: CGFloat(color[2]) / 255.0))
                .frame(width: 5)
                .cornerRadius(2.0)
            Text(title)
                .padding(.leading, 4)
                .padding(.vertical, 1)
                .lineLimit(1)
                .font(.callout)
        }.frame(height: 24, alignment: .leading)//.widgetURL(urlInApp("item"))
    }
    
    
    
    func urlInApp(_ name: String) -> URL {
        let activity: NSUserActivity = NSUserActivity.init(activityType: "ViewEventIntent")

        activity.title = "widget_PreviewEvent"
        activity.userInfo = [:]
        activity.becomeCurrent()

        let urlString: String = "widget_PreviewEvent" + name.addingPercentEncoding(withAllowedCharacters: .urlHostAllowed)!
        return URL(string: urlString)!
    }

    // Helper function to categorize the dates
    func getDateCategory(for date: Date, today: Date) -> String {
        let calendar = Calendar.current
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "E, MMM d" // Format for future dates like "Fri, Aug 30"
        let dayFormatter = DateFormatter()
        dayFormatter.dateFormat = "EEEE"
        
        if (calendar.startOfDay(for: date) < today) {
            return "In the past"
        }
        
        let daysBetween = calendar.dateComponents([.day], from: today, to: date).day!
        
        if daysBetween == 0 {
            return "Today"
        } else if daysBetween == 1 {
            return "Tomorrow"
        } else {
            switch daysBetween {
            case 2:
                return "In two days"
            case 3...6:
                return "This " + dayFormatter.string(from: date) // Return day of the week like "This Thursday"
            case 7...13:
                return "Next " + dayFormatter.string(from: date) // "Next Friday"
            default:
                return dateFormatter.string(from: date) // Short date name
            }
        }
    }
}

@available(iOSApplicationExtension 15.0, *)
@main
struct NativeWidget: Widget {
    private let kind: String = "NativeWidget"

    public var body: some WidgetConfiguration {
        IntentConfiguration(kind: kind, intent: ConfigurationIntent.self, provider: Provider()) { entry in
            PPAgendaWidgetView(entry: entry)
            }
            .contentMarginsDisabled()
            .configurationDisplayName("Power Planner Agenda")
            .description("Displays your upcoming tasks and events.")
            .supportedFamilies([.systemSmall, .systemMedium, .systemLarge])
    }
}
