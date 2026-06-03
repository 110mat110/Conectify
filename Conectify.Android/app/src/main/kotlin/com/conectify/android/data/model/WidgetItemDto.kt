package com.conectify.android.data.model

data class WidgetItemDto(
    val type: String,           // "sensor" or "actuator"
    val id: String,
    val name: String,
    val numericValue: Float?,
    val stringValue: String,
    val unit: String,
    val accentColor: String = "#4fc3f7",
    val isActive: Boolean = false,
    val minVal: Float = 0f,
    val maxVal: Float = 1f
) {
    companion object {
        const val TYPE_SENSOR = "sensor"
        const val TYPE_ACTUATOR = "actuator"

        fun fromSensor(s: SensorValue) = WidgetItemDto(
            type = TYPE_SENSOR,
            id = s.id,
            name = s.name,
            numericValue = s.numericValue,
            stringValue = s.stringValue ?: "",
            unit = s.unit,
            accentColor = s.accentColor
        )

        fun fromActuator(a: ActionDevice) = WidgetItemDto(
            type = TYPE_ACTUATOR,
            id = a.id,
            name = a.name,
            numericValue = a.currentValue,
            stringValue = "",
            unit = a.unit,
            isActive = a.isActive,
            minVal = a.minVal,
            maxVal = a.maxVal
        )
    }
}
