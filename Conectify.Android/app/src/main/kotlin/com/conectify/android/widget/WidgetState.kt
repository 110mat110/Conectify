package com.conectify.android.widget

import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import java.io.IOException
import java.net.ConnectException
import java.net.SocketTimeoutException
import java.net.UnknownHostException
import retrofit2.HttpException

object WidgetState {
    // Large widget
    const val TAB_ACTUATORS = "actuators"
    const val TAB_SENSORS = "sensors"

    val ACTIVE_TAB_KEY = stringPreferencesKey("active_tab")
    val LAST_REFRESH_KEY = longPreferencesKey("last_refresh")
    val SENSORS_JSON_KEY = stringPreferencesKey("sensors_json")
    val ACTUATORS_JSON_KEY = stringPreferencesKey("actuators_json")
    val STATUS_KEY = stringPreferencesKey("last_status")

    // Outcome of the last button press. Cleared by the next successful refresh.
    val SEND_STATUS_KEY = stringPreferencesKey("last_send_status")

    // Small carousel widget (Glance — separate DataStore)
    val SMALL_ITEMS_JSON_KEY = stringPreferencesKey("small_items_json")
    val SMALL_INDEX_KEY = intPreferencesKey("small_index")
}

/**
 * The one line of text under the widget. Stored with its kind so the bar can colour it without the
 * composable having to pattern-match on wording.
 */
object StatusLine {

    fun ok(text: String) = "ok|$text"

    fun error(text: String) = "err|$text"

    fun isError(raw: String) = raw.startsWith("err|")

    fun text(raw: String) = raw.substringAfter('|')

    /** Turns an exception into something a person can act on, with no class names on screen. */
    fun describe(e: Exception): String = when (e) {
        is HttpException -> when (e.code()) {
            404 -> "Device not found"
            503 -> "Server can't reach the device"
            else -> "Server error (${e.code()})"
        }
        is UnknownHostException,
        is ConnectException,
        is SocketTimeoutException,
        is IOException -> "No connection to server"
        else -> "Something went wrong"
    }
}
