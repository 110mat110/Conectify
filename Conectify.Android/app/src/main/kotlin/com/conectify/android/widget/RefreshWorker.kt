package com.conectify.android.widget

import android.content.Context
import android.util.Log
import androidx.glance.appwidget.GlanceAppWidgetManager
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.google.gson.Gson

class RefreshWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        Log.d(TAG, "doWork started")

        if (!AppPreferences(context).isConfigured()) {
            Log.w(TAG, "Server URL not configured — skipping")
            return Result.success()
        }

        val manager = GlanceAppWidgetManager(context)
        val glanceIds = manager.getGlanceIds(ConectifyWidget::class.java)
        if (glanceIds.isEmpty()) return Result.success()

        return try {
            val (sensors, devices) = SensorRepository(context).getWidgetData()
            Log.d(TAG, "Received ${sensors.size} sensors, ${devices.size} actuators")

            val gson = Gson()
            glanceIds.forEach { glanceId ->
                updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                    prefs.toMutablePreferences().apply {
                        this[WidgetState.SENSORS_JSON_KEY] = gson.toJson(sensors)
                        this[WidgetState.ACTUATORS_JSON_KEY] = gson.toJson(devices)
                        this[WidgetState.LAST_REFRESH_KEY] = System.currentTimeMillis()
                        this[WidgetState.STATUS_KEY] = "OK — ${sensors.size} sensors, ${devices.size} actuators"
                    }
                }
                ConectifyWidget().update(context, glanceId)
            }
            Result.success()
        } catch (e: Exception) {
            val msg = "${e.javaClass.simpleName}: ${e.message}"
            Log.e(TAG, "doWork failed — $msg", e)
            glanceIds.forEach { glanceId ->
                updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                    prefs.toMutablePreferences().apply {
                        this[WidgetState.STATUS_KEY] = "Error: $msg"
                    }
                }
            }
            Result.retry()
        }
    }

    companion object {
        private const val TAG = "ConectifyRefreshWorker"
    }
}
