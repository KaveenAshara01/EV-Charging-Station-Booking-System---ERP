package com.evcharging.app.network

data class CreateReservationRequest(
    val stationId: String,
    val slotId: String,
    val reservationTimeUtc: String
)
