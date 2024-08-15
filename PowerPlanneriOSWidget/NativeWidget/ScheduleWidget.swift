//
//  NativeWidget.swift
//  NativeWidget
//
//  Created by Chris Hamons on 6/30/20.
//

import WidgetKit
import SwiftUI
import Intents

struct ScheduleProvider: IntentTimelineProvider {
    public func placeholder(in context: Context) -> ScheduleDataEntry {
        return getSampleEntry(in: Provider.Intent.init());
    }
    
    public func getSnapshot(for configuration: ConfigurationIntent, in context: Context, completion: @escaping (ScheduleDataEntry) -> Void) {
        let entry = getSampleEntry(in: configuration);
        completion(entry)
    }
    
    private func getSampleEntry(in configuration: ConfigurationIntent) -> ScheduleDataEntry {
        let calendar = Calendar.current;
        return ScheduleDataEntry(holidays: nil, schedules: [
            ScheduleWidgetScheduleItem(className: "ASTR 170B2", classColor: [230, 133, 184], startTime: 11 * 3600, endTime: (12 * 3600) + (15 * 60), room: "Chavez 103"),
            ScheduleWidgetScheduleItem(className: "CSC 401A", classColor: [207, 13, 217], startTime: (12 * 3600) + (30 * 60), endTime: (13 * 3600) + (45 * 60), room: "Harvill 302")
        ], errorMessage: nil, date: Date(), dateOfItems: calendar.date(byAdding: .day, value: 2, to: Date()), configuration: configuration);
    }
    
    private func getNextDayWithItems(days: [ScheduleWidgetDayItem], currentDay: ScheduleWidgetDayItem) -> ScheduleWidgetDayItem? {
        // Find the next subsequent day that either has a non-nil non-empty holidays or non-nil non-empty schedules.
        
        // Find the index of the current day in the days array
        guard let currentIndex = days.firstIndex(where: { $0.date == currentDay.date }) else {
            return nil // Return nil if the current day is not found in the array
        }
        
        // Iterate over the subsequent days in the array
        for index in (currentIndex + 1)..<days.count {
            let day = days[index]
            
            // Check if the day has non-nil and non-empty holidays or schedules
            if (day.holidays?.isEmpty == false) || (day.schedules?.isEmpty == false) {
                return day
            }
        }
        
        // Return nil if no subsequent day with items is found
        return nil
    }
    
    public func getTimeline(for configuration: ConfigurationIntent, in context: Context, completion: @escaping (Timeline<ScheduleDataEntry>) -> Void) {
        var entries: [ScheduleDataEntry] = []
        let calendar = Calendar.current
        let today = calendar.startOfDay(for: Date())

        let data = readScheduleData()
        let errorMessage: String?

        if data.days == nil && data.errorMessage == nil {
            errorMessage = "Error loading"
        } else {
            errorMessage = data.errorMessage
        }
        
        if (errorMessage != nil) {
            let entry = ScheduleDataEntry(holidays:nil, schedules:nil, errorMessage: data.errorMessage, date: today, dateOfItems: nil, configuration: configuration);
            completion(Timeline(entries: [entry], policy: .never))
            return
        }
        
        for day in data.days! {
            if (day.holidays != nil && !day.holidays!.isEmpty) {
                // If there's a holiday on this date, we just display the holiday all day
                entries.append(ScheduleDataEntry(holidays: day.holidays, schedules: nil, errorMessage: nil, date: day.date, dateOfItems: nil, configuration: configuration))
            } else if (day.schedules == nil || day.schedules!.isEmpty) {
                // If there aren't any schedules on this day...
                // Get the next day that does have content
                if let nextDayWithContent = getNextDayWithItems(days: data.days!, currentDay: day) {
                    // Show that content
                    entries.append(ScheduleDataEntry(holidays: nextDayWithContent.holidays, schedules: nextDayWithContent.schedules, errorMessage: nil, date: day.date, dateOfItems: nextDayWithContent.date, configuration: configuration))
                } else {
                    // Show that we've reached the end
                    entries.append(ScheduleDataEntry(holidays: nil, schedules: nil, errorMessage: "No more entries. Open the app to refresh.", date: day.date, dateOfItems: nil, configuration: configuration))
                }
            } else {
                // Otherwise, we know there's schedules, iterate over them to switch through the day
                var displayDate = day.date;
                var remainingSchedules = Array(day.schedules!)
                for schedule in day.schedules! {
                    entries.append(ScheduleDataEntry(holidays:nil, schedules:remainingSchedules, errorMessage: nil, date: displayDate, dateOfItems: nil, configuration: configuration))
                    
                    // Remove the first item
                    remainingSchedules.removeFirst()
                    
                    // Set the next display date to whenever the current class ends
                    displayDate = day.date.addingTimeInterval(schedule.endTime)
                }
                
                // And finally, we've exhausted all the schedules on the day, but we need to start displaying the schedule for tomorrow (or the next day)
                if let nextDayWithContent = getNextDayWithItems(days: data.days!, currentDay: day) {
                    entries.append(ScheduleDataEntry(holidays: nextDayWithContent.holidays, schedules: nextDayWithContent.schedules, errorMessage: nil, date: displayDate, dateOfItems: nextDayWithContent.date, configuration: configuration))
                } else {
                    // Show that we've reached the end
                    entries.append(ScheduleDataEntry(holidays: nil, schedules: nil, errorMessage: "No more entries. Open the app to refresh.", date: displayDate, dateOfItems: nil, configuration: configuration))
                }
            }
        }
        
        completion(Timeline(entries: entries, policy: .never));
    }
    
