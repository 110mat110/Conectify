package com.conectify.android.data.model

data class ActionDevice(
    val id: String,
    val name: String,
    val isActive: Boolean = false,
    val currentValue: Float? = null,
    val unit: String = "",
    val ioType: Int = -1,       // -1=Undefined, 0=Linear, 1=Binary, 2=Trigger, 3=Color, 4=CCT
    val minVal: Float = 0f,
    val maxVal: Float = 1f,
    val triggerValue: Float = 1f
)
