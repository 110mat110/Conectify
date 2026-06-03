package com.conectify.android.widget.actions

import android.content.Context
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import androidx.glance.appwidget.state.getAppWidgetState
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import com.conectify.android.data.model.WidgetItemDto
import com.conectify.android.widget.SmallConectifyWidget
import com.conectify.android.widget.WidgetState
import com.google.gson.Gson

class NavigateAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        val direction = parameters[DIRECTION_KEY] ?: return

        val prefs = getAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId)
        val currentIndex = prefs[WidgetState.SMALL_INDEX_KEY] ?: 0
        val itemsJson = prefs[WidgetState.SMALL_ITEMS_JSON_KEY] ?: return
        val total = Gson().fromJson(itemsJson, Array<WidgetItemDto>::class.java).size

        if (total == 0) return

        val newIndex = ((currentIndex + direction) + total) % total

        updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { p ->
            p.toMutablePreferences().apply {
                this[WidgetState.SMALL_INDEX_KEY] = newIndex
            }
        }
        SmallConectifyWidget().update(context, glanceId)
    }

    companion object {
        val DIRECTION_KEY = ActionParameters.Key<Int>("direction")
        const val NEXT = 1
        const val PREV = -1
    }
}
