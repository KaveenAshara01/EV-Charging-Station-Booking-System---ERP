// import React, { useEffect, useState } from "react";
// import axios from "axios";
// import { Card, CardContent } from "@/components/ui/card";
// import { Input } from "@/components/ui/input";
// import { Button } from "@/components/ui/button";

// export default function ReservationPage() {
//   const [reservations, setReservations] = useState([]);
//   const [filteredReservations, setFilteredReservations] = useState([]);
//   const [searchTerm, setSearchTerm] = useState("");

//   // ✅ Fetch all reservations on mount
//   useEffect(() => {
//     const fetchReservations = async () => {
//       try {
//         const res = await axios.get("http://localhost:5074/api/Reservations");
//         const reservationsData = res.data;

//         // Fetch slot details for each reservation
//         const reservationsWithSlots = await Promise.all(
//           reservationsData.map(async (reservation) => {
//             try {
//               const slotRes = await axios.get(
//                 `http://localhost:5074/api/Slots/${reservation.slotId}`
//               );
//               const slotData = slotRes.data;
//               return { ...reservation, slot: slotData };
//             } catch (err) {
//               console.error("Error fetching slot:", err);
//               return { ...reservation, slot: null };
//             }
//           })
//         );

//         setReservations(reservationsWithSlots);
//         setFilteredReservations(reservationsWithSlots);
//       } catch (error) {
//         console.error("Error fetching reservations:", error);
//       }
//     };

//     fetchReservations();
//   }, []);

//   // ✅ Filter by station ID
//   const handleSearch = (e) => {
//     e.preventDefault();
//     const filtered = reservations.filter((res) =>
//       res.stationId.toString().includes(searchTerm.trim())
//     );
//     setFilteredReservations(filtered);
//   };

//   // ✅ Approve reservation
//   const handleApprove = async (id) => {
//     try {
//       await axios.put(`http://localhost:5074/api/Reservations/approve/${id}`);
//       setReservations((prev) =>
//         prev.map((r) => (r.id === id ? { ...r, status: "Approved" } : r))
//       );
//     } catch (error) {
//       console.error("Error approving reservation:", error);
//     }
//   };

//   // ✅ Cancel reservation
//   const handleCancel = async (id) => {
//     try {
//       await axios.delete(`http://localhost:5074/api/Reservations/${id}`);
//       setReservations((prev) => prev.filter((r) => r.id !== id));
//     } catch (error) {
//       console.error("Error cancelling reservation:", error);
//     }
//   };

//   return (
//     <div className="p-8 bg-gray-50 min-h-screen">
//       <h1 className="text-3xl font-bold text-gray-800 mb-6">Reservations</h1>

//       {/* Search Bar */}
//       <form onSubmit={handleSearch} className="flex gap-2 mb-6">
//         <Input
//           type="text"
//           placeholder="Search by Station ID"
//           value={searchTerm}
//           onChange={(e) => setSearchTerm(e.target.value)}
//           className="w-64"
//         />
//         <Button type="submit">Search</Button>
//       </form>

//       {/* Reservation Cards */}
//       <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
//         {filteredReservations.map((res) => (
//           <Card key={res.id} className="shadow-md border border-gray-200">
//             <CardContent className="p-4">
//               <h2 className="text-lg font-semibold text-gray-700">
//                 Reservation #{res.id}
//               </h2>
//               <p className="text-gray-500 text-sm">Station ID: {res.stationId}</p>
//               <p className="text-gray-500 text-sm">Vehicle ID: {res.vehicleId}</p>
//               <p className="text-gray-500 text-sm">Status: {res.status}</p>

//               {/* ✅ Display Slot Times */}
//               {res.slot ? (
//                 <div className="mt-2 text-gray-600 text-sm">
//                   <p>
//                     <span className="font-semibold">Start:</span>{" "}
//                     {new Date(res.slot.startUtc).toLocaleString()}
//                   </p>
//                   <p>
//                     <span className="font-semibold">End:</span>{" "}
//                     {new Date(res.slot.endUtc).toLocaleString()}
//                   </p>
//                 </div>
//               ) : (
//                 <p className="text-sm text-gray-400 mt-2">Slot details unavailable</p>
//               )}

//               <div className="mt-4 flex gap-2">
//                 {res.status !== "Approved" && (
//                   <Button
//                     onClick={() => handleApprove(res.id)}
//                     className="bg-green-600 hover:bg-green-700 text-white"
//                   >
//                     Approve
//                   </Button>
//                 )}
//                 <Button
//                   onClick={() => handleCancel(res.id)}
//                   className="bg-red-500 hover:bg-red-600 text-white"
//                 >
//                   Cancel
//                 </Button>
//               </div>
//             </CardContent>
//           </Card>
//         ))}
//       </div>
//     </div>
//   );
// }




