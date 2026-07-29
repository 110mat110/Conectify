package com.conectify.android.widget

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.util.Log

/**
 * Receives ON / OFF / TRIGGER taps straight from the widget.
 *
 * The buttons deliberately use `actionSendBroadcast` with an explicit intent rather than Glance's
 * `actionRunCallback`, so a tap lands here directly instead of going through Glance's own
 * broadcast receiver, reflective callback instantiation and ActionParameters bundle. Fewer moving
 * parts between the finger and the network call, and the first line of this method is a log.
 */
class WidgetActionReceiver : BroadcastReceiver() {

    override fun onReceive(context: Context, intent: Intent) {
        Log.i(TAG, "onReceive action=${intent.action}")

        if (intent.action != ACTION_TRIGGER) return

        val deviceId = intent.getStringExtra(EXTRA_DEVICE_ID)
        if (deviceId.isNullOrBlank()) {
            Log.w(TAG, "Trigger without a device id — ignoring")
            return
        }
        val value = intent.getFloatExtra(EXTRA_VALUE, 0f)
        val label = intent.getStringExtra(EXTRA_LABEL).orEmpty()

        Log.i(TAG, "Widget button tapped: $label ($deviceId = $value)")
        SendValueWorker.enqueue(context, deviceId, value, label)
    }

    companion object {
        private const val TAG = "WidgetActionReceiver"

        const val ACTION_TRIGGER = "com.conectify.android.widget.action.TRIGGER"
        private const val EXTRA_DEVICE_ID = "deviceId"
        private const val EXTRA_VALUE = "value"
        private const val EXTRA_LABEL = "label"

        /**
         * [label] is what the status line says, e.g. "Kitchen light on". It is passed from the
         * composable, which already knows the device name and which button was drawn, rather than
         * looked up later from the id.
         *
         * Glance stamps a unique data URI (per widget id and per view id) onto this intent, which is
         * what keeps each button's PendingIntent distinct — the extras alone would not, since
         * PendingIntent matching ignores them.
         */
        fun triggerIntent(context: Context, deviceId: String, value: Float, label: String): Intent =
            Intent(context, WidgetActionReceiver::class.java)
                .setAction(ACTION_TRIGGER)
                .putExtra(EXTRA_DEVICE_ID, deviceId)
                .putExtra(EXTRA_VALUE, value)
                .putExtra(EXTRA_LABEL, label)
    }
}
