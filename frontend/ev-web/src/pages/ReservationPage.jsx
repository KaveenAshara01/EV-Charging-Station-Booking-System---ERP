import { useEffect, useState } from "react";
import axios from "axios";
import { useAuth } from "../context/AuthContext";

export default function ReservationsPage() {
  const { user, logout } = useAuth();
  const [reservations, setReservations] = useState([]);
  const [stationId, setStationId] = useState("");
  const [slotId, setSlotId] = useState("");
  const [time, setTime] = useState("");

  useEffect(() => {
    fetchReservations();
  }, []);

  const fetchReservations = async () => {
    try {
      const res = await axios.get("http://localhost:5074/api/reservations/me", {
        headers: { Authorization: `Bearer ${user.token}` },
      });
      setReservations(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      await axios.post(
        "http://localhost:5074/api/reservations",
        {
          stationId,
          slotId,
          reservationTimeUtc: time,
        },
        {
          headers: { Authorization: `Bearer ${user.token}` },
        }
      );
      fetchReservations();
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="flex justify-between mb-6">
        <h1 className="text-2xl font-bold">My Reservations</h1>
        <button
          onClick={logout}
          className="bg-red-500 text-white px-4 py-2 rounded-lg"
        >
          Logout
        </button>
      </div>

      <form
        onSubmit={handleCreate}
        className="bg-white p-6 rounded-lg shadow mb-8"
      >
        <h2 className="text-lg font-semibold mb-4">Create Reservation</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <input
            type="text"
            placeholder="Station ID"
            value={stationId}
            onChange={(e) => setStationId(e.target.value)}
            className="p-3 border rounded-lg"
          />
          <input
            type="text"
            placeholder="Slot ID"
            value={slotId}
            onChange={(e) => setSlotId(e.target.value)}
            className="p-3 border rounded-lg"
          />
          <input
            type="datetime-local"
            value={time}
            onChange={(e) => setTime(e.target.value)}
            className="p-3 border rounded-lg"
          />
        </div>
        <button
          type="submit"
          className="mt-4 bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700"
        >
          Reserve
        </button>
      </form>

      <div className="grid gap-4">
        {reservations.map((r) => (
          <div
            key={r.reservationId}
            className="p-4 bg-white rounded-lg shadow flex justify-between"
          >
            <div>
              <p><b>Station:</b> {r.stationId}</p>
              <p><b>Slot:</b> {r.slotId}</p>
              <p><b>Time:</b> {new Date(r.reservationTimeUtc).toLocaleString()}</p>
              <p><b>Status:</b> {r.status}</p>
            </div>
            {r.qrCodeData && (
              <img
                src={`data:image/png;base64,${r.qrCodeData}`}
                alt="QR Code"
                className="w-20 h-20"
              />
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
