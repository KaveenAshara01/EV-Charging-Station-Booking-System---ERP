import React, { useEffect, useState } from "react";
import api from "../api/apiClient";

export default function SlotsPage() {
  const [stationId, setStationId] = useState("");
  const [startUtc, setStartUtc] = useState("");
  const [endUtc, setEndUtc] = useState("");
  const [slots, setSlots] = useState([]);
  const [message, setMessage] = useState("");

  const fetchSlots = async () => {
    //if (!stationId) return;
    try {
      console.log("Fetching slots ");
      // returns slots in window — for simplicity we'll fetch a week window
      //const from = new Date().toISOString();
      //const to = new Date(Date.now() + 7 * 24 * 3600 * 1000).toISOString();
      const res = await api.get("/api/Slots");

      // const res = await api.get(`/api/slots/station/${encodeURIComponent(stationId)}?fromUtc=${encodeURIComponent(from)}&toUtc=${encodeURIComponent(to)}`);
      console.log(res);
      setSlots(res.data);
    } catch (err) {
      console.error(err);
    }
  };

   useEffect(() => {
    fetchSlots();
  }, []);
  // useEffect(() => {
  //   if (stationId) fetchSlots();
  // }, [stationId]);

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      await api.post("/api/slots", {
        stationId,
        startUtc: new Date(startUtc).toISOString(),
        endUtc: new Date(endUtc).toISOString(),
        label: `Slot ${stationId} ${startUtc}`
      });
      setMessage("Slot created");
      fetchSlots();
    } catch (err) {
      console.error(err);
      setMessage("Create failed: " + (err.response?.data?.error ?? err.message));
    }
  };

  const handleDelete = async (slotId) => {
    if (!confirm("Delete slot?")) return;
    try {
      await api.delete(`/api/slots/${slotId}`);
      setMessage("Slot deleted");
      fetchSlots();
    } catch (err) {
      setMessage("Delete failed: " + (err.response?.data?.error ?? err.message));
    }
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Manage Slots</h2>

      <div className="mb-4">
        <span className="font-bold">Station ID:</span> {stationId || <span className="text-slate-500">Not selected</span>}
      </div>

      <div className="bg-white p-4 rounded shadow mb-6">
        <form onSubmit={handleCreate} className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <input placeholder="Station ID" value={stationId} onChange={e => setStationId(e.target.value)} className="p-2 border rounded" required />
          <input type="datetime-local" value={startUtc} onChange={e => setStartUtc(e.target.value)} className="p-2 border rounded" required />
          <input type="datetime-local" value={endUtc} onChange={e => setEndUtc(e.target.value)} className="p-2 border rounded" required />
          <button className="bg-green-600 text-white px-4 rounded">Create Slot</button>
        </form>
        {message && <div className="text-sm mt-2 text-slate-600">{message}</div>}
      </div>

      <div className="grid gap-3">
        {slots.length === 0 && <div className="text-slate-500">No slots found for station.</div>}
        {slots.map(s => (
          <div key={s.slotId} className="bg-white p-3 rounded shadow flex justify-between items-center">
            <div>
              <div className="font-medium">{s.label ?? s.slotId}</div>
              <div className="font-medium">{s.stationId}</div>
              <div className="text-sm text-slate-500">{new Date(s.startUtc).toLocaleString()} - {new Date(s.endUtc).toLocaleString()}</div>
              <div className="text-sm">Available: {s.isAvailable ? "Yes" : "No"}</div>
            </div>
            <div className="flex gap-2">
              <button className="px-3 py-1 bg-red-500 text-white rounded" onClick={() => handleDelete(s.slotId)}>Delete</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
