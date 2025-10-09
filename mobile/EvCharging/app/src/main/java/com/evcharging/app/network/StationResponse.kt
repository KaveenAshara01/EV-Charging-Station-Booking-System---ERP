package com.evcharging.app.network

data class StationResponse(
    val id: String,
    val stationId: String,
    val uniqueIdentifier: String?,
    val latitude: Double,
    val longitude: Double,
    val address: String
)
