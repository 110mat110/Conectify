package com.conectify.android.widget

import android.content.Context
import android.util.Log
import androidx.datastore.preferences.core.MutablePreferences
import androidx.glance.GlanceId
import androidx.glance.appwidget.GlanceAppWidgetManager
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.work.CoroutineWorker
import androidx.work.ExistingWorkPolicy
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.google.gson.Gson
import kotlinx.coroutines.CancellationException

/**
 * Reloads the large widget's data. Driven once a minute by [WidgetTicker], plus a WorkManager
 * periodic backstop (see [ConectifyWidget]) in case the alarm is ever lost.
 *
 * This worker never fails and never asks for a retry: a failed refresh writes the error onto the
 * widget and returns success. Retrying is the tick's job, and it is always exactly a minute away.
 */
class RefreshWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        // Every run re-arms the tick, so the minute loop heals itself after a reboot or force stop.
        WidgetTicker.schedule(context)
        refresh(context)

        // Deliberately not Result.retry(): WorkManager's exponential backoff is what used to
        // leave the widget dead for hours after a single failure.
        return Result.success()
    }

    companion object {
        private const val TAG = "ConectifyRefreshWorker"
        const val WORK_NAME = "conectify_refresh"

        fun enqueueNow(context: Context) {
            WorkManager.getInstance(context).enqueueUniqueWork(
                WORK_NAME,
                ExistingWorkPolicy.REPLACE,
                OneTimeWorkRequestBuilder<RefreshWorker>().build()
            )
        }

        /**
         * Fetches the widget payload and paints it. One server call: the response already carries
         * the current values, so there is nothing to merge afterwards.
         */
        internal suspend fun refresh(context: Context) {
            val glanceIds = GlanceAppWidgetManager(context).getGlanceIds(ConectifyWidget::class.java)
            if (glanceIds.isEmpty()) return

            if (!AppPreferences(context).isConfigured()) {
                publish(context, glanceIds) { it[WidgetState.STATUS_KEY] = "Open app to set server URL" }
                return
            }

            try {
                val (sensors, devices) = SensorRepository(context).getWidgetData()
                Log.d(TAG, "Received ${sensors.size} sensors, ${devices.size} actuators")

                val gson = Gson()
                publish(context, glanceIds) { prefs ->
                    prefs[WidgetState.SENSORS_JSON_KEY] = gson.toJson(sensors)
                    prefs[WidgetState.ACTUATORS_JSON_KEY] = gson.toJson(devices)
                    prefs[WidgetState.LAST_REFRESH_KEY] = System.currentTimeMillis()
                    prefs.remove(WidgetState.STATUS_KEY)
                }
            } catch (e: CancellationException) {
                throw e
            } catch (e: Exception) {
                Log.e(TAG, "Refresh failed", e)
                publish(context, glanceIds) { it[WidgetState.STATUS_KEY] = "Error: ${e.javaClass.simpleName}" }
            }
        }

        private suspend fun publish(
            context: Context,
            glanceIds: List<GlanceId>,
            edit: (MutablePreferences) -> Unit
        ) {
            glanceIds.forEach { glanceId ->
                updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                    prefs.toMutablePreferences().apply(edit)
                }
                ConectifyWidget().update(context, glanceId)
            }
        }
    }
}
