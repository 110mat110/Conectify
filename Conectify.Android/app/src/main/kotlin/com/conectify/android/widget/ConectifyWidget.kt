package com.conectify.android.widget

import android.content.Context
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.DpSize
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.datastore.preferences.core.Preferences
import androidx.glance.GlanceId
import androidx.glance.GlanceModifier
import androidx.glance.Image
import androidx.glance.ImageProvider
import androidx.glance.LocalSize
import androidx.glance.action.actionParametersOf
import androidx.glance.action.clickable
import androidx.glance.appwidget.GlanceAppWidget
import androidx.glance.appwidget.SizeMode
import androidx.glance.appwidget.action.actionRunCallback
import androidx.glance.appwidget.cornerRadius
import androidx.glance.appwidget.lazy.LazyColumn
import androidx.glance.appwidget.lazy.items
import androidx.glance.appwidget.provideContent
import androidx.glance.appwidget.state.getAppWidgetState
import androidx.glance.background
import androidx.glance.currentState
import androidx.glance.layout.Alignment
import androidx.glance.layout.Box
import androidx.glance.layout.Column
import androidx.glance.layout.Row
import androidx.glance.layout.Spacer
import androidx.glance.layout.fillMaxHeight
import androidx.glance.layout.fillMaxSize
import androidx.glance.layout.fillMaxWidth
import androidx.glance.layout.height
import androidx.glance.layout.padding
import androidx.glance.layout.size
import androidx.glance.layout.width
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.glance.text.FontWeight
import androidx.glance.text.Text
import androidx.glance.text.TextStyle
import androidx.glance.unit.ColorProvider
import androidx.work.ExistingPeriodicWorkPolicy
import androidx.work.ExistingWorkPolicy
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.PeriodicWorkRequestBuilder
import androidx.work.WorkManager
import com.conectify.android.R
import com.conectify.android.data.model.ActionDevice
import com.conectify.android.data.model.SensorValue
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.widget.WidgetState.TAB_ACTUATORS
import com.conectify.android.widget.WidgetState.TAB_SENSORS
import com.conectify.android.widget.actions.RefreshAction
import com.conectify.android.widget.actions.SwitchTabAction
import com.conectify.android.widget.actions.TriggerDeviceAction
import com.google.gson.Gson
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.concurrent.TimeUnit

class ConectifyWidget : GlanceAppWidget() {

    override val stateDefinition = PreferencesGlanceStateDefinition

    override val sizeMode = SizeMode.Responsive(
        setOf(
            DpSize(180.dp, 100.dp),
            DpSize(300.dp, 100.dp),
        )
    )

    override suspend fun provideGlance(context: Context, id: GlanceId) {
        schedulePeriodicRefresh(context)

        if (AppPreferences(context).isConfigured()) {
            val state = getAppWidgetState(context, PreferencesGlanceStateDefinition, id)
            if (state[WidgetState.SENSORS_JSON_KEY] == null) {
                WorkManager.getInstance(context).enqueueUniqueWork(
                    INITIAL_LOAD_WORK,
                    ExistingWorkPolicy.KEEP,
                    OneTimeWorkRequestBuilder<RefreshWorker>().build()
                )
            }
        }

        provideContent {
            val state = currentState<Preferences>()
            val activeTab = state[WidgetState.ACTIVE_TAB_KEY] ?: TAB_ACTUATORS
            val lastRefresh = state[WidgetState.LAST_REFRESH_KEY] ?: 0L
            val configured = AppPreferences(context).isConfigured()

            val gson = Gson()
            val sensors = gson.fromJson(
                state[WidgetState.SENSORS_JSON_KEY] ?: "[]", Array<SensorValue>::class.java
            ).toList()
            val devices = gson.fromJson(
                state[WidgetState.ACTUATORS_JSON_KEY] ?: "[]", Array<ActionDevice>::class.java
            ).toList()

            ConectifyWidgetContent(
                sensors = sensors,
                devices = devices,
                activeTab = activeTab,
                lastRefreshTime = lastRefresh,
                isConfigured = configured,
                statusMessage = state[WidgetState.STATUS_KEY]
            )
        }
    }

    private fun schedulePeriodicRefresh(context: Context) {
        WorkManager.getInstance(context).enqueueUniquePeriodicWork(
            WORK_NAME,
            ExistingPeriodicWorkPolicy.KEEP,
            PeriodicWorkRequestBuilder<RefreshWorker>(10, TimeUnit.MINUTES).build()
        )
    }

    companion object {
        const val WORK_NAME = "conectify_periodic_refresh"
        const val INITIAL_LOAD_WORK = "conectify_initial_load"
    }
}