    typealias Entry = ScheduleDataEntry
    typealias Intent = ConfigurationIntent
}

struct ScheduleDataEntry: TimelineEntry {
    public let holidays: [String]?
    public let schedules: [ScheduleWidgetScheduleItem]?
    public let errorMessage: String?
    
    public let date: Date
    public let dateOfItems: Date? // Nil means the items are for the same date as the normal date specified.
    public let configuration: ConfigurationIntent
}

struct PPScheduleWidgetView: View {
    var entry: ScheduleDataEntry
    var body: some View {
        let today = Calendar.current.startOfDay(for: entry.date)
        
        let headerText = entry.errorMessage != nil ? "Schedule" : getRelativeDateString(for: entry.dateOfItems ?? today, today: today)
        
        GeometryReader { geometry in
            VStack(alignment: .leading, spacing: 0) {
                // Header bar
                HStack {
                    if #available(iOSApplicationExtension 15.0, *) {
                        Text(headerText)
                            .padding(.leading)
                            .padding(.vertical, 8)
                            .foregroundStyle(.white)
                            .lineLimit(1)
                    } else {
                        Text(headerText)
                            .padding(.leading)
                            .padding(.vertical, 8)
                            .foregroundColor(.white)
                            .lineLimit(1)
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
                VStack(alignment: .leading, spacing: 8) {
                    if let errorMessage = entry.errorMessage {
                        Text(errorMessage)
                            .padding()
                    } else if let holidays = entry.holidays {
                        ForEach(holidays.indices, id: \.self) { index in
                            Text(holidays[index])
                                .padding()
                        }
                    } else {
                        let schedules = entry.schedules!
                        ForEach(schedules.indices, id: \.self) { index in
                            scheduleItemView(schedule: schedules[index])
                        }
                    }
                }
                .frame(minHeight: 0, maxHeight: /*@START_MENU_TOKEN@*/.infinity/*@END_MENU_TOKEN@*/, alignment: .top)
                .padding(.top, 8)
            }
            .frame(maxHeight: /*@START_MENU_TOKEN@*/.infinity/*@END_MENU_TOKEN@*/, alignment: .top)
        }.background(Color(red: 46/255, green: 54/255, blue: 109/255, opacity: 0.1))
    }
    
    // Schedule item view
    func scheduleItemView(schedule: ScheduleWidgetScheduleItem) -> some View {
        let color = schedule.classColor
        let title = schedule.className
        
        let calendar = Calendar.current
        let currentDate = Date() // Using current date as the reference
        
        let startDate = calendar.date(bySettingHour: Int(schedule.startTime / 3600),
                                      minute: Int((schedule.startTime.truncatingRemainder(dividingBy: 3600)) / 60),
                                       second: 0,
                                       of: currentDate)!

        let endDate = calendar.date(bySettingHour: Int(schedule.endTime / 3600),
                                    minute: Int((schedule.endTime.truncatingRemainder(dividingBy: 3600)) / 60),
                                     second: 0,
                                     of: currentDate)!
        
        let hasRoom = schedule.room != nil && !schedule.room!.isEmpty;
        
        // Format the dates as time strings
        let dateFormatter = DateFormatter()
        
        // Use the user's preferred time format
        dateFormatter.dateStyle = .none
        dateFormatter.timeStyle = .short // This respects the user's locale and settings

        let startTimeString = dateFormatter.string(from: startDate)
        let endTimeString = dateFormatter.string(from: endDate)
        
        let timeString = startTimeString + " - " + endTimeString
        
        return HStack(spacing: 0) {
            Rectangle()
                .fill(Color(red: CGFloat(color[0]) / 255.0, green: CGFloat(color[1]) / 255.0, blue: CGFloat(color[2]) / 255.0))
                .frame(width: 5)
                .cornerRadius(2.0)
            VStack(alignment: .leading) {
                Text(title)
                    .lineLimit(1)
                    .font(.callout)
                
                if hasRoom {
                    Text(schedule.room!)
                        .lineLimit(1)
                        .font(.callout)
                        .foregroundColor(.secondary)
                }
                Text(timeString)
                    .lineLimit(1)
                    .font(.caption)
                    .foregroundColor(.secondary)
            }.padding(.leading, 11).padding(.vertical, 1)
        }.frame(height: hasRoom ? 60 : 44, alignment: .leading)//.widgetURL(urlInApp("item"))
    }
    
    
    func urlInApp(_ name: String) -> URL {
        let activity: NSUserActivity = NSUserActivity.init(activityType: "ViewEventIntent")

        activity.title = "widget_PreviewEvent"
        activity.userInfo = [:]
        activity.becomeCurrent()

        let urlString: String = "widget_PreviewEvent" + name.addingPercentEncoding(withAllowedCharacters: .urlHostAllowed)!
        return URL(string: urlString)!
    }
}

@available(iOSApplicationExtension 15.0, *)
struct ScheduleWidget: Widget {
    private let kind: String = "ScheduleWidget"

    public var body: some WidgetConfiguration {
        IntentConfiguration(kind: kind, intent: ConfigurationIntent.self, provider: ScheduleProvider()) { entry in
            PPScheduleWidgetView(entry: entry)
            }
            .contentMarginsDisabled()
            .configurationDisplayName("Schedule")
            .description("Displays your upcoming classes.")
            .supportedFamilies([.systemSmall, .systemMedium, .systemLarge])
    }
}
