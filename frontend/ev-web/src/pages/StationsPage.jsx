// import React, { useEffect, useState, useRef } from "react";
// import api from "../api/apiClient";
// import { Search, Trash2, Edit2, Save, XCircle } from "lucide-react";
// import { LoadScript, Autocomplete } from "@react-google-maps/api";

// export default function StationPage() {
//   const [stations, setStations] = useState([]);
//   const [loading, setLoading] = useState(true);
//   const [editingId, setEditingId] = useState(null);
//   const [form, setForm] = useState({
//     stationId: "",
//     uniqueIdentifier: "",
//     address: "",
//     latitude: "",
//     longitude: "",
//   });

//   const autocompleteRef = useRef(null);
//   const BASE_URL = "http://localhost:5074/api/Stations";

//   useEffect(() => {
//     fetchStations();
//   }, []);

//   async function fetchStations() {
//     try {
//       setLoading(true);
//       const res = await api.get(BASE_URL);
//       setStations(res.data);
//     } catch (error) {
//       console.error("Error fetching stations:", error);
//     } finally {
//       setLoading(false);
//     }
//   }

//   // Handle form input change
//   function handleChange(e) {
//     setForm({ ...form, [e.target.name]: e.target.value });
//   }

//   // Handle address autocomplete selection
//   function handlePlaceChanged() {
//     const place = autocompleteRef.current.getPlace();
//     if (place && place.geometry) {
//       const lat = place.geometry.location.lat();
//       const lng = place.geometry.location.lng();
//       const address = place.formatted_address;
//       setForm({
//         ...form,
//         address,
//         latitude: lat,
//         longitude: lng,
//       });
//     }
//   }

//   // Create or Update
//   async function handleSubmit(e) {
//     e.preventDefault();
//     try {
//       if (editingId) {
//         await api.put(`${BASE_URL}/${editingId}`, form);
//       } else {
//         await api.post(BASE_URL, form);
//       }
//       fetchStations();
//       setForm({
//         stationId: "",
//         uniqueIdentifier: "",
//         address: "",
//         latitude: "",
//         longitude: "",
//       });
//       setEditingId(null);
//     } catch (error) {
//       console.error("Error saving station:", error);
//     }
//   }

//   // Edit
//   function handleEdit(station) {
//     setEditingId(station.id);
//     setForm({
//       stationId: station.stationId,
//       uniqueIdentifier: station.uniqueIdentifier,
//       address: station.address,
//       latitude: station.latitude,
//       longitude: station.longitude,
//     });
//   }

//   // Delete
//   async function handleDelete(id) {
//     if (!window.confirm("Are you sure you want to delete this station?")) return;
//     try {
//       await api.delete(`${BASE_URL}/${id}`);
//       fetchStations();
//     } catch (error) {
//       console.error("Error deleting station:", error);
//     }
//   }

//   // Cancel Edit
//   function handleCancel() {
//     setEditingId(null);
//     setForm({
//       stationId: "",
//       uniqueIdentifier: "",
//       address: "",
//       latitude: "",
//       longitude: "",
//     });
//   }

//   return (
//     <div className="p-6">
//       <h1 className="text-3xl font-semibold mb-6 text-gray-800">
//         Station Management
//       </h1>

//       {/* 🚀 Add/Edit Form */}
//       <div className="bg-white shadow-lg rounded-xl p-5 mb-6 border border-gray-100">
//         <h2 className="text-xl font-semibold mb-4">
//           {editingId ? "Edit Station" : "Add New Station"}
//         </h2>

//         <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-2 gap-4">
//           <input
//             type="text"
//             name="stationId"
//             value={form.stationId}
//             onChange={handleChange}
//             placeholder="Station ID (e.g. Malabe_Station_01)"
//             className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
//             required
//           />
//           <input
//             type="text"
//             name="uniqueIdentifier"
//             value={form.uniqueIdentifier}
//             onChange={handleChange}
//             placeholder="Unique Identifier"
//             className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
//             required
//           />