// ── Root ──────────────────────────────────────────────────────────────────────

@Composable
fun ConectifyWidgetContent(
    sensors: List<SensorValue>,
    devices: List<ActionDevice>,
    activeTab: String,
    lastRefreshTime: Long,
    isConfigured: Boolean = true,
    statusMessage: String? = null
) {
    Box(
        modifier = GlanceModifier
            .fillMaxSize()
            .background(ColorProvider(Color(0xFF0E0E0E)))
            .padding(10.dp)
    ) {
        Column(modifier = GlanceModifier.fillMaxSize()) {
            TabBar(activeTab = activeTab)
            Spacer(modifier = GlanceModifier.height(8.dp))

            when {
                !isConfigured -> EmptyState(
                    message = "Open app to set server URL",
                    modifier = GlanceModifier.fillMaxWidth().defaultWeight()
                )
                activeTab == TAB_ACTUATORS && devices.isEmpty() -> EmptyState(
                    message = "Loading…",
                    modifier = GlanceModifier.fillMaxWidth().defaultWeight()
                )
                activeTab == TAB_SENSORS && sensors.isEmpty() -> EmptyState(
                    message = "Loading…",
                    modifier = GlanceModifier.fillMaxWidth().defaultWeight()
                )
                activeTab == TAB_ACTUATORS -> ActuatorsCarousel(
                    devices = devices,
                    modifier = GlanceModifier.fillMaxWidth().defaultWeight()
                )
                else -> SensorsCarousel(
                    sensors = sensors,
                    modifier = GlanceModifier.fillMaxWidth().defaultWeight()
                )
            }

            Spacer(modifier = GlanceModifier.height(4.dp))
            RefreshBar(lastRefreshTime = lastRefreshTime, statusMessage = statusMessage)
        }
    }
}

// ── Tab bar ───────────────────────────────────────────────────────────────────

@Composable
private fun TabBar(activeTab: String) {
    Row(
        modifier = GlanceModifier
            .fillMaxWidth()
            .background(ColorProvider(Color(0xFF1A1A1A)))
            .cornerRadius(10.dp)
            .padding(3.dp)
    ) {
        TabButton(
            label = "Actuators",
            isActive = activeTab == TAB_ACTUATORS,
            modifier = GlanceModifier.defaultWeight().clickable(
                actionRunCallback<SwitchTabAction>(
                    actionParametersOf(SwitchTabAction.TAB_KEY to TAB_ACTUATORS)
                )
            )
        )
        Spacer(modifier = GlanceModifier.width(3.dp))
        TabButton(
            label = "Sensors",
            isActive = activeTab == TAB_SENSORS,
            modifier = GlanceModifier.defaultWeight().clickable(
                actionRunCallback<SwitchTabAction>(
                    actionParametersOf(SwitchTabAction.TAB_KEY to TAB_SENSORS)
                )
            )
        )
    }
}

@Composable
private fun TabButton(label: String, isActive: Boolean, modifier: GlanceModifier) {
    val bg = if (isActive) Color(0xFF4fc3f7).copy(alpha = 0.2f) else Color.Transparent
    val textColor = if (isActive) Color(0xFF4fc3f7) else Color(0xFF666666)
    Box(
        modifier = modifier
            .background(ColorProvider(bg))
            .cornerRadius(8.dp)
            .padding(vertical = 6.dp, horizontal = 4.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = label,
            style = TextStyle(
                color = ColorProvider(textColor),
                fontSize = 12.sp,
                fontWeight = if (isActive) FontWeight.Bold else FontWeight.Normal
            )
        )
    }
}

// ── Empty state ───────────────────────────────────────────────────────────────

@Composable
private fun EmptyState(message: String, modifier: GlanceModifier) {
    Box(modifier = modifier, contentAlignment = Alignment.Center) {
        Text(
            text = message,
            style = TextStyle(color = ColorProvider(Color(0xFF555555)), fontSize = 12.sp)
        )
    }
}

// ── Carousels ─────────────────────────────────────────────────────────────────

@Composable
private fun SensorsCarousel(sensors: List<SensorValue>, modifier: GlanceModifier) {
    val twoColumns = LocalSize.current.width >= 300.dp
    val chunks = if (twoColumns) sensors.chunked(2) else sensors.map { listOf(it) }
    LazyColumn(modifier = modifier) {
        items(chunks) { row ->
            Row(modifier = GlanceModifier.fillMaxWidth().padding(bottom = 6.dp)) {
                row.forEachIndexed { index, sensor ->
                    SensorCube(
                        sensor = sensor,
                        modifier = GlanceModifier.defaultWeight()
                            .padding(start = if (index > 0) 4.dp else 0.dp)
                    )
                }
                if (row.size == 1 && twoColumns) Spacer(modifier = GlanceModifier.defaultWeight())
            }
        }
    }
}

