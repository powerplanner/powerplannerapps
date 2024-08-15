//
//  RelativeDateFormatter.swift
//  NativeWidgetExtension
//
//  Created by Andrew Leader on 8/14/24.
//

import Foundation

func getRelativeDateString(for date: Date, today: Date) -> String {
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
