import React, { useEffect, useState } from "react";
import api from "../api/apiClient";

export default function Dashboard() {
  const [summary, setSummary] = useState({
    pending: 0,
    approved: 0,
    upcoming: 0,
  });

  useEffect(() => {
    // Example: you might implement a backend endpoint /api/admin/summary
    (async () => {
      try {
        const res = await api.get("/api/admin/summary"); // OPTIONAL - implement in backend
        setSummary(res.data);
      } catch (err) {
        // fallback: you can compute using other endpoints
        console.log("Summary endpoint not available:", err.message);
      }
    })();
  }, []);

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Dashboard</h2>
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
    </div>
  );
}