@Composable
private fun ActuatorsCarousel(devices: List<ActionDevice>, modifier: GlanceModifier) {
    val twoColumns = LocalSize.current.width >= 300.dp
    if (twoColumns) {
        val pairs = devices.chunked(2)
        LazyColumn(modifier = modifier) {
            items(pairs) { pair ->
                Row(modifier = GlanceModifier.fillMaxWidth().padding(bottom = 5.dp)) {
                    pair.forEachIndexed { index, device ->
                        ActuatorRow(
                            device = device,
                            modifier = GlanceModifier.defaultWeight()
                                .padding(start = if (index > 0) 4.dp else 0.dp)
                        )
                    }
                    if (pair.size == 1) Spacer(modifier = GlanceModifier.defaultWeight())
                }
            }
        }
    } else {
        LazyColumn(modifier = modifier) {
            items(devices) { device ->
                ActuatorRow(
                    device = device,
                    modifier = GlanceModifier.fillMaxWidth().padding(bottom = 5.dp)
                )
            }
        }
    }
}

// ── Sensor cube ───────────────────────────────────────────────────────────────

@Composable
private fun SensorCube(sensor: SensorValue, modifier: GlanceModifier = GlanceModifier) {
    val accent = parseHexColor(sensor.accentColor)
    val valueText = sensor.numericValue
        ?.let { "%.1f %s".format(it, sensor.unit) }
        ?: "${sensor.stringValue} ${sensor.unit}"

    Box(
        modifier = modifier
            .background(ColorProvider(accent.copy(alpha = 0.25f)))
            .cornerRadius(12.dp)
            .padding(1.dp)
    ) {
        Column(
            modifier = GlanceModifier
                .fillMaxWidth()
                .background(ColorProvider(Color(0xFF1A1A1A)))
                .cornerRadius(11.dp)
                .padding(10.dp)
        ) {
            Text(
                text = sensor.name,
                style = TextStyle(color = ColorProvider(Color(0xFF9E9E9E)), fontSize = 11.sp),
                maxLines = 1
            )
            Spacer(modifier = GlanceModifier.height(5.dp))
            Box(
                modifier = GlanceModifier
                    .background(ColorProvider(accent.copy(alpha = 0.15f)))
                    .cornerRadius(6.dp)
                    .padding(horizontal = 6.dp, vertical = 3.dp)
            ) {
                Text(
                    text = valueText,
                    style = TextStyle(
                        color = ColorProvider(accent),
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Medium
                    )
                )
            }
        }
    }
}

// ── Actuator row ──────────────────────────────────────────────────────────────

