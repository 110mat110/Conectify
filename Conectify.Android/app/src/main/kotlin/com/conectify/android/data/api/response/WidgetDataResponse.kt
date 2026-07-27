package com.conectify.android.data.api.response

import com.conectify.android.data.model.ActionDevice
import com.conectify.android.data.model.SensorValue

/**
 * The server sends the widget's models verbatim — names resolved, accent colours picked, IO types
 * parsed, invisible devices filtered out — so there is nothing to map here.
 */
data class WidgetDataResponse(
    val sensors: List<SensorValue>,
    val actuators: List<ActionDevice>
)
