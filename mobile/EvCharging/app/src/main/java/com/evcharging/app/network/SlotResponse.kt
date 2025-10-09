package com.evcharging.app.network

data class SlotResponse(
    val slotId: String,
    val stationId: String,
    val startUtc: String,
    val endUtc: String,
    val isAvailable: Boolean,
    val reservationId: String?,
    val label: String?,
    val createdAtUtc: String?,
    val updatedAtUtc: String?
)
