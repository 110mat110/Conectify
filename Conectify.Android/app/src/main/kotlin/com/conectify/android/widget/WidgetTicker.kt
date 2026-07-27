package com.conectify.android.widget

import android.app.AlarmManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.util.Log

/**
 * Drives the one-minute refresh of both widgets.
 *
 * A repeating [AlarmManager] alarm broadcasts [ACTION_TICK] to [ConectifyWidgetReceiver] every
 * minute; the receiver re-arms the alarm and enqueues the refresh workers. Nothing here looks at
 * connectivity or at the outcome of the previous tick — a failed refresh paints an error on the
 * widget and the next tick, a minute later, simply tries again.
 *
 * [schedule] is called from every place that runs regularly (widget composition, every refresh
 * worker, the tick itself), so a dropped alarm — reboot, force stop, OEM battery manager — is
 * re-armed on the next thing that happens instead of leaving the widget permanently dead.
 */
object WidgetTicker {

    const val ACTION_TICK = "com.conectify.android.widget.action.TICK"

    private const val TAG = "WidgetTicker"
    private const val INTERVAL_MS = 60_000L

    fun schedule(context: Context) {
        val alarmManager = context.getSystemService(Context.ALARM_SERVICE) as? AlarmManager ?: return
        // RTC (not RTC_WAKEUP) on purpose: the widget only matters when the screen is on, so a
        // tick that lands during Doze can wait for the next wake-up.
        alarmManager.setRepeating(
            AlarmManager.RTC,
            System.currentTimeMillis() + INTERVAL_MS,
            INTERVAL_MS,
            pendingIntent(context)
        )
        Log.d(TAG, "Tick alarm armed — every ${INTERVAL_MS / 1000}s")
    }

    private fun pendingIntent(context: Context): PendingIntent =
        PendingIntent.getBroadcast(
            context,
            0,
            // Explicit intent — no manifest intent-filter needed on the receiver.
            Intent(context, ConectifyWidgetReceiver::class.java).setAction(ACTION_TICK),
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )
}