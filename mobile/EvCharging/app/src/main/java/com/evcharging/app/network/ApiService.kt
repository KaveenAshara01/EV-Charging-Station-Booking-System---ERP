package com.evcharging.app.network
import android.os.Parcelable
import kotlinx.parcelize.Parcelize
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path


interface ApiService {
    @POST("Users/login")
    fun login(@Body request: LoginRequest): Call<LoginResponse>

    @POST("Users/register")
    fun register(@Body request: RegisterRequest): Call<LoginResponse>

    @POST("Operations/validate")
    fun validateQr(@Body request: QrValidationRequest): Call<QrValidationResponse>

    @POST("Operations/start")
    fun startReservation(@Body request: FinalizeRequest): Call<QrValidationResponse>

    @POST("Operations/finalize")
    fun finalizeReservation(@Body request: FinalizeRequest): Call<QrValidationResponse>

    @GET("Reservations/me")
    fun getMyReservations(): Call<List<com.evcharging.app.network.ReservationResponse>>

    @GET("Stations")
    fun getStations(): Call<List<StationResponse>>

    @GET("Stations/{id}/slots")
    fun getAvailableSlots(@Path("id") stationId: String): Call<List<SlotResponse>>


    @POST("Reservations")
    fun createReservation(@Body request: CreateReservationRequest): Call<Void>

    @GET("Reservations/{id}")
    fun getReservationById(@Path("id") reservationId: String): Call<ReservationResponse>

    @DELETE("Reservations/{id}")
    fun deleteReservation(@Path("id") reservationId: String): Call<Void>


}

// --- QR Validation Models ---

data class QrValidationResponse(
    val message: String,
    val reservation: Reservation?
)

data class FinalizeRequest(
    val reservationId: String
)


@Parcelize
data class Reservation(
    val reservationId: String,
    val ownerId: String,
    val stationId: String,
    val slotId: String,
    val reservationTimeUtc: String,
    val status: String,
    val qrCodeData: String?,
    val approvedBy: String?,
    val approvedAtUtc: String?,
    val startTime: String?,
    val endTime: String?,
    val createdAtUtc: String?,
    val updatedAtUtc: String?
): Parcelable

