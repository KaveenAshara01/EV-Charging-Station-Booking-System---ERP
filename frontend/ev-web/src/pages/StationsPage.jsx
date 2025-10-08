import React, { useEffect, useState } from "react";
import api from "../api/apiClient";

export default function StationsPage() {
  const [stations, setStations] = useState([]);

  useEffect(() => {
    (async () => {
      try {
        const res = await api.get("/api/stations"); // optional: implement in backend
        setStations(res.data);
      } catch (err) {
        console.log("Stations endpoint not available", err.message);
      }
    })();
  }, []);

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Charging Stations</h2>
      <div className="grid gap-3">
        {stations.length === 0 && <div className="text-slate-500">No stations available (backend endpoint missing)</div>}
        {stations.map(s => (
          <div key={s.stationId} className="bg-white p-4 rounded shadow">
            <div className="font-medium">{s.name ?? s.stationId}</div>
            <div className="text-sm text-slate-500">{s.location ?? ""}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
