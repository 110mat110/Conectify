package com.conectify.android.data.api.response

data class MetadataResponse(
    val name: String,
    val numericValue: Float?,
    val stringValue: String,
    val unit: String,
    val minVal: Float?,
    val maxVal: Float?
)
