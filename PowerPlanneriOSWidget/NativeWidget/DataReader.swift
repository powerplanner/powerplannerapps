//
//  File.swift
//  NativeApp
//
//  Created by Chris Hamons on 7/1/20.
//

import Foundation

struct PrimaryWidgetDataItem: Codable {
    let name: String
    let color: [UInt8]
    let date: Date
}

func readPrimaryData() -> [PrimaryWidgetDataItem] {
    if let url = FileManager.default.containerURL(forSecurityApplicationGroupIdentifier: "group.com.barebonesdev.powerplanner") {
        let path = url.appendingPathComponent("primaryWidget.json")

        if let data = try? Data(contentsOf: path) {
            let decoder = JSONDecoder()
            
            // Configure date decoding if needed
            decoder.dateDecodingStrategy = .iso8601
            
            do {
                let value = try decoder.decode([PrimaryWidgetDataItem].self, from: data)
                return value
            } catch {
                return [PrimaryWidgetDataItem(name: error.localizedDescription, color: [155, 42, 155], date: Date())];
            }
        }
    }

    // Return blank list if none
    return []
}
