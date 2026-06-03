package com.conectify.android.data.api

import com.conectify.android.data.api.response.CurrentValueResponse
import com.conectify.android.data.api.response.WidgetDataResponse
import com.conectify.android.data.model.SetActuatorValueRequest
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

interface ConectifyApi {

    @GET("api/android/widget")
    suspend fun getWidgetData(
        @Query("user") userMail: String = "",
        @Query("widgetType") widgetType: String = "large"
    ): WidgetDataResponse

    @GET("api/android/values")
    suspend fun getCurrentValues(): List<CurrentValueResponse>

    @POST("api/android/actuator/{id}")
    suspend fun triggerActuator(
        @Path("id") id: String,
        @Body body: SetActuatorValueRequest
    )
}
