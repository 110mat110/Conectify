package com.conectify.android.data.api

import android.app.usage.UsageStatsManager
import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import android.os.Build
import android.os.PowerManager

/**
 * Reports the OS-level restrictions that decide whether a background request is allowed out.
 *
 * This matters because Android enforces those restrictions with per-UID firewall rules that *drop*
 * packets rather than rejecting them. A restricted app therefore sees its SYN disappear and fails
 * with a connect timeout — the same symptom as an unreachable host — while the very same URL loads
 * instantly in a browser, because the browser is in the foreground and the rules do not apply to it.
 */
object NetworkDiagnostics {

    fun report(context: Context): String = buildList {
        add("battery: ${batteryState(context)}")
        add("dataSaver: ${dataSaverState(context)}")
        add("bucket: ${standbyBucket(context)}")
        add("network: ${networkState(context)}")
    }.joinToString("\n")

    /** The one that usually matters: an optimized app loses background network when idle. */
    fun isBatteryOptimized(context: Context): Boolean {
        val power = context.getSystemService(Context.POWER_SERVICE) as? PowerManager ?: return false
        return !power.isIgnoringBatteryOptimizations(context.packageName)
    }

    private fun batteryState(context: Context) =
        if (isBatteryOptimized(context)) "OPTIMIZED (restricts background)" else "unrestricted"

    private fun dataSaverState(context: Context): String {
        val connectivity = context.getSystemService(Context.CONNECTIVITY_SERVICE) as? ConnectivityManager
            ?: return "unknown"
        return when (connectivity.restrictBackgroundStatus) {
            ConnectivityManager.RESTRICT_BACKGROUND_STATUS_ENABLED -> "ENABLED (blocks background)"
            ConnectivityManager.RESTRICT_BACKGROUND_STATUS_WHITELISTED -> "on, app allowed"
            else -> "off"
        }
    }

    private fun standbyBucket(context: Context): String {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.P) return "n/a"
        val usage = context.getSystemService(Context.USAGE_STATS_SERVICE) as? UsageStatsManager
            ?: return "unknown"
        return when (val bucket = usage.appStandbyBucket) {
            10 -> "ACTIVE"
            20 -> "WORKING_SET"
            30 -> "FREQUENT"
            40 -> "RARE"
            45 -> "RESTRICTED (blocks background)"
            else -> "bucket $bucket"
        }
    }

    private fun networkState(context: Context): String {
        val connectivity = context.getSystemService(Context.CONNECTIVITY_SERVICE) as? ConnectivityManager
            ?: return "unknown"
        val capabilities = connectivity.getNetworkCapabilities(connectivity.activeNetwork)
            ?: return "none"

        val transport = when {
            capabilities.hasTransport(NetworkCapabilities.TRANSPORT_WIFI) -> "wifi"
            capabilities.hasTransport(NetworkCapabilities.TRANSPORT_CELLULAR) -> "cellular"
            capabilities.hasTransport(NetworkCapabilities.TRANSPORT_VPN) -> "vpn"
            else -> "other"
        }
        val validated = capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED)
        return "$transport validated=$validated"
    }
}
