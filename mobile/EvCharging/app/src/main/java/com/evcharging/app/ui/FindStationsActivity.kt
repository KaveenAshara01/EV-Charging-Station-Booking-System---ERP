package com.evcharging.app.ui

import android.os.Bundle
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.evcharging.app.R
import com.evcharging.app.network.ApiClient
import com.evcharging.app.network.StationResponse
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.GoogleMap
import com.google.android.gms.maps.OnMapReadyCallback
import com.google.android.gms.maps.SupportMapFragment
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.MarkerOptions
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class FindStationsActivity : AppCompatActivity(), OnMapReadyCallback {

    private lateinit var googleMap: GoogleMap

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_find_stations)

        ApiClient.init(this)

        val mapFragment = supportFragmentManager
            .findFragmentById(R.id.map) as SupportMapFragment
        mapFragment.getMapAsync(this)
    }

    override fun onMapReady(map: GoogleMap) {
        googleMap = map
        fetchStations()
    }

    private fun fetchStations() {
        ApiClient.retrofitService.getStations().enqueue(object : Callback<List<StationResponse>> {
            override fun onResponse(
                call: Call<List<StationResponse>>,
                response: Response<List<StationResponse>>
            ) {
                if (response.isSuccessful) {
                    val stations = response.body() ?: emptyList()
                    if (stations.isNotEmpty()) {
                        for (station in stations) {
                            val position = LatLng(station.latitude, station.longitude)
                            googleMap.addMarker(
                                MarkerOptions()
                                    .position(position)
                                    .title(station.stationId)
                                    .snippet(station.address)
                            )
                        }
                        // Focus camera to first station
                        val first = stations.first()
                        googleMap.moveCamera(
                            CameraUpdateFactory.newLatLngZoom(
                                LatLng(first.latitude, first.longitude), 11f
                            )
                        )
                    } else {
                        Toast.makeText(this@FindStationsActivity, "No stations found", Toast.LENGTH_SHORT).show()
                    }
                } else {
                    Toast.makeText(this@FindStationsActivity, "Failed to load stations", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<List<StationResponse>>, t: Throwable) {
                Toast.makeText(this@FindStationsActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        })
    }
}
