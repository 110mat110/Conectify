package com.conectify.android.widget

import android.content.Context
import android.util.Log
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
 * Deliberately dumb: no connectivity constraint, one POST attempt, no retry, failure is only
 * logged. A tap always produces exactly one send attempt — retrying would risk re-sending a value
 * the user pressed once, and whether it arrives is not the widget's problem.
 *
 * The POST lives in a worker rather than in the click callback because the callback runs inside a
 * BroadcastReceiver's short `goAsync` window, which the request can outlive.
 */
class SendValueWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        val deviceId = inputData.getString(KEY_DEVICE_ID) ?: return Result.success()
        val value = inputData.getFloat(KEY_VALUE, 0f)

        if (!AppPreferences(context).isConfigured()) {
            Log.w(TAG, "No server URL configured — dropping $deviceId = $value")
            return Result.success()
        }

        try {
            SensorRepository(context).triggerActuator(deviceId, value)
            Log.d(TAG, "Sent $deviceId = $value")
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            Log.e(TAG, "Send failed for $deviceId = $value", e)
        }

        // Pull the new value back so the widget shows it without waiting for the next tick.
        DelayedRefreshWorker.enqueue(context)
        return Result.success()
    }

    companion object {
        private const val TAG = "SendValueWorker"
        private const val KEY_DEVICE_ID = "deviceId"
        private const val KEY_VALUE = "value"

        /** Not unique work: every tap gets its own job, so rapid taps are all sent. */
        fun enqueue(context: Context, deviceId: String, value: Float) {
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
