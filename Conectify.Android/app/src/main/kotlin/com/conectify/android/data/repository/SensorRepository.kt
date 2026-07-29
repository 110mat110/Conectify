package com.conectify.android.data.repository

import android.content.Context
import com.conectify.android.data.api.ConectifyApi
import com.conectify.android.data.api.RecordingInterceptor
import com.conectify.android.data.model.ActionDevice
import com.conectify.android.data.model.SensorValue
import com.conectify.android.data.model.SetActuatorValueRequest
import com.conectify.android.data.preferences.AppPreferences
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

class SensorRepository(context: Context) {

    private val baseUrl = AppPreferences(context).serverUrl
    private val userMail = AppPreferences(context).userMail

    private val api: ConectifyApi by lazy {
        val logging = HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BASIC }
        val client = OkHttpClient.Builder()
            .addInterceptor(logging)
            // Innermost, so it sees the request as sent and the outcome as it happens.
            .addInterceptor(RecordingInterceptor())
            // Well under the widget's one-minute refresh cadence, so a hung request fails and
            // shows an error rather than overlapping the next tick.
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(15, TimeUnit.SECONDS)
            .writeTimeout(15, TimeUnit.SECONDS)
            .build()

        Retrofit.Builder()
            .baseUrl("$baseUrl/")
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(ConectifyApi::class.java)
    }

    /** The response is already in the widget's own shape — no mapping, no metadata to interpret. */
    suspend fun getWidgetData(widgetType: String = "large"): Pair<List<SensorValue>, List<ActionDevice>> {
        val response = api.getWidgetData(userMail, widgetType)
        return Pair(response.sensors, response.actuators)
    }

    suspend fun triggerActuator(id: String, value: Float) {
        api.triggerActuator(id, SetActuatorValueRequest(numericValue = value, stringValue = null, unit = ""))
    }
}
