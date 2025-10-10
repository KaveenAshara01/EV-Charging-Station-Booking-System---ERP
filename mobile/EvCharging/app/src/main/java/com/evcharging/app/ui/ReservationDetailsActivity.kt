package com.evcharging.app.ui

import android.app.DatePickerDialog
import android.app.TimePickerDialog
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.databinding.ActivityReservationDetailsBinding
import com.evcharging.app.databinding.DialogUpdateReservationBinding
import com.evcharging.app.network.ApiClient
import com.evcharging.app.network.ReservationResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.text.SimpleDateFormat
import java.util.*

class ReservationDetailsActivity : AppCompatActivity() {

    private lateinit var binding: ActivityReservationDetailsBinding
    private lateinit var reservation: ReservationResponse
    private var selectedNewTimeUtc: String? = null
    private val calendar = Calendar.getInstance()

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
            showUpdateDialog()
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
            val qrBytes = android.util.Base64.decode(reservation.qrCodeData, android.util.Base64.DEFAULT)
            val bitmap = android.graphics.BitmapFactory.decodeByteArray(qrBytes, 0, qrBytes.size)
            binding.imageQr.setImageBitmap(bitmap)
            binding.imageQr.visibility = View.VISIBLE
            binding.txtQrStatus.text = "Scan the QR code at the station"
        }
    }

    // 🟦 Step 2: Show Update Dialog
    private fun showUpdateDialog() {
        val dialogBinding = DialogUpdateReservationBinding.inflate(LayoutInflater.from(this))
        val dialog = AlertDialog.Builder(this)
            .setView(dialogBinding.root)
            .create()

        dialogBinding.btnPickNewDateTime.setOnClickListener {
            showDateTimePicker { time ->
                selectedNewTimeUtc = time
                dialogBinding.tvNewSelectedDateTime.text = selectedNewTimeUtc
            }
        }

        dialogBinding.btnConfirmUpdate.setOnClickListener {
            if (selectedNewTimeUtc == null) {
                Toast.makeText(this, "Please select a new date and time", Toast.LENGTH_SHORT).show()
            } else {
                updateReservation(reservation.reservationId, selectedNewTimeUtc!!)
                dialog.dismiss()
            }
        }

        dialogBinding.btnCancelUpdate.setOnClickListener {
            dialog.dismiss()
        }

        dialog.show()
    }

    // 🟨 Step 3: Date & Time Picker
    private fun showDateTimePicker(onDateSelected: (String) -> Unit) {
        val now = Calendar.getInstance()
        DatePickerDialog(
            this,
            { _, year, month, day ->
                calendar.set(Calendar.YEAR, year)
                calendar.set(Calendar.MONTH, month)
                calendar.set(Calendar.DAY_OF_MONTH, day)

                TimePickerDialog(
                    this,
                    { _, hourOfDay, minute ->
                        calendar.set(Calendar.HOUR_OF_DAY, hourOfDay)
                        calendar.set(Calendar.MINUTE, minute)
                        calendar.set(Calendar.SECOND, 0)
                        calendar.set(Calendar.MILLISECOND, 0)

                        onDateSelected(convertToUtcIso(calendar.time))
                    },
                    now.get(Calendar.HOUR_OF_DAY),
                    now.get(Calendar.MINUTE),
                    true
                ).show()
            },
            now.get(Calendar.YEAR),
            now.get(Calendar.MONTH),
            now.get(Calendar.DAY_OF_MONTH)
        ).show()
    }

    private fun convertToUtcIso(date: Date): String {
        val dateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.getDefault())
        dateFormat.timeZone = TimeZone.getTimeZone("UTC")
        return dateFormat.format(date)
    }

    // 🟥 Step 4: API call to update reservation
    private fun updateReservation(reservationId: String, newTime: String) {
        val body = mapOf("reservationTimeUtc" to newTime)
        ApiClient.retrofitService.updateReservation(reservationId, body)
            .enqueue(object : Callback<Void> {
                override fun onResponse(call: Call<Void>, response: Response<Void>) {
                    if (response.isSuccessful) {
                        Toast.makeText(
                            this@ReservationDetailsActivity,
                            "Reservation updated successfully",
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
                            "Failed to update reservation",
                            Toast.LENGTH_SHORT
                        ).show()
                    }
                }

                override fun onFailure(call: Call<Void>, t: Throwable) {
                    Toast.makeText(
                        this@ReservationDetailsActivity,
                        "Error: ${t.localizedMessage}",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            })
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
