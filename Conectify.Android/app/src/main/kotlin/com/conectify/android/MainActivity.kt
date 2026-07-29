package com.conectify.android

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.provider.Settings
import android.util.Log
import android.widget.Button
import android.widget.EditText
import android.widget.TextView
import androidx.activity.ComponentActivity
import androidx.lifecycle.lifecycleScope
import com.conectify.android.data.api.NetworkDiagnostics
import com.conectify.android.data.api.NetworkLog
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.conectify.android.widget.PackageReplacedReceiver
import com.conectify.android.widget.RefreshWorker
import com.conectify.android.widget.SmallRefreshWorker
import com.conectify.android.widget.WidgetTicker
import kotlinx.coroutines.launch
import retrofit2.HttpException

class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val prefs = AppPreferences(this)
        val editUrl = findViewById<EditText>(R.id.editServerUrl)
        val editUser = findViewById<EditText>(R.id.editUserMail)
        val btnSave = findViewById<Button>(R.id.btnSave)
        val tvStatus = findViewById<TextView>(R.id.tvStatus)
        val btnSelfTest = findViewById<Button>(R.id.btnSelfTest)
        val tvSelfTest = findViewById<TextView>(R.id.tvSelfTest)

        if (prefs.isConfigured()) editUrl.setText(prefs.serverUrl)
        if (prefs.userMail.isNotBlank()) editUser.setText(prefs.userMail)

        btnSelfTest.setOnClickListener { runSelfTest(btnSelfTest, tvSelfTest) }
        findViewById<Button>(R.id.btnAllowBackground).setOnClickListener { requestBackgroundNetwork() }

        // Opening the app always rebuilds the placed widgets. Android Studio's incremental install
        // does not reliably broadcast MY_PACKAGE_REPLACED, so this is the dependable way to get a
        // widget off a stale build's click targets.
        lifecycleScope.launch {
            runCatching { PackageReplacedReceiver.refreshWidgetViews(this@MainActivity) }
                .onFailure { Log.w("MainActivity", "Could not re-render widgets", it) }
        }

        btnSave.setOnClickListener {
            val url = editUrl.text.toString().trim()

            if (url.isBlank()) {
                showStatus(tvStatus, "URL cannot be empty", StatusType.ERROR)
                return@setOnClickListener
            }
            if (!url.startsWith("http://") && !url.startsWith("https://")) {
                showStatus(tvStatus, "URL must start with http:// or https://", StatusType.ERROR)
                return@setOnClickListener
            }

            prefs.serverUrl = url
            prefs.userMail = editUser.text.toString().trim()
            btnSave.isEnabled = false
            showStatus(tvStatus, "Connecting to $url …", StatusType.INFO)

            lifecycleScope.launch {
                try {
                    val (sensors, devices) = SensorRepository(this@MainActivity).getWidgetData()
                    showStatus(
                        tvStatus,
                        "✓ Connected — ${sensors.size} sensors, ${devices.size} actuators",
                        StatusType.SUCCESS
                    )
                    triggerWidgetRefresh()
                } catch (e: Exception) {
                    showStatus(tvStatus, "✗ ${e.javaClass.simpleName}: ${e.message}", StatusType.ERROR)
                } finally {
                    btnSave.isEnabled = true
                }
            }
        }
    }

    /**
     * Runs the widget's two server calls from inside the app, using the same repository the widget
     * uses. It splits the problem in one tap: if these succeed but the widget's own buttons do
     * nothing, the network and the server are fine and the fault is in the widget's click delivery;
     * if these fail, the phone cannot reach the service at all and the widget never could either.
     */
    private fun runSelfTest(button: Button, output: TextView) {
        val prefs = AppPreferences(this)
        val report = StringBuilder()

        fun render() = runOnUiThread { output.text = report.toString() }

        button.isEnabled = false
        report.appendLine("URL:  ${prefs.serverUrl.ifBlank { "(not set)" }}")
        report.appendLine("User: ${prefs.userMail.ifBlank { "(not set)" }}")
        report.appendLine()
        report.appendLine(NetworkDiagnostics.report(this))
        render()

        if (!prefs.isConfigured()) {
            report.appendLine("\n✗ No server URL — nothing to test.")
            render()
            button.isEnabled = true
            return
        }

        lifecycleScope.launch {
            val repository = SensorRepository(this@MainActivity)

            report.appendLine("\n1) GET /api/android/widget")
            render()

            val devices = try {
                val (sensors, actuators) = repository.getWidgetData()
                report.appendLine("   ✓ ${sensors.size} sensor(s), ${actuators.size} actuator(s)")
                report.appendLine(NetworkLog.lastCall.prependIndent("   "))
                actuators
            } catch (e: Exception) {
                report.appendLine("   ✗ ${describe(e)}")
                report.appendLine(NetworkLog.lastCall.prependIndent("   "))
                report.appendLine("\nThe phone cannot reach the service. The widget's refresh")
                report.appendLine("is failing the same way and its data is stale.")
                render()
                button.isEnabled = true
                return@launch
            }

            val target = devices.firstOrNull()
            if (target == null) {
                report.appendLine("\n2) POST skipped — no actuators returned.")
                render()
                button.isEnabled = true
                return@launch
            }

            report.appendLine("\n2) POST /api/android/actuator/${target.id.take(8)}…")
            report.appendLine("   \"${target.name}\"")
            report.appendLine("   ioType=${target.ioType} min=${target.minVal} max=${target.maxVal}")
            report.appendLine("   ON sends max = ${target.maxVal}")
            render()

            try {
                repository.triggerActuator(target.id, target.maxVal)
                report.appendLine("   ✓ accepted by the server")
            } catch (e: Exception) {
                report.appendLine("   ✗ ${describe(e)}")
            }
            report.appendLine(NetworkLog.lastCall.prependIndent("   "))

            render()
            button.isEnabled = true
        }
    }

    /**
     * Opens the battery-optimization exemption dialog. An optimized app is subject to the per-UID
     * firewall once it leaves the foreground, which is what makes the widget's requests time out
     * while the same URL loads instantly in a browser.
     */
    private fun requestBackgroundNetwork() {
        val intent = if (NetworkDiagnostics.isBatteryOptimized(this)) {
            Intent(Settings.ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS)
                .setData(Uri.parse("package:$packageName"))
        } else {
            // Already exempt — send them to app info, where OEM-specific restrictions also live.
            Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS)
                .setData(Uri.parse("package:$packageName"))
        }

        runCatching { startActivity(intent) }.onFailure {
            Log.w("MainActivity", "Could not open battery settings", it)
            startActivity(
                Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS)
                    .setData(Uri.parse("package:$packageName"))
            )
        }
    }

    private fun describe(e: Exception) = when (e) {
        is HttpException -> "HTTP ${e.code()} ${e.message()}"
        else -> "${e.javaClass.simpleName}: ${e.message}"
    }

    private fun triggerWidgetRefresh() {
        WidgetTicker.schedule(this)
        RefreshWorker.enqueueNow(this)
        SmallRefreshWorker.enqueueNow(this)
    }

    private fun showStatus(view: TextView, message: String, type: StatusType) {
        view.text = message
        view.setTextColor(
            getColor(
                when (type) {
                    StatusType.SUCCESS -> android.R.color.holo_green_light
                    StatusType.ERROR -> android.R.color.holo_red_light
                    StatusType.INFO -> android.R.color.white
                }
            )
        )
    }

    private enum class StatusType { SUCCESS, ERROR, INFO }
}
