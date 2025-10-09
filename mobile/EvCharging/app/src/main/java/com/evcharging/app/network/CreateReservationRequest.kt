package com.evcharging.app.network

data class CreateReservationRequest(
    val ownerId: String,
    val stationId: String,
    val slotId: String
)
