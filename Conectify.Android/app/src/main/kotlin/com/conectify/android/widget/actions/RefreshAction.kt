package com.conectify.android.widget.actions

import android.content.Context
import androidx.glance.GlanceId
import androidx.glance.action.ActionParameters
import androidx.glance.appwidget.action.ActionCallback
import androidx.glance.appwidget.state.updateAppWidgetState
import androidx.glance.state.PreferencesGlanceStateDefinition
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.conectify.android.widget.ConectifyWidget
import com.conectify.android.widget.WidgetState
import com.google.gson.Gson

class RefreshAction : ActionCallback {

    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        if (!AppPreferences(context).isConfigured()) return

        try {
            val (sensors, devices) = SensorRepository(context).getWidgetData()
            val gson = Gson()

            updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                prefs.toMutablePreferences().apply {
                    this[WidgetState.SENSORS_JSON_KEY] = gson.toJson(sensors)
                    this[WidgetState.ACTUATORS_JSON_KEY] = gson.toJson(devices)
                    this[WidgetState.LAST_REFRESH_KEY] = System.currentTimeMillis()
                }
            }
        } catch (_: Exception) {
            updateAppWidgetState(context, PreferencesGlanceStateDefinition, glanceId) { prefs ->
                prefs.toMutablePreferences().apply {
                    this[WidgetState.LAST_REFRESH_KEY] = System.currentTimeMillis()
                }
            }
        }

        ConectifyWidget().update(context, glanceId)
    }
}
