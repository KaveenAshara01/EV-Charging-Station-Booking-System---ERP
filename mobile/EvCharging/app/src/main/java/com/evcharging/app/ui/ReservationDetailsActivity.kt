package com.evcharging.app.ui

import android.content.Intent
import android.graphics.BitmapFactory
import android.os.Bundle
import android.util.Base64
import android.view.View
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.databinding.ActivityReservationDetailsBinding
import com.evcharging.app.network.ApiClient
import com.evcharging.app.network.ReservationResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ReservationDetailsActivity : AppCompatActivity() {

    private lateinit var binding: ActivityReservationDetailsBinding
    private lateinit var reservation: ReservationResponse

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityReservationDetailsBinding.inflate(layoutInflater)
        setContentView(binding.root)

        reservation = intent.getParcelableExtra("reservation") ?: run {
            Toast.makeText(this, "No reservation data", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        displayReservationDetails()

        binding.btnUpdate.setOnClickListener {
            Toast.makeText(this, "Update functionality coming soon...", Toast.LENGTH_SHORT).show()
        }

        binding.btnDelete.setOnClickListener {
            deleteReservation(reservation.reservationId)
        }
    }

    private fun displayReservationDetails() {
        binding.txtReservationId.text = "Reservation ID: ${reservation.reservationId}"
        binding.txtStation.text = "Station: ${reservation.stationId}"
        binding.txtSlot.text = "Slot: ${reservation.slotId}"
        binding.txtStatus.text = "Status: ${reservation.status}"
        binding.txtTime.text = "Reservation Time: ${reservation.reservationTimeUtc}"

        if (reservation.qrCodeData.isNullOrEmpty()) {
            binding.txtQrStatus.text = "Reservation not approved yet"
            binding.imageQr.visibility = View.GONE
        } else {
            val qrBytes = Base64.decode(reservation.qrCodeData, Base64.DEFAULT)
            val bitmap = BitmapFactory.decodeByteArray(qrBytes, 0, qrBytes.size)
            binding.imageQr.setImageBitmap(bitmap)
            binding.imageQr.visibility = View.VISIBLE
            binding.txtQrStatus.text = "Scan the QR code at the station"
        }
    }

    private fun deleteReservation(reservationId: String) {
        binding.btnDelete.isEnabled = false
        binding.btnDelete.text = "Deleting..."

        ApiClient.retrofitService.deleteReservation(reservationId)
            .enqueue(object : Callback<Void> {
                override fun onResponse(call: Call<Void>, response: Response<Void>) {
                    binding.btnDelete.isEnabled = true
                    binding.btnDelete.text = "Delete Reservation"

                    if (response.isSuccessful) {
                        Toast.makeText(
                            this@ReservationDetailsActivity,
                            "Reservation deleted successfully",
                            Toast.LENGTH_SHORT
                        ).show()

                        val intent = Intent(
                            this@ReservationDetailsActivity,
                            UserDashboardActivity::class.java
                        )
                        intent.flags = Intent.FLAG_ACTIVITY_CLEAR_TOP or Intent.FLAG_ACTIVITY_NEW_TASK
                        startActivity(intent)
                        finish()
                    } else {
                        Toast.makeText(
                            this@ReservationDetailsActivity,
                            "Failed to delete reservation",
                            Toast.LENGTH_SHORT
                        ).show()
                    }
                }

                override fun onFailure(call: Call<Void>, t: Throwable) {
                    binding.btnDelete.isEnabled = true
                    binding.btnDelete.text = "Delete Reservation"
                    Toast.makeText(
                        this@ReservationDetailsActivity,
                        "Error: ${t.localizedMessage}",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            })
    }
}
