package com.evcharging.app.ui.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.evcharging.app.R
import com.evcharging.app.network.ReservationResponse

class ReservationAdapter(
    private val items: List<ReservationResponse>,
    private val onClick: ((ReservationResponse) -> Unit)? = null
) : RecyclerView.Adapter<ReservationAdapter.VH>() {

    inner class VH(view: View) : RecyclerView.ViewHolder(view) {
        val txtStation: TextView = view.findViewById(R.id.item_station)
        val txtSlot: TextView = view.findViewById(R.id.item_slot)
        val txtStatus: TextView = view.findViewById(R.id.item_status)
        val txtWhen: TextView = view.findViewById(R.id.item_when)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): VH {
        val v = LayoutInflater.from(parent.context).inflate(R.layout.item_reservation, parent, false)
        return VH(v)
    }

    override fun onBindViewHolder(holder: VH, position: Int) {
        val reservation = items[position]
        holder.txtStation.text = reservation.stationId ?: "Unknown station"
        // Prefer label-like display for the slot (if slotId is not descriptive, you can show time)
        holder.txtSlot.text = reservation.slotId ?: (reservation.reservationTimeUtc ?: "Slot: N/A")
        holder.txtStatus.text = reservation.status ?: "Status: N/A"
        holder.txtWhen.text = reservation.reservationTimeUtc ?: ""
        holder.itemView.setOnClickListener { onClick?.invoke(reservation) }
    }

    override fun getItemCount(): Int = items.size
}
