package com.evcharging.app.ui

import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.databinding.ActivityLoginBinding
import com.evcharging.app.network.ApiClient
import com.evcharging.app.network.LoginRequest
import com.evcharging.app.network.LoginResponse
import com.evcharging.app.database.DatabaseHelper
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class LoginActivity : AppCompatActivity() {

    private lateinit var binding: ActivityLoginBinding
    private lateinit var dbHelper: DatabaseHelper

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityLoginBinding.inflate(layoutInflater)
        setContentView(binding.root)
        ApiClient.init(this)

        dbHelper = DatabaseHelper(this)

        binding.btnLogin.setOnClickListener {
            val email = binding.editEmail.text.toString().trim()
            val password = binding.editPassword.text.toString().trim()

            if (email.isEmpty() || password.isEmpty()) {
                Toast.makeText(this, "Please enter email and password", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            loginUser(email, password)
        }

        // Register only for EV Owner (just a toast for now)
        binding.txtRegister.setOnClickListener {
            Toast.makeText(this, "Register screen coming soon", Toast.LENGTH_SHORT).show()
        }
    }

    private fun loginUser(email: String, password: String) {
        val request = LoginRequest(email, password)

        ApiClient.retrofitService.login(request).enqueue(object : Callback<LoginResponse> {
            override fun onResponse(call: Call<LoginResponse>, response: Response<LoginResponse>) {
                if (response.isSuccessful) {
                    val loginResp = response.body()
                    if (loginResp != null) {

                        // Save login details (session) in SQLite
                        dbHelper.saveUserSession(
                            loginResp.username,
                            loginResp.email,
                            loginResp.role,
                            loginResp.token
                        )

                        when (loginResp.role.uppercase()) {
                            "STATION_OPERATOR" -> {
                                Toast.makeText(
                                    this@LoginActivity,
                                    "Station Operator login successful",
                                    Toast.LENGTH_SHORT
                                ).show()

                                // Navigate to Operator Dashboard
                                val intent = Intent(this@LoginActivity, OperatorDashboardActivity::class.java)
                                startActivity(intent)
                                finish()
                            }

                            "USER" -> {
                                Toast.makeText(
                                    this@LoginActivity,
                                    "EV Owner login successful",
                                    Toast.LENGTH_SHORT
                                ).show()
                                val intent = Intent(this@LoginActivity, UserDashboardActivity::class.java)
                                startActivity(intent)
                                finish()
                            }


                            else -> {
                                Toast.makeText(
                                    this@LoginActivity,
                                    "Unknown role: ${loginResp.role}",
                                    Toast.LENGTH_SHORT
                                ).show()
                            }
                        }
                    } else {
                        Toast.makeText(this@LoginActivity, "Empty response", Toast.LENGTH_SHORT).show()
                    }
                } else {
                    Toast.makeText(this@LoginActivity, "Login failed: ${response.code()}", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<LoginResponse>, t: Throwable) {
                Toast.makeText(this@LoginActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        })
    }
}
