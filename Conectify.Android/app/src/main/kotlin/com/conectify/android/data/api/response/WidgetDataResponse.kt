package com.conectify.android.data.api.response

data class WidgetDataResponse(
    val sensors: List<WidgetSensorResponse>,
    val actuators: List<WidgetActuatorResponse>
)
