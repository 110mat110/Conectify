package com.conectify.android.widget.actions

import android.content.Context
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import com.conectify.android.widget.ConectifyWidget
import com.conectify.android.widget.WidgetState

class SwitchTabAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        val tab = parameters[TAB_KEY] ?: WidgetState.TAB_ACTUATORS

        updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
            prefs.toMutablePreferences().apply { this[WidgetState.ACTIVE_TAB_KEY] = tab }
        }
        ConectifyWidget().update(context, glanceId)
    }

    companion object {
        val TAB_KEY = ActionParameters.Key<String>("tab")
    }
}
