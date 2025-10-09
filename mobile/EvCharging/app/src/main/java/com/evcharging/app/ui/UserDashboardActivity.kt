package com.evcharging.app.ui

import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import com.evcharging.app.database.DatabaseHelper
import com.evcharging.app.databinding.ActivityUserDashboardBinding
import com.evcharging.app.network.ApiClient
import com.evcharging.app.network.ReservationResponse
import com.evcharging.app.ui.adapters.ReservationAdapter
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class UserDashboardActivity : AppCompatActivity() {

    private lateinit var binding: ActivityUserDashboardBinding
    private lateinit var dbHelper: DatabaseHelper
    private lateinit var adapter: ReservationAdapter
    private val reservationList = mutableListOf<ReservationResponse>()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityUserDashboardBinding.inflate(layoutInflater)
        setContentView(binding.root)
        ApiClient.init(this)

        dbHelper = DatabaseHelper(this)

        val session = dbHelper.getUserSession()
        binding.txtWelcome.text = "Welcome, ${session?.get("name") ?: "User"}"

        adapter = ReservationAdapter(reservationList) { selectedReservation ->
            val intent = Intent(this, ReservationDetailsActivity::class.java)
            intent.putExtra("reservation", selectedReservation) // Parcelable!
            startActivity(intent)
        }

        binding.recyclerViewReservations.layoutManager = LinearLayoutManager(this)
        binding.recyclerViewReservations.adapter = adapter

        binding.btnCreateReservation.setOnClickListener {
            startActivity(Intent(this, CreateReservationActivity::class.java))
        }

        binding.btnMyReservations.setOnClickListener {
            startActivity(Intent(this, MyReservationsActivity::class.java))
        }

        fetchMyReservations()
    }

    override fun onResume() {
        super.onResume()
        // 🔁 Refresh reservations whenever user returns to dashboard
        fetchMyReservations()
    }

    private fun fetchMyReservations() {
        ApiClient.retrofitService.getMyReservations().enqueue(object : Callback<List<ReservationResponse>> {
            override fun onResponse(
                call: Call<List<ReservationResponse>>,
                response: Response<List<ReservationResponse>>
            ) {
                if (response.isSuccessful) {
                    reservationList.clear()
                    reservationList.addAll(response.body() ?: emptyList())
                    adapter.notifyDataSetChanged()
                } else {
                    Toast.makeText(this@UserDashboardActivity, "Failed to load reservations", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<List<ReservationResponse>>, t: Throwable) {
                Toast.makeText(this@UserDashboardActivity, "Error: ${t.localizedMessage}", Toast.LENGTH_SHORT).show()
            }
        })
    }
}
