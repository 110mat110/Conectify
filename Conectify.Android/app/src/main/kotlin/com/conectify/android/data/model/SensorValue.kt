package com.conectify.android.data.model

data class SensorValue(
    val id: String,
    val name: String,
    val numericValue: Float?,
    val stringValue: String?,
    val unit: String,
    val timeCreated: Long,
    val accentColor: String = "#4fc3f7"
)