import React, { useEffect, useState } from "react";
import { CheckCircle, XCircle, Search } from "lucide-react";
import api from "../api/apiClient";

export default function ReservationsPage() {
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState("");
  const [filteredList, setFilteredList] = useState([]);

  const BASE_URL = "http://localhost:5074/api/reservations";
  const SLOT_URL = "http://localhost:5074/api/slots";

  useEffect(() => {
    fetchReservations();
  }, []);

  // ✅ Fetch reservations and slot details
  async function fetchReservations() {
    try {
      setLoading(true);
      const res = await api.get(BASE_URL);
      const reservationsData = res.data;

      // Fetch slot details for each reservation
      const reservationsWithSlots = await Promise.all(
        reservationsData.map(async (r) => {
          try {
            const slotRes = await api.get(`${SLOT_URL}/${r.slotId}`);
            return { ...r, slot: slotRes.data };
          } catch (err) {
            console.error(`Error fetching slot ${r.slotId}:`, err);
            return { ...r, slot: null };
          }
        })
      );

      setReservations(reservationsWithSlots);
      setFilteredList(reservationsWithSlots);
    } catch (error) {
      console.error("Error fetching reservations:", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleApprove(id) {
    try {
      await api.post(`${BASE_URL}/${id}/approve`);
      fetchReservations();
    } catch (error) {
      console.error("Error approving reservation:", error);
    }
  }

  async function handleCancel(id) {
    try {
      await api.delete(`${BASE_URL}/${id}`);
      fetchReservations();
    } catch (error) {
      console.error("Error canceling reservation:", error);
    }
  }

  function handleSearch() {
    if (!filter.trim()) {
      setFilteredList(reservations);
    } else {
      setFilteredList(
        reservations.filter((r) =>
          r.stationId.toLowerCase().includes(filter.toLowerCase())
        )
      );
    }
  }

  return (
    <div className="p-6 bg-gray-50 min-h-screen">
      <h1 className="text-3xl font-semibold mb-6 text-gray-800">
        Reservations Management
      </h1>

      {/* 🔍 Search bar */}
      <div className="flex items-center gap-2 mb-6">
        <input
          type="text"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          placeholder="Search by Station ID..."
          className="border rounded-lg p-2 w-1/3 focus:ring-2 focus:ring-blue-400 outline-none"
        />
        <button
          onClick={handleSearch}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-700 transition"
        >
          <Search className="w-4 h-4" /> Search
        </button>
      </div>

      {loading ? (
        <p className="text-gray-500">Loading reservations...</p>
      ) : filteredList.length === 0 ? (
        <p className="text-gray-500">No reservations found.</p>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {filteredList.map((res) => (
            <div
              key={res.reservationId}
              className="bg-white shadow-lg rounded-xl p-5 border border-gray-100 hover:shadow-xl transition"
            >
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="text-lg font-semibold text-gray-800">
                    Reservation #{res.reservationId.slice(0, 8)}...
                  </h3>
                  <p className="text-sm text-gray-500">Station: {res.stationId}</p>
                  <p className="text-sm text-gray-500">Vehicle: {res.vehicleId}</p>

                  {/* ✅ Display slot start/end time */}
                  {res.slot ? (
                    <>
                      <p className="text-sm text-gray-500">
                        Start:{" "}
                        {new Date(res.slot.startUtc).toLocaleString()}
                      </p>
                      <p className="text-sm text-gray-500">
                        End:{" "}
                        {new Date(res.slot.endUtc).toLocaleString()}
                      </p>
                    </>
                  ) : (
                    <p className="text-sm text-gray-400">
                      Slot details unavailable
                    </p>
                  )}

                  <p className="mt-1">
                    <span
                      className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        res.status === "Approved"
                          ? "bg-green-100 text-green-700"
                          : res.status === "Pending"
                          ? "bg-yellow-100 text-yellow-700"
                          : "bg-red-100 text-red-700"
                      }`}
                    >
                      {res.status}
                    </span>
                  </p>
                </div>

                {/* ✅ Show Approve + Cancel only for Pending */}
                {res.status === "Pending" && (
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleApprove(res.reservationId)}
                      className="bg-green-500 text-white px-3 py-2 rounded-lg hover:bg-green-600 transition"
                      title="Approve Reservation"
                    >
                      <CheckCircle className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleCancel(res.reservationId)}
                      className="bg-red-500 text-white px-3 py-2 rounded-lg hover:bg-red-600 transition"
                      title="Cancel Reservation"
                    >
                      <XCircle className="w-4 h-4" />
                    </button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
