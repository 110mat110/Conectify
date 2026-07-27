package com.conectify.android.widget

import android.content.Context
import android.content.Intent
import androidx.glance.appwidget.GlanceAppWidget
import androidx.glance.appwidget.GlanceAppWidgetReceiver

class ConectifyWidgetReceiver : GlanceAppWidgetReceiver() {
    override val glanceAppWidget: GlanceAppWidget = ConectifyWidget()

    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action == WidgetTicker.ACTION_TICK) {
            // Re-arm first, so the loop survives even if the refresh below blows up.
            WidgetTicker.schedule(context)
            RefreshWorker.enqueueNow(context)
            SmallRefreshWorker.enqueueNow(context)
            return
        }
        super.onReceive(context, intent)
    }

    override fun onEnabled(context: Context) {
        super.onEnabled(context)
        WidgetTicker.schedule(context)
    }
}
