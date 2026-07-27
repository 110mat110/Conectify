package com.conectify.android.data.api

import com.conectify.android.data.api.response.WidgetDataResponse
import com.conectify.android.data.model.SetActuatorValueRequest
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

interface ConectifyApi {

    /**
     * The one read the widget needs. It is answered from the Android service's in-memory caches, so
     * it is cheap enough to call on every refresh — there is no separate values endpoint.
     */
    @GET("api/android/widget")
    suspend fun getWidgetData(
        @Query("user") userMail: String = "",
        @Query("widgetType") widgetType: String = "large"
    ): WidgetDataResponse

    @POST("api/android/actuator/{id}")
    suspend fun triggerActuator(
        @Path("id") id: String,
        @Body body: SetActuatorValueRequest
    )
}
