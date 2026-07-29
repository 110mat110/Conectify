package com.conectify.android.data.api

import okhttp3.Interceptor
import okhttp3.Response
import okio.Buffer
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Records the last HTTP call so the widget can show it.
 *
 * The URL is captured from the request OkHttp actually sends rather than being rebuilt for display,
 * so what appears on the widget is exactly what went on the wire — including whatever Retrofit made
 * of the base URL and query parameters.
 */
object NetworkLog {

    /** Lines describing the most recent call: request line, body, outcome. */
    @Volatile
    var lastCall: String = ""
        private set

    fun record(vararg lines: String) {
        lastCall = lines.filter { it.isNotBlank() }.joinToString("\n")
    }

    fun timestamp(): String = SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date())
}

class RecordingInterceptor : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val started = System.currentTimeMillis()

        val body = request.body?.let { requestBody ->
            runCatching { Buffer().also { requestBody.writeTo(it) }.readUtf8() }.getOrDefault("")
        } ?: ""

        val requestLine = "${NetworkLog.timestamp()} ${request.method} ${request.url}"

        return try {
            val response = chain.proceed(request)
            val elapsed = System.currentTimeMillis() - started
            NetworkLog.record(requestLine, body, "← HTTP ${response.code} in ${elapsed}ms")
            response
        } catch (e: Exception) {
            val elapsed = System.currentTimeMillis() - started
            NetworkLog.record(requestLine, body, "✗ ${e.javaClass.simpleName} after ${elapsed}ms")
            throw e
        }
    }
}