//           {/* 🌍 Google Address Autocomplete */}
//           <LoadScript googleMapsApiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY} libraries={["places"]}>
//             <Autocomplete onLoad={(ac) => (autocompleteRef.current = ac)} onPlaceChanged={handlePlaceChanged}>
//               <input
//                 type="text"
//                 name="address"
//                 value={form.address}
//                 onChange={handleChange}
//                 placeholder="Search address..."
//                 className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
//                 required
//               />
//             </Autocomplete>
//           </LoadScript>

//           <input
//             type="text"
//             name="latitude"
//             value={form.latitude}
//             onChange={handleChange}
//             placeholder="Latitude"
//             className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
//             required
//           />
//           <input
//             type="text"
//             name="longitude"
//             value={form.longitude}
//             onChange={handleChange}
//             placeholder="Longitude"
//             className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
//             required
//           />

//           <div className="flex gap-3 col-span-2">
//             <button
//               type="submit"
//               className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition flex items-center gap-2"
//             >
//               <Save className="w-4 h-4" /> {editingId ? "Update" : "Save"}
//             </button>
//             {editingId && (
//               <button
//                 type="button"
//                 onClick={handleCancel}
//                 className="bg-gray-400 text-white px-4 py-2 rounded-lg hover:bg-gray-500 transition flex items-center gap-2"
//               >
//                 <XCircle className="w-4 h-4" /> Cancel
//               </button>
//             )}
//           </div>
//         </form>
//       </div>

//       {/* 📋 List of Stations */}
//       {loading ? (
//         <p className="text-gray-500">Loading stations...</p>
//       ) : stations.length === 0 ? (
//         <p className="text-gray-500">No stations found.</p>
//       ) : (
//         <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
//           {stations.map((st) => (
//             <div
//               key={st.id}
//               className="bg-white shadow-lg rounded-xl p-5 border border-gray-100 hover:shadow-xl transition"
//             >
//               <h3 className="text-lg font-semibold text-gray-800">{st.stationId}</h3>
//               <p className="text-sm text-gray-500">Unique ID: {st.uniqueIdentifier}</p>
//               <p className="text-sm text-gray-500">Address: {st.address}</p>
//               <p className="text-sm text-gray-500">
//                 Coordinates: {st.latitude}, {st.longitude}
//               </p>

//               <div className="flex gap-2 mt-3">
//                 <button
//                   onClick={() => handleEdit(st)}
//                   className="bg-green-500 text-white px-3 py-2 rounded-lg hover:bg-green-600 transition flex items-center gap-2"
//                 >
//                   <Edit2 className="w-4 h-4" /> Edit
//                 </button>
//                 <button
//                   onClick={() => handleDelete(st.id)}
//                   className="bg-red-500 text-white px-3 py-2 rounded-lg hover:bg-red-600 transition flex items-center gap-2"
//                 >
//                   <Trash2 className="w-4 h-4" /> Delete
//                 </button>
//               </div>
//             </div>
//           ))}
//         </div>
//       )}
//     </div>
//   );
// }



import React, { useEffect, useState, useRef } from "react";
import api from "../api/apiClient";
import { Search, Trash2, Edit2, Save, XCircle } from "lucide-react";
import { LoadScript, Autocomplete } from "@react-google-maps/api";

