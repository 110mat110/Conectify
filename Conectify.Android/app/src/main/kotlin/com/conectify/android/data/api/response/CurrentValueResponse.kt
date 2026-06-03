package com.conectify.android.data.api.response

data class CurrentValueResponse(
    val id: String,
    val numericValue: Float?,
    val stringValue: String,
    val unit: String,
    val timeCreated: Long
)
