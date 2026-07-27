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
import kotlinx.coroutines.CancellationException

/**
 * Reloads the small carousel widget. Like [RefreshWorker] it never retries — the minute tick is
 * the only retry mechanism.
 */
class SmallRefreshWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        WidgetTicker.schedule(context)

        if (!AppPreferences(context).isConfigured()) return Result.success()

        val glanceIds = GlanceAppWidgetManager(context).getGlanceIds(SmallConectifyWidget::class.java)
        if (glanceIds.isEmpty()) return Result.success()

        try {
            val (sensors, devices) = SensorRepository(context).getWidgetData("small")

            val items = sensors.map { WidgetItemDto.fromSensor(it) } +
                        devices.filter { it.ioType >= 0 }.map { WidgetItemDto.fromActuator(it) }

            val itemsJson = Gson().toJson(items)

            glanceIds.forEach { glanceId ->
                updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                    prefs.toMutablePreferences().apply {
                        this[WidgetState.SMALL_ITEMS_JSON_KEY] = itemsJson
                    }
                }
                SmallConectifyWidget().update(context, glanceId)
            }
            Log.d(TAG, "Loaded ${items.size} items for small widget")
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            Log.e(TAG, "SmallRefreshWorker failed", e)
        }

        return Result.success()
    }

    companion object {
        private const val TAG = "SmallRefreshWorker"
        const val WORK_NAME = "conectify_small_refresh"

        fun enqueueNow(context: Context) {
            WorkManager.getInstance(context).enqueueUniqueWork(
                WORK_NAME,
                ExistingWorkPolicy.REPLACE,
                OneTimeWorkRequestBuilder<SmallRefreshWorker>().build()
            )
        }
    }
}
