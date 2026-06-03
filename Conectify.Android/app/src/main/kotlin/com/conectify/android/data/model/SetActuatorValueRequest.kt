package com.conectify.android.data.model

data class SetActuatorValueRequest(
    val numericValue: Float?,
    val stringValue: String?,
    val unit: String
)
