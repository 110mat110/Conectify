package com.conectify.android.data.api.response

data class WidgetSensorResponse(
    val id: String,
    val name: String,
    val sourceDeviceName: String,
    val currentNumericValue: Float?,
    val currentStringValue: String,
    val unit: String,
    val lastUpdated: Long,
    val metadata: List<MetadataResponse>
)
