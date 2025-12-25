package com.evcharging.app.ui

import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.database.DatabaseHelper
import com.evcharging.app.databinding.ActivityOperatorDashboardBinding
import com.evcharging.app.network.*
import com.google.zxing.integration.android.IntentIntegrator
import org.json.JSONObject
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class OperatorDashboardActivity : AppCompatActivity() {

    private lateinit var binding: ActivityOperatorDashboardBinding
    private lateinit var dbHelper: DatabaseHelper
//    private lateinit var operatorStationId: String

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityOperatorDashboardBinding.inflate(layoutInflater)
        setContentView(binding.root)
        ApiClient.init(this)

        dbHelper = DatabaseHelper(this)

        // Get logged-in operator info
        val user = dbHelper.getUserSession()

        if (user == null) {
            Toast.makeText(this, "No user session found. Please log in again.", Toast.LENGTH_SHORT).show()
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
            return
        }

        // Display operator details
        binding.txtOperatorName.text = "Name: ${user["name"]}"
        binding.txtOperatorEmail.text = "Email: ${user["email"]}"
        binding.txtOperatorRole.text = "Role: ${user["role"]}"

        // TODO: Replace with dynamic station ID from session if available
//        operatorStationId = "STATION001"  // temporary placeholder

        // QR Scan Button
        binding.btnScanQR.setOnClickListener {
            val integrator = IntentIntegrator(this)
            integrator.setDesiredBarcodeFormats(IntentIntegrator.QR_CODE)
            integrator.setPrompt("Scan reservation QR code")
            integrator.setCameraId(0)
            integrator.setBeepEnabled(true)
            integrator.setBarcodeImageEnabled(false)
            integrator.initiateScan()
        }

        // Logout
        binding.btnLogout.setOnClickListener {
            dbHelper.clearSession()
            Toast.makeText(this, "Logged out successfully", Toast.LENGTH_SHORT).show()
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
        }
    }

    // Handle QR scan result
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        val result = IntentIntegrator.parseActivityResult(requestCode, resultCode, data)
        if (result != null) {
            if (result.contents == null) {
                Toast.makeText(this, "Scan cancelled", Toast.LENGTH_SHORT).show()
            } else {
                val qrData = result.contents.trim()
                handleQrData(qrData)
            }
        } else {
            super.onActivityResult(requestCode, resultCode, data)
        }
    }

    private fun handleQrData(qrData: String) {
        try {
            val json = JSONObject(qrData)

            // ✅ Extract nested payload object
            val payload = if (json.has("payload")) json.getJSONObject("payload") else json

            val reservationId = payload.optString("reservationId", "")
            val stationIdFromQr = payload.optString("stationId", "")

            if (reservationId.isEmpty() || stationIdFromQr.isEmpty()) {
                Toast.makeText(this, "Invalid QR data structure", Toast.LENGTH_SHORT).show()
                return
            }

            // ✅ Use the stationId from operator side, not QR (for security)
            validateQr(reservationId, stationIdFromQr)

        } catch (e: Exception) {
            Toast.makeText(this, "Error parsing QR: ${e.message}", Toast.LENGTH_SHORT).show()
            e.printStackTrace()
        }
    }

//    private fun validateQr(reservationId: String, stationId: String) {
//        val request = QrValidationRequest(reservationId, stationId)
//
//        ApiClient.retrofitService.validateQr(request).enqueue(object : Callback<QrValidationResponse> {
//            override fun onResponse(call: Call<QrValidationResponse>, response: Response<QrValidationResponse>) {
//                if (response.isSuccessful) {
//                    val qrResp = response.body()
//                    if (qrResp?.reservation != null) {
//                        val intent = Intent(this@OperatorDashboardActivity, QrValidationResultActivity::class.java)
//                        intent.putExtra("reservation", qrResp.reservation)
//                        intent.putExtra("message", qrResp.message)
//                        startActivity(intent)
//                    } else {
//                        Toast.makeText(this@OperatorDashboardActivity, "Invalid or expired QR reservation", Toast.LENGTH_SHORT).show()
//                    }
//                } else {
//                    Toast.makeText(this@OperatorDashboardActivity, "Validation failed (${response.code()})", Toast.LENGTH_SHORT).show()
//                }
//            }
//
//            override fun onFailure(call: Call<QrValidationResponse>, t: Throwable) {
//                Toast.makeText(this@OperatorDashboardActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
//            }
//        })
//    }

    private fun validateQr(reservationId: String, stationId: String) {
        val request = QrValidationRequest(reservationId, stationId)

        Log.d("QR_VALIDATE", "Sending Request: reservationId=$reservationId , stationId=$stationId")

        ApiClient.retrofitService.validateQr(request).enqueue(object : Callback<QrValidationResponse> {

            override fun onResponse(call: Call<QrValidationResponse>, response: Response<QrValidationResponse>) {

                Log.d("QR_VALIDATE", "HTTP Status: ${response.code()}")

                if (!response.isSuccessful) {
                    // **** PRINT FULL ERROR BODY FROM SERVER ****
                    val errorBody = response.errorBody()?.string()
                    Log.e("QR_VALIDATE", "Error Body: $errorBody")

                    Toast.makeText(
                        this@OperatorDashboardActivity,
                        "Validation failed (${response.code()})",
                        Toast.LENGTH_SHORT
                    ).show()
                    return
                }

                val qrResp = response.body()
                Log.d("QR_VALIDATE", "Response Body: $qrResp")

                if (qrResp?.reservation != null) {
                    val intent = Intent(this@OperatorDashboardActivity, QrValidationResultActivity::class.java)
                    intent.putExtra("reservation", qrResp.reservation)
                    intent.putExtra("message", qrResp.message)
                    startActivity(intent)
                } else {
                    Log.w("QR_VALIDATE", "Reservation is null → Invalid QR")
                    Toast.makeText(
                        this@OperatorDashboardActivity,
                        "Invalid or expired QR reservation",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            }

            override fun onFailure(call: Call<QrValidationResponse>, t: Throwable) {
                Log.e("QR_VALIDATE", "Network failure", t)
                Toast.makeText(
                    this@OperatorDashboardActivity,
                    "Error: ${t.localizedMessage}",
                    Toast.LENGTH_SHORT
                ).show()
            }
        })
    }

}
