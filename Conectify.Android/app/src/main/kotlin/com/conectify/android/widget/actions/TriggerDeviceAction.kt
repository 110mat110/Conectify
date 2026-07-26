package com.conectify.android.widget.actions

import android.content.Context
import android.util.Log
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.conectify.android.widget.DelayedRefreshWorker

class TriggerDeviceAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        val deviceId = parameters[DEVICE_ID_KEY] ?: return
        val value = parameters[VALUE_KEY] ?: 1f
        if (!AppPreferences(context).isConfigured()) return

        // Send the actuator command exactly once, here at click time. This runs
        // once per tap and is never retried, so the value is never resent.
        // (Do NOT do this inside a WorkManager worker — a worker that gets
        // stopped/rescheduled by the OS re-runs from the top and resends.)
        try {
            SensorRepository(context).triggerActuator(deviceId, value)
            Log.d(TAG, "Triggered $deviceId = $value")
        } catch (e: Exception) {
            Log.e(TAG, "Trigger failed: ${e.message}")
            return
        }

        // Follow-up only: poll for the updated value so the widget reflects the
        // new state. These are read-only GETs, so it's safe to retry.
        DelayedRefreshWorker.enqueue(context)
    }

    companion object {
        private const val TAG = "TriggerDeviceAction"
        val DEVICE_ID_KEY = ActionParameters.Key<String>("deviceId")
        val VALUE_KEY = ActionParameters.Key<Float>("value")
    }
}
