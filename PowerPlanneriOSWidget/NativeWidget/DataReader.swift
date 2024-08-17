//
//  File.swift
//  NativeApp
//
//  Created by Chris Hamons on 7/1/20.
//

import Foundation

struct PrimaryWidgetData: Codable {
    let title: String
    let items: [PrimaryWidgetDataItem]?
    let errorMessage: String?
    let dateStrings: DateStrings
    let allDoneString: String
}

struct PrimaryWidgetDataItem: Codable {
    let name: String
    let color: [UInt8]
    let date: Date
}

struct DateStrings: Codable {
    let inThePast: String
    let today: String
    let tomorrow: String
    let inTwoDays: String
    let thisX: String
    let nextX: String

    static let defaultStrings = DateStrings(
        inThePast: "In the past",
        today: "Today",
        tomorrow: "Tomorrow",
        inTwoDays: "In two days",
        thisX: "This {0}",
        nextX: "Next {0}"
    )
}

func readPrimaryData() -> PrimaryWidgetData {
    if let url = FileManager.default.containerURL(forSecurityApplicationGroupIdentifier: "group.com.barebonesdev.powerplanner") {
        let path = url.appendingPathComponent("primaryWidget.json")

        if let data = try? Data(contentsOf: path) {
            let decoder = JSONDecoder()
            
            // Configure date decoding if needed
            decoder.dateDecodingStrategy = .iso8601
            
            do {
                let value = try decoder.decode(PrimaryWidgetData.self, from: data)
                return value
            } catch {
                return PrimaryWidgetData(
                    title: "Agenda",
                    items: nil,
                    errorMessage: "Error loading data",
                    dateStrings: DateStrings.defaultStrings,
                    allDoneString: "All done!"
                )
            }
        }
    }

    // Fallback
    return PrimaryWidgetData(
        title: "Agenda",
        items: nil,
        errorMessage: "Error loading data",
        dateStrings: DateStrings.defaultStrings,
        allDoneString: "All done!"
    )
}
