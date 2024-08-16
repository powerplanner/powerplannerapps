//
//  ScheduleDataReader.swift
//  NativeWidgetExtension
//
//  Created by Andrew Leader on 8/14/24.
//

import Foundation


// Represents the main data structure for the schedule widget.
struct ScheduleWidgetData: Codable {
    let title: String
    let errorMessage: String?
    let days: [ScheduleWidgetDayItem]?
    let dateStrings: DateStrings
}

// Represents a day with its holidays and schedules.
struct ScheduleWidgetDayItem: Codable {
    let date: Date
    let holidays: [String]?
    let schedules: [ScheduleWidgetScheduleItem]?
}

// Represents a schedule item for a class.
struct ScheduleWidgetScheduleItem: Codable {
    let className: String
    let classColor: [UInt8]
    let startTime: TimeInterval
    let endTime: TimeInterval
    let room: String?
}

func readScheduleData() -> ScheduleWidgetData {
    if let url = FileManager.default.containerURL(forSecurityApplicationGroupIdentifier: "group.com.barebonesdev.powerplanner") {
        let path = url.appendingPathComponent("scheduleWidget.json")

        if let data = try? Data(contentsOf: path) {
            let decoder = JSONDecoder()
            
            // Configure date decoding if needed
            decoder.dateDecodingStrategy = .iso8601
            
            do {
                let value = try decoder.decode(ScheduleWidgetData.self, from: data)
                return value
            } catch {
                return ScheduleWidgetData(title: "Schedule", errorMessage: "Error loading data", days: nil, dateStrings: DateStrings.defaultStrings)
            }
        }
    }

    // Return blank list if none
    return ScheduleWidgetData(title: "Schedule", errorMessage: "Error loading data", days: nil, dateStrings: DateStrings.defaultStrings)
}
