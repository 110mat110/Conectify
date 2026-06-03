package com.conectify.android.widget.actions

import android.content.Context
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.widget.DelayedRefreshWorker

class TriggerDeviceAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        val deviceId = parameters[DEVICE_ID_KEY] ?: return
        val value = parameters[VALUE_KEY] ?: 1f
        if (!AppPreferences(context).isConfigured()) return

        DelayedRefreshWorker.enqueue(context, deviceId, value)
    }

    companion object {
        val DEVICE_ID_KEY = ActionParameters.Key<String>("deviceId")
        val VALUE_KEY = ActionParameters.Key<Float>("value")
    }
}
