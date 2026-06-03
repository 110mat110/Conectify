package com.conectify.android.widget

import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey

object WidgetState {
    // Large widget (native AppWidgetProvider + DataStore via WidgetPrefs)
    const val TAB_ACTUATORS = "actuators"
    const val TAB_SENSORS = "sensors"

    val ACTIVE_TAB_KEY = stringPreferencesKey("active_tab")
    val LAST_REFRESH_KEY = longPreferencesKey("last_refresh")
    val SENSORS_JSON_KEY = stringPreferencesKey("sensors_json")
    val ACTUATORS_JSON_KEY = stringPreferencesKey("actuators_json")
    val STATUS_KEY = stringPreferencesKey("last_status")

    // Small carousel widget (Glance — separate DataStore)
    val SMALL_ITEMS_JSON_KEY = stringPreferencesKey("small_items_json")
    val SMALL_INDEX_KEY = intPreferencesKey("small_index")
}
