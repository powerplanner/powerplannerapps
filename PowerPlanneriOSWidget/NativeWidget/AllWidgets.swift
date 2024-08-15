//
//  AllWidgets.swift
//  NativeWidgetExtension
//
//  Created by Andrew Leader on 8/14/24.
//

import Foundation
import SwiftUI

@available(iOSApplicationExtension 15.0, *)
@main
struct AllWidgets: WidgetBundle {
    var body: some Widget {
        AgendaWidget()
        ScheduleWidget()
    }
}
