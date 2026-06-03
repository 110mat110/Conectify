package com.conectify.android.widget

import android.content.Context
import android.util.Log
import androidx.glance.appwidget.GlanceAppWidgetManager
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.work.CoroutineWorker
import androidx.work.ExistingWorkPolicy
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import com.conectify.android.data.model.WidgetItemDto
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.google.gson.Gson

class SmallRefreshWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        if (!AppPreferences(context).isConfigured()) return Result.success()

        val manager = GlanceAppWidgetManager(context)
        val glanceIds = manager.getGlanceIds(SmallConectifyWidget::class.java)
        if (glanceIds.isEmpty()) return Result.success()

        return try {
            val (sensors, devices) = SensorRepository(context).getWidgetData("small")

            val items = sensors.map { WidgetItemDto.fromSensor(it) } +
                        devices.filter { it.ioType >= 0 }.map { WidgetItemDto.fromActuator(it) }

            val gson = Gson()
            val itemsJson = gson.toJson(items)

            glanceIds.forEach { glanceId ->
                updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                    prefs.toMutablePreferences().apply {
                        this[WidgetState.SMALL_ITEMS_JSON_KEY] = itemsJson
                    }
                }
                SmallConectifyWidget().update(context, glanceId)
            }
            Log.d(TAG, "Loaded ${items.size} items for small widget")
            Result.success()
        } catch (e: Exception) {
            Log.e(TAG, "SmallRefreshWorker failed: ${e.message}")
            Result.retry()
        }
    }

    companion object {
        private const val TAG = "SmallRefreshWorker"
        const val WORK_NAME = "conectify_small_initial_load"

        fun enqueue(context: Context) {
            WorkManager.getInstance(context).enqueueUniqueWork(
                WORK_NAME,
                ExistingWorkPolicy.KEEP,
                OneTimeWorkRequestBuilder<SmallRefreshWorker>().build()
            )
        }
    }
}
