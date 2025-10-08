package com.evcharging.app.ui

import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.databinding.ActivityQrValidationResultBinding
import com.evcharging.app.network.*
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.text.SimpleDateFormat
import java.util.*

class QrValidationResultActivity : AppCompatActivity() {

    private lateinit var binding: ActivityQrValidationResultBinding
    private var reservation: Reservation? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityQrValidationResultBinding.inflate(layoutInflater)
        setContentView(binding.root)

        reservation = intent.getParcelableExtra("reservation")
        val message = intent.getStringExtra("message")

        if (reservation == null) {
            Toast.makeText(this, "Reservation data is missing", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        // Format time
        val formatter = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault())
        val currentTime = formatter.format(Date())

        // Display data
        binding.txtValidationMessage.text = message ?: "Unknown result"
        binding.txtReservationId.text = "Reservation ID: ${reservation!!.reservationId}"
        binding.txtStatus.text = "Status: ${reservation!!.status}"
        binding.txtOwnerId.text = "Owner ID: ${reservation!!.ownerId}"
        binding.txtStationId.text = "Station ID: ${reservation!!.stationId}"
        binding.txtSlotId.text = "Slot ID: ${reservation!!.slotId}"
        binding.txtReservationTime.text = "Reservation Time: ${reservation!!.reservationTimeUtc}"
        binding.txtStartTime.text = "Start Time: ${currentTime}"
        binding.txtEndTime.text = "End Time: ${reservation!!.endTime ?: "N/A"}"
        binding.txtApprovedBy.text = "Approved By: ${reservation!!.approvedBy ?: "N/A"}"
        binding.txtApprovedAt.text = "Approved At: ${reservation!!.approvedAtUtc ?: "N/A"}"

        // Call backend to record StartTime
        ApiClient.retrofitService.startReservation(FinalizeRequest(reservation!!.reservationId))
            .enqueue(object : Callback<QrValidationResponse> {
                override fun onResponse(
                    call: Call<QrValidationResponse>,
                    response: Response<QrValidationResponse>
                ) {
                    if (response.isSuccessful) {
                        val updated = response.body()?.reservation
                        if (updated != null) {
                            reservation = updated
                            binding.txtStartTime.text = "Start Time: ${updated.startTime}"
                        }
                    } else {
                        Toast.makeText(this@QrValidationResultActivity, "Failed to record start time", Toast.LENGTH_SHORT).show()
                    }
                }

                override fun onFailure(call: Call<QrValidationResponse>, t: Throwable) {
                    Toast.makeText(this@QrValidationResultActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
                }
            })

        binding.btnComplete.setOnClickListener {
            ApiClient.retrofitService.finalizeReservation(FinalizeRequest(reservation!!.reservationId))
                .enqueue(object : Callback<QrValidationResponse> {
                    override fun onResponse(
                        call: Call<QrValidationResponse>,
                        response: Response<QrValidationResponse>
                    ) {
                        if (response.isSuccessful) {
                            Toast.makeText(this@QrValidationResultActivity, "Charging session completed", Toast.LENGTH_SHORT).show()
                            startActivity(Intent(this@QrValidationResultActivity, OperatorDashboardActivity::class.java))
                            finish()
                        } else {
                            Toast.makeText(this@QrValidationResultActivity, "Failed to complete session", Toast.LENGTH_SHORT).show()
                        }
                    }

                    override fun onFailure(call: Call<QrValidationResponse>, t: Throwable) {
                        Toast.makeText(this@QrValidationResultActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
                    }
                })
        }
    }
}
