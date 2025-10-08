package com.evcharging.app.network

data class QrValidationRequest(
    val reservationId: String,
    val stationId: String
)
