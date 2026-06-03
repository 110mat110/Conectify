package com.conectify.android.widget

import android.content.Context
import android.util.Log
import androidx.glance.appwidget.GlanceAppWidgetManager
import androidx.glance.appwidget.state.getAppWidgetState
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.work.CoroutineWorker
import androidx.work.ExistingWorkPolicy
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import androidx.work.workDataOf
import com.conectify.android.data.model.ActionDevice
import com.conectify.android.data.model.SensorValue
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.google.gson.Gson
import kotlinx.coroutines.delay

class DelayedRefreshWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        if (!AppPreferences(context).isConfigured()) return Result.failure()

        val deviceId = inputData.getString(KEY_DEVICE_ID)
        val value = inputData.getFloat(KEY_VALUE, Float.NaN)
        val repository = SensorRepository(context)

        // Trigger actuator immediately
        if (deviceId != null && !value.isNaN()) {
            try {
                repository.triggerActuator(deviceId, value)
                Log.d(TAG, "Triggered $deviceId = $value")
            } catch (e: Exception) {
                Log.e(TAG, "Trigger failed: ${e.message}")
                return Result.failure()
            }
        }

        // Refresh current values at 1s, 2s, 5s after trigger
        for ((label, delayMs) in listOf("1s" to 1_000L, "2s" to 1_000L, "5s" to 3_000L)) {
            delay(delayMs)
            try {
                refreshValues(repository)
                Log.d(TAG, "Refreshed at $label")
            } catch (e: Exception) {
                Log.w(TAG, "Refresh at $label failed: ${e.message}")
            }
        }

        return Result.success()
    }

    private suspend fun refreshValues(repository: SensorRepository) {
        val manager = GlanceAppWidgetManager(context)
        val glanceIds = manager.getGlanceIds(ConectifyWidget::class.java)
        if (glanceIds.isEmpty()) return

        val gson = Gson()
        val firstPrefs = getAppWidgetState(context, PreferencesGlanceStateDefinition, glanceIds.first())
        val sensors = gson.fromJson(firstPrefs[WidgetState.SENSORS_JSON_KEY] ?: "[]", Array<SensorValue>::class.java).toList()
        val devices = gson.fromJson(firstPrefs[WidgetState.ACTUATORS_JSON_KEY] ?: "[]", Array<ActionDevice>::class.java).toList()

        val (updatedSensors, updatedDevices) = repository.applyCurrentValues(sensors, devices)

        glanceIds.forEach { glanceId ->
            updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                prefs.toMutablePreferences().apply {
                    this[WidgetState.SENSORS_JSON_KEY] = gson.toJson(updatedSensors)
                    this[WidgetState.ACTUATORS_JSON_KEY] = gson.toJson(updatedDevices)
                    this[WidgetState.LAST_REFRESH_KEY] = System.currentTimeMillis()
                }
            }
            ConectifyWidget().update(context, glanceId)
        }
    }

    companion object {
        private const val TAG = "DelayedRefreshWorker"
        const val KEY_DEVICE_ID = "deviceId"
        const val KEY_VALUE = "value"

        fun enqueue(context: Context, deviceId: String, value: Float) {
            val data = workDataOf(KEY_DEVICE_ID to deviceId, KEY_VALUE to value)
            WorkManager.getInstance(context).enqueueUniqueWork(
                "trigger_$deviceId",
                ExistingWorkPolicy.REPLACE,
                OneTimeWorkRequestBuilder<DelayedRefreshWorker>().setInputData(data).build()
            )
        }
    }
}
