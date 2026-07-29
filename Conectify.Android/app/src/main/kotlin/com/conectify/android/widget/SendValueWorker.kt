package com.conectify.android.widget

import android.content.Context
import android.util.Log
import androidx.glance.appwidget.GlanceAppWidgetManager
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.work.CoroutineWorker
import androidx.work.Data
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import kotlinx.coroutines.CancellationException

/**
 * Sends one actuator value to Conectify.Services.Android. Fire and forget.
 *
 * Deliberately dumb: no connectivity constraint, one POST attempt, no retry. A tap always produces
 * exactly one send attempt — retrying would risk re-sending a value the user pressed once, and
 * whether it arrives is not the widget's problem.
 *
 * The POST lives in a worker rather than in the receiver because a BroadcastReceiver's `goAsync`
 * window is short and the request can outlive it.
 *
 * The outcome is written onto the widget's status line, which is the only feedback available from
 * a home screen.
 */
class SendValueWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        val deviceId = inputData.getString(KEY_DEVICE_ID) ?: return Result.success()
        val value = inputData.getFloat(KEY_VALUE, 0f)
        val label = inputData.getString(KEY_LABEL).orEmpty().ifBlank { "Command" }

        if (!AppPreferences(context).isConfigured()) {
            Log.w(TAG, "No server URL configured — dropping $deviceId = $value")
            publish(StatusLine.error("Open the app to set the server URL"))
            return Result.success()
        }

        publish(StatusLine.ok("$label…"))

        try {
            SensorRepository(context).triggerActuator(deviceId, value)
            Log.i(TAG, "Sent $deviceId = $value")
            publish(StatusLine.ok(label))
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            Log.e(TAG, "Send failed for $deviceId = $value", e)
            publish(StatusLine.error(StatusLine.describe(e)))
        }

        // Pull the new value back so the widget shows it without waiting for the next tick.
        DelayedRefreshWorker.enqueue(context)
        return Result.success()
    }

    private suspend fun publish(status: String) {
        val glanceIds = GlanceAppWidgetManager(context).getGlanceIds(ConectifyWidget::class.java)
        glanceIds.forEach { glanceId ->
            updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                prefs.toMutablePreferences().apply { this[WidgetState.SEND_STATUS_KEY] = status }
            }
            ConectifyWidget().update(context, glanceId)
        }
    }

    companion object {
        private const val TAG = "SendValueWorker"
        private const val KEY_DEVICE_ID = "deviceId"
        private const val KEY_VALUE = "value"
        private const val KEY_LABEL = "label"

        /** Not unique work: every tap gets its own job, so rapid taps are all sent. */
        fun enqueue(context: Context, deviceId: String, value: Float, label: String) {
            Log.i(TAG, "Enqueueing send $deviceId = $value")
            WorkManager.getInstance(context).enqueue(
                OneTimeWorkRequestBuilder<SendValueWorker>()
                    .setInputData(
                        Data.Builder()
                            .putString(KEY_DEVICE_ID, deviceId)
                            .putFloat(KEY_VALUE, value)
                            .putString(KEY_LABEL, label)
                            .build()
                    )
                    .build()
            )
        }
    }
}
