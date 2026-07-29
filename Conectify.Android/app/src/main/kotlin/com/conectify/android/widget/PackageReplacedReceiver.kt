package com.conectify.android.widget

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.util.Log
import androidx.glance.appwidget.updateAll
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

/**
 * Re-renders the placed widgets after the app is reinstalled or updated.
 *
 * This is not redundant with Glance's own MyPackageReplacedReceiver, which only calls
 * `cleanReceivers()` and leaves the RemoteViews alone. A widget's click targets are PendingIntents
 * baked in when its composition was last rendered, and `updatePeriodMillis` is 0, so nothing else
 * forces a re-render: a widget placed before an update keeps invoking the *previous* build's code
 * until something calls update(). When that code has been deleted, the tap resolves to nothing and
 * fails silently — no request, no log, no error.
 */
class PackageReplacedReceiver : BroadcastReceiver() {

    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action != Intent.ACTION_MY_PACKAGE_REPLACED) return

        val pendingResult = goAsync()
        CoroutineScope(Dispatchers.Default).launch {
            try {
                refreshWidgetViews(context)
                Log.i(TAG, "Widgets re-rendered after package replace")
            } catch (e: Exception) {
                Log.e(TAG, "Re-render after package replace failed", e)
            } finally {
                pendingResult.finish()
            }
        }
    }

    companion object {
        private const val TAG = "PackageReplaced"

        /** Rebuilds both widgets' RemoteViews, which re-points every click at the current build. */
        suspend fun refreshWidgetViews(context: Context) {
            ConectifyWidget().updateAll(context)
            SmallConectifyWidget().updateAll(context)
            WidgetTicker.schedule(context)
        }
    }
}
