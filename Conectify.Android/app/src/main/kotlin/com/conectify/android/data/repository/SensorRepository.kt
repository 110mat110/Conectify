package com.conectify.android.data.repository

import android.content.Context
import com.conectify.android.data.api.ConectifyApi
import com.conectify.android.data.api.response.MetadataResponse
import com.conectify.android.data.api.response.WidgetActuatorResponse
import com.conectify.android.data.api.response.WidgetSensorResponse
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
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .build()

        Retrofit.Builder()
            .baseUrl("$baseUrl/")
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(ConectifyApi::class.java)
    }

    suspend fun getWidgetData(widgetType: String = "large"): Pair<List<SensorValue>, List<ActionDevice>> {
        val response = api.getWidgetData(userMail, widgetType)
        return Pair(
            response.sensors.map { it.toSensorValue() },
            response.actuators.mapNotNull { it.toActionDevice() }
        )
    }

    suspend fun applyCurrentValues(
        sensors: List<SensorValue>,
        devices: List<ActionDevice>
    ): Pair<List<SensorValue>, List<ActionDevice>> {
        val values = api.getCurrentValues().associateBy { it.id }

        val updatedSensors = sensors.map { sensor ->
            values[sensor.id]?.let { v ->
                sensor.copy(numericValue = v.numericValue, stringValue = v.stringValue, unit = v.unit, timeCreated = v.timeCreated)
            } ?: sensor
        }

        val updatedDevices = devices.map { device ->
            values[device.id]?.let { v ->
                device.copy(
                    isActive = (v.numericValue ?: 0f) > 0f,
                    currentValue = v.numericValue,
                    unit = v.unit.ifBlank { device.unit }
                )
            } ?: device
        }

        return Pair(updatedSensors, updatedDevices)
    }

    suspend fun triggerActuator(id: String, value: Float) {
        api.triggerActuator(id, SetActuatorValueRequest(numericValue = value, stringValue = null, unit = ""))
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private fun WidgetSensorResponse.toSensorValue() = SensorValue(
        id = id,
        name = name,
        numericValue = currentNumericValue,
        stringValue = currentStringValue,
        unit = unit,
        timeCreated = lastUpdated,
        accentColor = resolveAccentColor(currentNumericValue, metadata)
    )

    // Returns null when no IOType metadata → filtered out (not shown in widget)
    private fun WidgetActuatorResponse.toActionDevice(): ActionDevice? {
        val ioMeta = metadata.firstOrNull { it.name == "IOType" } ?: return null
        val ioType = parseIOType(ioMeta.stringValue) ?: return null
        if (ioType == -1) return null

        return ActionDevice(
            id = id,
            name = name,
            isActive = (currentNumericValue ?: 0f) > 0f,
            currentValue = currentNumericValue,
            unit = unit,
            ioType = ioType,
            minVal = ioMeta.minVal ?: 0f,
            maxVal = ioMeta.maxVal ?: 1f,
            triggerValue = ioMeta.numericValue ?: 1f
        )
    }

    private fun parseIOType(value: String): Int? = when (value) {
        "Linear"    -> 0
        "Binary"    -> 1
        "Trigger"   -> 2
        "Color"     -> 3
        "CCT"       -> 4
        "Undefined" -> -1
        else        -> null
    }

    private fun resolveAccentColor(value: Float?, metadata: List<MetadataResponse>): String {
        if (value == null) return DEFAULT_COLOR
        val threshold = metadata.find { m ->
            m.name == "Threshold" &&
            m.minVal != null && m.maxVal != null &&
            value >= m.minVal && value < m.maxVal &&
            m.stringValue.startsWith("#")
        }
        return threshold?.stringValue ?: DEFAULT_COLOR
    }

    companion object {
        private const val DEFAULT_COLOR = "#4fc3f7"
    }
}