@Composable
private fun ActuatorRow(device: ActionDevice, modifier: GlanceModifier = GlanceModifier) {
    val accent = Color(0xFF4fc3f7)
    val isOn = device.isActive
    val borderAlpha = if (isOn) 0.45f else 0.1f

    Box(
        modifier = modifier
            .background(ColorProvider(accent.copy(alpha = borderAlpha)))
            .cornerRadius(12.dp)
            .padding(1.dp)
    ) {
        Column(
            modifier = GlanceModifier
                .fillMaxWidth()
                .background(ColorProvider(Color(0xFF1A1A1A)))
                .cornerRadius(11.dp)
                .padding(horizontal = 12.dp, vertical = 10.dp)
        ) {
            Row(
                modifier = GlanceModifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Image(
                    provider = ImageProvider(R.drawable.ic_power),
                    contentDescription = null,
                    modifier = GlanceModifier.size(13.dp)
                )
                Spacer(modifier = GlanceModifier.width(6.dp))
                Text(
                    text = device.name,
                    style = TextStyle(
                        color = ColorProvider(Color(0xFFCCCCCC)),
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Medium
                    ),
                    modifier = GlanceModifier.defaultWeight(),
                    maxLines = 1
                )
                device.currentValue?.let { v ->
                    val label = if (v % 1f == 0f) v.toInt().toString() else "%.1f".format(v)
                    val display = if (device.unit.isNotBlank()) "$label ${device.unit}" else label
                    Spacer(modifier = GlanceModifier.width(6.dp))
                    Box(
                        modifier = GlanceModifier
                            .background(ColorProvider(if (isOn) accent.copy(alpha = 0.15f) else Color(0xFF242424)))
                            .cornerRadius(5.dp)
                            .padding(horizontal = 6.dp, vertical = 2.dp)
                    ) {
                        Text(
                            text = display,
                            style = TextStyle(
                                color = ColorProvider(if (isOn) accent else Color(0xFF666666)),
                                fontSize = 11.sp
                            )
                        )
                    }
                }
            }

            Spacer(modifier = GlanceModifier.height(8.dp))

            Row(modifier = GlanceModifier.fillMaxWidth()) {
                when (device.ioType) {
                    IO_BINARY, IO_LINEAR, IO_COLOR, IO_CCT -> {
                        ActuatorButton(
                            label = "ON",
                            color = BTN_GREEN,
                            modifier = GlanceModifier.defaultWeight(),
                            onClick = actionRunCallback<TriggerDeviceAction>(
                                actionParametersOf(
                                    TriggerDeviceAction.DEVICE_ID_KEY to device.id,
                                    TriggerDeviceAction.VALUE_KEY to device.maxVal
                                )
                            )
                        )
                        Spacer(modifier = GlanceModifier.width(6.dp))
                        ActuatorButton(
                            label = "OFF",
                            color = BTN_RED,
                            modifier = GlanceModifier.defaultWeight(),
                            onClick = actionRunCallback<TriggerDeviceAction>(
                                actionParametersOf(
                                    TriggerDeviceAction.DEVICE_ID_KEY to device.id,
                                    TriggerDeviceAction.VALUE_KEY to device.minVal
                                )
                            )
                        )
                    }
                    IO_TRIGGER -> {
                        ActuatorButton(
                            label = "TRIGGER",
                            color = BTN_BLUE,
                            modifier = GlanceModifier.fillMaxWidth(),
                            onClick = actionRunCallback<TriggerDeviceAction>(
                                actionParametersOf(
                                    TriggerDeviceAction.DEVICE_ID_KEY to device.id,
                                    TriggerDeviceAction.VALUE_KEY to device.triggerValue
                                )
                            )
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun ActuatorButton(
    label: String,
    color: Color,
    modifier: GlanceModifier,
    onClick: androidx.glance.action.Action
) {
    Box(
        modifier = modifier
            .background(ColorProvider(color.copy(alpha = 0.15f)))
            .cornerRadius(7.dp)
            .padding(vertical = 6.dp)
            .clickable(onClick),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = label,
            style = TextStyle(
                color = ColorProvider(color),
                fontSize = 12.sp,
                fontWeight = FontWeight.Bold
            )
        )
    }
}

// ── Refresh bar ───────────────────────────────────────────────────────────────

@Composable
private fun RefreshBar(lastRefreshTime: Long, statusMessage: String? = null) {
    val isError = statusMessage?.startsWith("Error") == true
    val displayText = when {
        isError -> statusMessage!!
        statusMessage != null && lastRefreshTime > 0 ->
            "${SimpleDateFormat("HH:mm", Locale.getDefault()).format(Date(lastRefreshTime))} — $statusMessage"
        lastRefreshTime > 0 ->
            "Refreshed ${SimpleDateFormat("HH:mm", Locale.getDefault()).format(Date(lastRefreshTime))}"
        else -> "Tap ↺ to load"
    }
    val textColor = if (isError) Color(0xFFef5350) else Color(0xFF555555)

    Row(
        modifier = GlanceModifier
            .fillMaxWidth()
            .background(ColorProvider(Color(0xFF1A1A1A)))
            .cornerRadius(8.dp)
            .padding(horizontal = 10.dp, vertical = 6.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Image(
            provider = ImageProvider(R.drawable.ic_refresh),
            contentDescription = "Refresh",
            modifier = GlanceModifier.size(16.dp).clickable(actionRunCallback<RefreshAction>())
        )
        Spacer(modifier = GlanceModifier.width(6.dp))
        Text(
            text = displayText,
            style = TextStyle(color = ColorProvider(textColor), fontSize = 11.sp),
            maxLines = 1
        )
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

private fun parseHexColor(hex: String): Color =
    try { Color(android.graphics.Color.parseColor(hex)) }
    catch (_: IllegalArgumentException) { Color(0xFF4fc3f7) }

private const val IO_LINEAR = 0
private const val IO_BINARY = 1
private const val IO_TRIGGER = 2
private const val IO_COLOR = 3
private const val IO_CCT = 4

private val BTN_GREEN = Color(0xFF66bb6a)
private val BTN_RED   = Color(0xFFef5350)
private val BTN_BLUE  = Color(0xFF4fc3f7)
