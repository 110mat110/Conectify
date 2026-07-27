package com.conectify.android.widget.actions

import android.content.Context
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import com.conectify.android.widget.RefreshWorker
import com.conectify.android.widget.WidgetTicker

/**
 * Manual ↺ button. Runs the same worker as the minute tick, so a manual refresh and an automatic
 * one behave identically — including how an error is reported.
 */
class RefreshAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        WidgetTicker.schedule(context)
        RefreshWorker.enqueueNow(context)
    }
}
