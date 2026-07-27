package com.conectify.android.widget

import android.content.Context
import android.util.Log
import androidx.work.CoroutineWorker
import androidx.work.ExistingWorkPolicy
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import com.conectify.android.data.preferences.AppPreferences
import kotlinx.coroutines.delay

/**
 * Short follow-up polling after a button press, so the widget shows the new value in a second or
 * two instead of waiting for the next minute tick.
 *
 * Refresh-only. The value itself is sent by [SendValueWorker]; it is deliberately NOT sent here,
 * because a worker that gets stopped and rescheduled re-runs from the top and would resend it.
 * These refreshes are read-only, so re-running them is harmless.
 */
class DelayedRefreshWorker(
    private val context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {

    override suspend fun doWork(): Result {
        if (!AppPreferences(context).isConfigured()) return Result.success()

        for ((label, delayMs) in listOf("1s" to 1_000L, "2s" to 1_000L, "5s" to 3_000L)) {
            delay(delayMs)
            RefreshWorker.refresh(context)
            Log.d(TAG, "Refreshed at $label")
        }

        // The small widget shares the ON/OFF buttons, so pull its values back too.
        SmallRefreshWorker.enqueueNow(context)
        return Result.success()
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
