package com.evcharging.app.ui

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

class CreateReservationActivity : AppCompatActivity() {

    private lateinit var spinnerStation: Spinner
    private lateinit var spinnerSlot: Spinner
    private lateinit var btnReserve: Button
    private lateinit var progressBar: ProgressBar

    private var stationList = mutableListOf<StationResponse>()
    private var slotList = mutableListOf<SlotResponse>()
    private var selectedStationId: String? = null
    private var selectedSlotId: String? = null

    private lateinit var dbHelper: DatabaseHelper

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_create_reservation)
        ApiClient.init(this) // ✅ same as UserDashboardActivity

        spinnerStation = findViewById(R.id.spinnerStation)
        spinnerSlot = findViewById(R.id.spinnerSlot)
        btnReserve = findViewById(R.id.btnReserve)
        progressBar = findViewById(R.id.progressBar)

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

        btnReserve.setOnClickListener {
            if (selectedStationId == null || selectedSlotId == null) {
                Toast.makeText(this, "Please select station and slot", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
            createReservation()
        }
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
            ownerId = userId,
            stationId = selectedStationId ?: "",
            slotId = selectedSlotId ?: ""
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
