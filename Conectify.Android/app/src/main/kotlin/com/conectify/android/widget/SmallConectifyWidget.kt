package com.conectify.android.widget

import android.content.Context
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.datastore.preferences.core.Preferences
import androidx.glance.GlanceId
import androidx.glance.GlanceModifier
import androidx.glance.Image
import androidx.glance.ImageProvider
import androidx.glance.action.actionParametersOf
import androidx.glance.action.clickable
import androidx.glance.appwidget.GlanceAppWidget
import androidx.glance.appwidget.action.actionRunCallback
import androidx.glance.appwidget.cornerRadius
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
import androidx.glance.layout.padding
import androidx.glance.layout.width
import androidx.glance.state.PreferencesGlanceStateDefinition
import androidx.glance.text.FontWeight
import androidx.glance.text.Text
import androidx.glance.text.TextStyle
import androidx.glance.unit.ColorProvider
import com.conectify.android.R
import com.conectify.android.data.model.WidgetItemDto
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.widget.actions.NavigateAction
import com.conectify.android.widget.actions.TriggerDeviceAction
import com.google.gson.Gson

class SmallConectifyWidget : GlanceAppWidget() {

    override val stateDefinition = PreferencesGlanceStateDefinition

    override suspend fun provideGlance(context: Context, id: GlanceId) {
        if (AppPreferences(context).isConfigured()) {
            val state = getAppWidgetState(context, PreferencesGlanceStateDefinition, id)
            if (state[WidgetState.SMALL_ITEMS_JSON_KEY] == null) {
                SmallRefreshWorker.enqueue(context)
            }
        }

        provideContent {
            val prefs = currentState<Preferences>()
            val itemsJson = prefs[WidgetState.SMALL_ITEMS_JSON_KEY] ?: "[]"
            val index = prefs[WidgetState.SMALL_INDEX_KEY] ?: 0
            val items = Gson().fromJson(itemsJson, Array<WidgetItemDto>::class.java).toList()

            SmallWidgetContent(items = items, currentIndex = index)
        }
    }
}

@Composable
fun SmallWidgetContent(items: List<WidgetItemDto>, currentIndex: Int) {
    Box(
        modifier = GlanceModifier
            .fillMaxSize()
            .background(ColorProvider(Color(0xFF0E0E0E)))
            .cornerRadius(12.dp)
    ) {
        if (items.isEmpty()) {
            Box(modifier = GlanceModifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                Text(
                    text = "Loading…",
                    style = TextStyle(color = ColorProvider(Color(0xFF555555)), fontSize = 11.sp)
                )
            }
        } else {
            val safeIndex = currentIndex.coerceIn(0, items.lastIndex)
            val item = items[safeIndex]
            val total = items.size

            Row(
                modifier = GlanceModifier.fillMaxSize(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                // ◁ Prev button
                NavButton(
                    icon = R.drawable.ic_chevron_left,
                    onClick = actionRunCallback<NavigateAction>(
                        actionParametersOf(NavigateAction.DIRECTION_KEY to NavigateAction.PREV)
                    )
                )

                // Cube content
                Box(
                    modifier = GlanceModifier
                        .defaultWeight()
                        .fillMaxHeight()
                        .padding(vertical = 6.dp),
                    contentAlignment = Alignment.Center
                ) {
                    SmallCube(item = item, index = safeIndex, total = total)
                }

                // ▷ Next button
                NavButton(
                    icon = R.drawable.ic_chevron_right,
                    onClick = actionRunCallback<NavigateAction>(
                        actionParametersOf(NavigateAction.DIRECTION_KEY to NavigateAction.NEXT)
                    )
                )
            }
        }
    }
}

@Composable
private fun NavButton(icon: Int, onClick: androidx.glance.action.Action) {
    Box(
        modifier = GlanceModifier
            .width(32.dp)
            .fillMaxHeight()
            .clickable(onClick),
        contentAlignment = Alignment.Center
    ) {
        Image(
            provider = ImageProvider(icon),
            contentDescription = null,
            modifier = GlanceModifier.padding(8.dp)
        )
    }
}

@Composable
private fun SmallCube(item: WidgetItemDto, index: Int, total: Int) {
    val accent = parseHexColor(item.accentColor)

    Column(
        modifier = GlanceModifier.fillMaxWidth(),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // Name + page indicator row
        Row(
            modifier = GlanceModifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Spacer(modifier = GlanceModifier.defaultWeight())
            Text(
                text = item.name,
                style = TextStyle(color = ColorProvider(Color(0xFF9E9E9E)), fontSize = 10.sp),
                maxLines = 1
            )
            Spacer(modifier = GlanceModifier.defaultWeight())
            Text(
                text = "${index + 1}/$total",
                style = TextStyle(color = ColorProvider(Color(0xFF3A3A3A)), fontSize = 9.sp)
            )
        }

        Spacer(modifier = GlanceModifier.padding(top = 3.dp))

        if (item.type == WidgetItemDto.TYPE_ACTUATOR) {
            // ON / OFF action buttons
            Row(horizontalAlignment = Alignment.CenterHorizontally) {
                SmallActionButton(
                    label = "ON",
                    accent = accent,
                    active = true,
                    onClick = actionRunCallback<TriggerDeviceAction>(
                        actionParametersOf(
                            TriggerDeviceAction.DEVICE_ID_KEY to item.id,
                            TriggerDeviceAction.VALUE_KEY to item.maxVal
                        )
                    )
                )
                Spacer(modifier = GlanceModifier.padding(horizontal = 4.dp))
                SmallActionButton(
                    label = "OFF",
                    accent = accent,
                    active = false,
                    onClick = actionRunCallback<TriggerDeviceAction>(
                        actionParametersOf(
                            TriggerDeviceAction.DEVICE_ID_KEY to item.id,
                            TriggerDeviceAction.VALUE_KEY to item.minVal
                        )
                    )
                )
            }
        } else {
            // Sensor value chip (centered)
            val valueText = when {
                item.numericValue != null -> {
                    val n = if (item.numericValue % 1f == 0f) item.numericValue.toInt().toString()
                            else "%.1f".format(item.numericValue)
                    if (item.unit.isNotBlank()) "$n ${item.unit}" else n
                }
                item.stringValue.isNotBlank() -> item.stringValue
                else -> "—"
            }
            Box(
                modifier = GlanceModifier
                    .background(ColorProvider(accent.copy(alpha = 0.15f)))
                    .cornerRadius(5.dp)
                    .padding(horizontal = 8.dp, vertical = 2.dp)
            ) {
                Text(
                    text = valueText,
                    style = TextStyle(
                        color = ColorProvider(accent),
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Bold
                    )
                )
            }
        }
    }
}

@Composable
private fun SmallActionButton(
    label: String,
    accent: Color,
    active: Boolean,
    onClick: androidx.glance.action.Action
) {
    val bg = if (active) accent.copy(alpha = 0.18f) else Color(0xFF242424)
    val textColor = if (active) accent else Color(0xFF777777)
    Box(
        modifier = GlanceModifier
            .background(ColorProvider(bg))
            .cornerRadius(5.dp)
            .padding(horizontal = 10.dp, vertical = 4.dp)
            .clickable(onClick),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = label,
            style = TextStyle(color = ColorProvider(textColor), fontSize = 11.sp, fontWeight = FontWeight.Bold)
        )
    }
}

private fun parseHexColor(hex: String): Color =
    try { Color(android.graphics.Color.parseColor(hex)) }
    catch (_: IllegalArgumentException) { Color(0xFF4fc3f7) }
