package com.conectify.android.data.preferences

import android.content.Context

class AppPreferences(context: Context) {

    private val prefs = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    var serverUrl: String
        get() = prefs.getString(KEY_SERVER_URL, "") ?: ""
        set(value) = prefs.edit().putString(KEY_SERVER_URL, value.trimEnd('/')).apply()

    var userMail: String
        get() = prefs.getString(KEY_USER_MAIL, "") ?: ""
        set(value) = prefs.edit().putString(KEY_USER_MAIL, value.trim()).apply()

    fun isConfigured(): Boolean = serverUrl.isNotBlank()

    companion object {
        private const val PREFS_NAME = "conectify_prefs"
        private const val KEY_SERVER_URL = "server_url"
        private const val KEY_USER_MAIL = "user_mail"
    }
}