export default function StationPage() {
  const [stations, setStations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({
    stationId: "",
    address: "",
    latitude: "",
    longitude: "",
  });

  const autocompleteRef = useRef(null);
  const BASE_URL = "http://localhost:5074/api/Stations";

  useEffect(() => {
    fetchStations();
  }, []);

  async function fetchStations() {
    try {
      setLoading(true);
      const res = await api.get(BASE_URL);
      setStations(res.data);
    } catch (error) {
      console.error("Error fetching stations:", error);
    } finally {
      setLoading(false);
    }
  }

  // Handle form input change
  function handleChange(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  // Handle address autocomplete selection
  function handlePlaceChanged() {
    const place = autocompleteRef.current.getPlace();
    if (place && place.geometry) {
      const lat = place.geometry.location.lat();
      const lng = place.geometry.location.lng();
      const address = place.formatted_address;
      setForm({
        ...form,
        address,
        latitude: lat,
        longitude: lng,
      });
    }
  }

  // Create or Update
  async function handleSubmit(e) {
    e.preventDefault();
    try {
      if (editingId) {
        await api.put(`${BASE_URL}/${editingId}`, form);
      } else {
        await api.post(BASE_URL, form);
      }
      fetchStations();
      setForm({ stationId: "", address: "", latitude: "", longitude: "" });
      setEditingId(null);
    } catch (error) {
      console.error("Error saving station:", error);
    }
  }

  // Edit
  function handleEdit(station) {
    setEditingId(station.id);
    setForm({
      stationId: station.stationId,
      address: station.address,
      latitude: station.latitude,
      longitude: station.longitude,
    });
  }

  // Delete
  async function handleDelete(id) {
    if (!window.confirm("Are you sure you want to delete this station?")) return;
    try {
      await api.delete(`${BASE_URL}/${id}`);
      fetchStations();
    } catch (error) {
      console.error("Error deleting station:", error);
    }
  }

  // Cancel Edit
  function handleCancel() {
    setEditingId(null);
    setForm({ stationId: "", address: "", latitude: "", longitude: "" });
  }

  return (
    <div className="p-6">
      <h1 className="text-3xl font-semibold mb-6 text-gray-800">
        Station Management
      </h1>

      {/* Add/Edit Form */}
      <div className="bg-white shadow-lg rounded-xl p-5 mb-6 border border-gray-100">
        <h2 className="text-xl font-semibold mb-4">
          {editingId ? "Edit Station" : "Add New Station"}
        </h2>

        <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <input
            type="text"
            name="stationId"
            value={form.stationId}
            onChange={handleChange}
            placeholder="Station ID (e.g. Malabe_Station_01)"
            className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
            required
          />

          {/* Google Address Autocomplete */}
          <LoadScript googleMapsApiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY} libraries={["places"]}>
            <Autocomplete onLoad={(ac) => (autocompleteRef.current = ac)} onPlaceChanged={handlePlaceChanged}>
              <input
                type="text"
                name="address"
                value={form.address}
                onChange={handleChange}
                placeholder="Search address..."
                className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
                required
              />
            </Autocomplete>
          </LoadScript>

          <input
            type="text"
            name="latitude"
            value={form.latitude}
            onChange={handleChange}
            placeholder="Latitude"
            className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
            required
          />
          <input
            type="text"
            name="longitude"
            value={form.longitude}
            onChange={handleChange}
            placeholder="Longitude"
            className="border rounded-lg p-2 w-full focus:ring-2 focus:ring-blue-400 outline-none"
            required
          />

          <div className="flex gap-3 col-span-2">
            <button
              type="submit"
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition flex items-center gap-2"
            >
              <Save className="w-4 h-4" /> {editingId ? "Update" : "Save"}
            </button>
            {editingId && (
              <button
                type="button"
                onClick={handleCancel}
                className="bg-gray-400 text-white px-4 py-2 rounded-lg hover:bg-gray-500 transition flex items-center gap-2"
              >
                <XCircle className="w-4 h-4" /> Cancel
              </button>
            )}
          </div>
        </form>
      </div>

      {/* List of Stations */}
      {loading ? (
        <p className="text-gray-500">Loading stations...</p>
      ) : stations.length === 0 ? (
        <p className="text-gray-500">No stations found.</p>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {stations.map((st) => (
            <div
              key={st.id}
              className="bg-white shadow-lg rounded-xl p-5 border border-gray-100 hover:shadow-xl transition"
            >
              <h3 className="text-lg font-semibold text-gray-800">{st.stationId}</h3>
              <p className="text-sm text-gray-500">Address: {st.address}</p>
              <p className="text-sm text-gray-500">
                Coordinates: {st.latitude}, {st.longitude}
              </p>

              <div className="flex gap-2 mt-3">
                <button
                  onClick={() => handleEdit(st)}
                  className="bg-green-500 text-white px-3 py-2 rounded-lg hover:bg-green-600 transition flex items-center gap-2"
                >
                  <Edit2 className="w-4 h-4" /> Edit
                </button>
                <button
                  onClick={() => handleDelete(st.id)}
                  className="bg-red-500 text-white px-3 py-2 rounded-lg hover:bg-red-600 transition flex items-center gap-2"
                >
                  <Trash2 className="w-4 h-4" /> Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
