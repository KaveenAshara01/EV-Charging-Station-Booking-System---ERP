package com.evcharging.app.network

data class LoginResponse(
    val username: String,
    val email: String,
    val role: String,
    val token: String
)
