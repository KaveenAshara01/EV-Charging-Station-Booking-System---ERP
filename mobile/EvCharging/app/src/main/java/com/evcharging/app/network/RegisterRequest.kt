package com.evcharging.app.network

data class RegisterRequest(
    val username: String,
    val email: String,
    val password: String,
    val role: String
)