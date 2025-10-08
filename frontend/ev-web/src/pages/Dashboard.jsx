import React, { useEffect, useState } from "react";
import api from "../api/apiClient";

export default function Dashboard() {
  const [summary, setSummary] = useState({
    pending: 0,
    approved: 0,
    upcoming: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchSummary() {
      try {
        setLoading(true);
        
        const res = await api.get("http://localhost:5074/api/Reservations");
        const reservations = res.data;

        let pendingCount = 0;
        let approvedCount = 0;
        let upcomingCount = 0;

        const nowUtc = new Date();

        // 2️⃣ Fetch slot details for each reservation
        await Promise.all(
          reservations.map(async (r) => {
            // Count pending / approved directly from reservation
            if (r.status === "Pending") pendingCount++;
            if (r.status === "Approved") approvedCount++;

            // Fetch slot info
            try {
              const slotRes = await api.get(`http://localhost:5074/api/Slots/${r.slotId}`);
              const slot = slotRes.data;

              // Check if the slot's endUtc is in the future
              if (slot && new Date(slot.endUtc) > nowUtc) {
                upcomingCount++;
              }
            } catch (err) {
              console.error(`Error fetching slot ${r.slotId}:`, err.message);
            }
          })
        );

        setSummary({ pending: pendingCount, approved: approvedCount, upcoming: upcomingCount });
      } catch (err) {
        console.error("Error fetching reservations:", err.message);
      } finally {
        setLoading(false);
      }
    }

    fetchSummary();
  }, []);

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Dashboard</h2>

      {loading ? (
        <p className="text-gray-500">Loading summary...</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-white p-4 rounded-lg shadow">
            <div className="text-sm text-slate-500">Pending Reservations</div>
            <div className="text-3xl font-bold">{summary.pending}</div>
          </div>
          <div className="bg-white p-4 rounded-lg shadow">
            <div className="text-sm text-slate-500">Approved Reservations</div>
            <div className="text-3xl font-bold">{summary.approved}</div>
          </div>
          <div className="bg-white p-4 rounded-lg shadow">
            <div className="text-sm text-slate-500">Upcoming</div>
            <div className="text-3xl font-bold">{summary.upcoming}</div>
          </div>
        </div>
      )}
    </div>
  );
}
