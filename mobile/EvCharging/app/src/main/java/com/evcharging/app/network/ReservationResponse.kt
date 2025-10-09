package com.evcharging.app.network

// Matches the ReservationResponseDto returned by the backend
data class ReservationResponse(
    val reservationId: String,
    val ownerId: String? = null,
    val stationId: String? = null,
    val slotId: String? = null,
    val reservationTimeUtc: String? = null,
    val status: String? = null,
    val createdAtUtc: String? = null,
    val updatedAtUtc: String? = null,
    val qrCodeData: String? = null,
    val approvedBy: String? = null,
    val approvedAtUtc: String? = null,
    val startTime: String? = null,
    val endTime: String? = null
)
