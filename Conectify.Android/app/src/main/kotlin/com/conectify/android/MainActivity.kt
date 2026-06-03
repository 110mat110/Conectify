package com.conectify.android

import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.TextView
import androidx.activity.ComponentActivity
import androidx.lifecycle.lifecycleScope
import androidx.work.ExistingWorkPolicy
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import com.conectify.android.data.preferences.AppPreferences
import com.conectify.android.data.repository.SensorRepository
import com.conectify.android.widget.ConectifyWidget
import com.conectify.android.widget.RefreshWorker
import kotlinx.coroutines.launch

class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val prefs = AppPreferences(this)
        val editUrl = findViewById<EditText>(R.id.editServerUrl)
        val editUser = findViewById<EditText>(R.id.editUserMail)
        val btnSave = findViewById<Button>(R.id.btnSave)
        val tvStatus = findViewById<TextView>(R.id.tvStatus)

        if (prefs.isConfigured()) editUrl.setText(prefs.serverUrl)
        if (prefs.userMail.isNotBlank()) editUser.setText(prefs.userMail)

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

    private fun triggerWidgetRefresh() {
        WorkManager.getInstance(this).enqueueUniqueWork(
            ConectifyWidget.INITIAL_LOAD_WORK,
            ExistingWorkPolicy.REPLACE,
            OneTimeWorkRequestBuilder<RefreshWorker>().build()
        )
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
