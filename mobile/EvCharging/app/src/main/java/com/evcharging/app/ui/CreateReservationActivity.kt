package com.evcharging.app.ui

import android.app.DatePickerDialog
import android.app.TimePickerDialog
import android.os.Bundle
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.R
import com.evcharging.app.database.DatabaseHelper
import com.evcharging.app.network.ApiClient
import com.evcharging.app.network.CreateReservationRequest
import com.evcharging.app.network.SlotResponse
import com.evcharging.app.network.StationResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.text.SimpleDateFormat
import java.util.*

class CreateReservationActivity : AppCompatActivity() {

    private lateinit var spinnerStation: Spinner
    private lateinit var spinnerSlot: Spinner
    private lateinit var btnReserve: Button
    private lateinit var progressBar: ProgressBar

    private lateinit var btnPickDateTime: Button
    private lateinit var tvSelectedDateTime: TextView

    private var stationList = mutableListOf<StationResponse>()
    private var slotList = mutableListOf<SlotResponse>()
    private var selectedStationId: String? = null
    private var selectedSlotId: String? = null
    private var selectedReservationTimeUtc: String? = null

    private lateinit var dbHelper: DatabaseHelper
    private val calendar = Calendar.getInstance()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_create_reservation)
        ApiClient.init(this)

        spinnerStation = findViewById(R.id.spinnerStation)
        spinnerSlot = findViewById(R.id.spinnerSlot)
        btnReserve = findViewById(R.id.btnReserve)
        progressBar = findViewById(R.id.progressBar)
        btnPickDateTime = findViewById(R.id.btnPickDateTime)
        tvSelectedDateTime = findViewById(R.id.tvSelectedDateTime)

        dbHelper = DatabaseHelper(this)

        fetchStations()

        spinnerStation.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                val station = stationList[position]
                selectedStationId = station.stationId
                fetchSlotsForStation(station.stationId)
            }
            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        spinnerSlot.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                selectedSlotId = slotList.getOrNull(position)?.slotId
            }
            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        btnPickDateTime.setOnClickListener { showDateTimePicker() }

        btnReserve.setOnClickListener {
            if (selectedStationId == null || selectedSlotId == null) {
                Toast.makeText(this, "Please select station and slot", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
            if (selectedReservationTimeUtc == null) {
                Toast.makeText(this, "Please select reservation date and time", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
            createReservation()
        }
    }

    private fun showDateTimePicker() {
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

                        selectedReservationTimeUtc = convertToUtcIso(calendar.time)
                        tvSelectedDateTime.text = selectedReservationTimeUtc
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

    private fun fetchStations() {
        progressBar.visibility = View.VISIBLE
        ApiClient.retrofitService.getStations().enqueue(object : Callback<List<StationResponse>> {
            override fun onResponse(call: Call<List<StationResponse>>, response: Response<List<StationResponse>>) {
                progressBar.visibility = View.GONE
                if (response.isSuccessful) {
                    stationList.clear()
                    stationList.addAll(response.body() ?: emptyList())

                    val stationDisplayNames = stationList.map {
                        "${it.stationId} - ${it.address ?: "Unknown address"}"
                    }

                    spinnerStation.adapter = ArrayAdapter(
                        this@CreateReservationActivity,
                        android.R.layout.simple_spinner_item,
                        stationDisplayNames
                    ).apply {
                        setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
                    }
                } else {
                    Toast.makeText(this@CreateReservationActivity, "Failed to load stations", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<List<StationResponse>>, t: Throwable) {
                progressBar.visibility = View.GONE
                Toast.makeText(this@CreateReservationActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun fetchSlotsForStation(stationId: String) {
        progressBar.visibility = View.VISIBLE
        ApiClient.retrofitService.getAvailableSlots(stationId).enqueue(object : Callback<List<SlotResponse>> {
            override fun onResponse(call: Call<List<SlotResponse>>, response: Response<List<SlotResponse>>) {
                progressBar.visibility = View.GONE
                if (response.isSuccessful) {
                    val slots = response.body() ?: emptyList()
                    slotList.clear()
                    slotList.addAll(slots)

                    val slotDisplayNames = slotList.map {
                        "${it.label ?: "Slot"} (${it.startUtc.take(16)} - ${it.endUtc.take(16)})"
                    }

                    spinnerSlot.adapter = ArrayAdapter(
                        this@CreateReservationActivity,
                        android.R.layout.simple_spinner_item,
                        slotDisplayNames
                    ).apply {
                        setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
                    }

                } else {
                    Toast.makeText(this@CreateReservationActivity, "Failed to load slots", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<List<SlotResponse>>, t: Throwable) {
                progressBar.visibility = View.GONE
                Toast.makeText(this@CreateReservationActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun createReservation() {
        progressBar.visibility = View.VISIBLE

        val session = dbHelper.getUserSession()
        val userId = session?.get("userId") ?: ""

        val request = CreateReservationRequest(
            stationId = selectedStationId ?: "",
            slotId = selectedSlotId ?: "",
            reservationTimeUtc = selectedReservationTimeUtc ?: ""
        )

        ApiClient.retrofitService.createReservation(request).enqueue(object : Callback<Void> {
            override fun onResponse(call: Call<Void>, response: Response<Void>) {
                progressBar.visibility = View.GONE
                if (response.isSuccessful) {
                    Toast.makeText(this@CreateReservationActivity, "Reservation created!", Toast.LENGTH_SHORT).show()
                    finish()
                } else {
                    Toast.makeText(this@CreateReservationActivity, "Failed to create reservation", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<Void>, t: Throwable) {
                progressBar.visibility = View.GONE
                Toast.makeText(this@CreateReservationActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        })
    }
}
