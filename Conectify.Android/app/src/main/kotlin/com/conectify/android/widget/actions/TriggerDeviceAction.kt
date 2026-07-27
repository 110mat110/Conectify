package com.conectify.android.widget.actions

import android.content.Context
import android.util.Log
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import com.conectify.android.widget.SendValueWorker

/**
 * ON / OFF / TRIGGER button. Hands the value straight to [SendValueWorker] and returns — no
 * connectivity check, no state inspection, no waiting for the result.
 */
class TriggerDeviceAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        val deviceId = parameters[DEVICE_ID_KEY] ?: return
        val value = parameters[VALUE_KEY] ?: 1f

        Log.d(TAG, "Button tapped: $deviceId = $value")
        SendValueWorker.enqueue(context, deviceId, value)
    }

    companion object {
        private const val TAG = "TriggerDeviceAction"
        val DEVICE_ID_KEY = ActionParameters.Key<String>("deviceId")
        val VALUE_KEY = ActionParameters.Key<Float>("value")
    }
}
