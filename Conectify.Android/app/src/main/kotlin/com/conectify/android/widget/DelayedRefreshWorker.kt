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

        val repository = SensorRepository(context)

        // Refresh-only worker. The actuator trigger is sent once at click time
        // (see TriggerDeviceAction); it is deliberately NOT done here, because a
        // worker that gets stopped/rescheduled re-runs from the top and would
        // resend the value. These refreshes are read-only GETs — safe to retry.
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
        private const val WORK_NAME = "conectify_refresh_after_trigger"

        fun enqueue(context: Context) {
            WorkManager.getInstance(context).enqueueUniqueWork(
                WORK_NAME,
                ExistingWorkPolicy.REPLACE,
                OneTimeWorkRequestBuilder<DelayedRefreshWorker>().build()
            )
        }
    }
}
