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
import retrofit2.HttpException
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

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
 * The outcome is written onto the widget's status line. That is the only feedback the user gets
 * from a home screen, and it is what distinguishes "the tap never fired" from "the tap fired and
 * the request failed".
 */
class SendValueWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        val deviceId = inputData.getString(KEY_DEVICE_ID) ?: return Result.success()
        val value = inputData.getFloat(KEY_VALUE, 0f)
        val shortId = deviceId.take(8)

        if (!AppPreferences(context).isConfigured()) {
            Log.w(TAG, "No server URL configured — dropping $deviceId = $value")
            publish("✗ no server URL")
            return Result.success()
        }

        publish("→ sending $shortId = ${format(value)}")

        try {
            SensorRepository(context).triggerActuator(deviceId, value)
            Log.i(TAG, "Sent $deviceId = $value")
            publish("✓ ${time()} sent $shortId = ${format(value)}")
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            Log.e(TAG, "Send failed for $deviceId = $value", e)
            publish("✗ ${time()} ${describe(e)}")
        }

        // Pull the new value back so the widget shows it without waiting for the next tick.
        DelayedRefreshWorker.enqueue(context)
        return Result.success()
    }

    private suspend fun publish(message: String) {
        val glanceIds = GlanceAppWidgetManager(context).getGlanceIds(ConectifyWidget::class.java)
        glanceIds.forEach { glanceId ->
            updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                prefs.toMutablePreferences().apply { this[WidgetState.SEND_STATUS_KEY] = message }
            }
            ConectifyWidget().update(context, glanceId)
        }
    }

    /**
     * The HTTP code matters here: 404 means the server did not recognise the actuator, 503 means it
     * could not reach the device bus, and anything else means we never got that far.
     */
    private fun describe(e: Exception) = when (e) {
        is HttpException -> "HTTP ${e.code()}"
        else -> e.javaClass.simpleName
    }

    private fun time() = SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date())

    private fun format(value: Float) = if (value % 1f == 0f) value.toInt().toString() else "$value"

    companion object {
        private const val TAG = "SendValueWorker"
        private const val KEY_DEVICE_ID = "deviceId"
        private const val KEY_VALUE = "value"

        /** Not unique work: every tap gets its own job, so rapid taps are all sent. */
        fun enqueue(context: Context, deviceId: String, value: Float) {
            Log.i(TAG, "Enqueueing send $deviceId = $value")
            WorkManager.getInstance(context).enqueue(
                OneTimeWorkRequestBuilder<SendValueWorker>()
                    .setInputData(
                        Data.Builder()
                            .putString(KEY_DEVICE_ID, deviceId)
                            .putFloat(KEY_VALUE, value)
                            .build()
                    )
                    .build()
            )
        }
    }
}
