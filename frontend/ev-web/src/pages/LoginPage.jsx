// import { useState } from "react";
// import axios from "axios";
// import { useAuth } from "../context/AuthContext";
// import { useNavigate } from "react-router-dom";

// export default function LoginPage() {
//   const { login } = useAuth();
//   const navigate = useNavigate();
//   const [email, setEmail] = useState("");
//   const [password, setPassword] = useState("");
//   const [error, setError] = useState("");

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     try {
//       const res = await axios.post("http://localhost:5074/api/users/login", {
//         email,
//         password,
//       });
//       login(res.data.token, res.data.role); 
//       navigate("/reservations");
//     } catch (err) {
//       setError("Invalid credentials");
//     }
//   };

//   return (
//     <div className="flex min-h-screen items-center justify-center bg-gray-100">
//       <div className="bg-white p-8 rounded-2xl shadow-lg w-96">
//         <h1 className="text-2xl font-bold mb-6 text-center">EV Charger Login</h1>
//         {error && <p className="text-red-500 mb-3">{error}</p>}
//         <form onSubmit={handleSubmit} className="space-y-4">
//           <input
//             type="email"
//             placeholder="Email"
//             value={email}
//             onChange={(e) => setEmail(e.target.value)}
//             className="w-full p-3 border rounded-lg focus:ring focus:ring-blue-400"
//           />
//           <input
//             type="password"
//             placeholder="Password"
//             value={password}
//             onChange={(e) => setPassword(e.target.value)}
//             className="w-full p-3 border rounded-lg focus:ring focus:ring-blue-400"
//           />
//           <button
//             type="submit"
//             className="w-full bg-blue-600 text-white py-3 rounded-lg hover:bg-blue-700"
//           >
//             Login
//           </button>
//         </form>
//             <p className="text-center mt-4 text-sm">
//     Don't have an account?{" "}
//     <a href="/register" className="text-blue-600 hover:underline">
//         Register
//     </a>
// </p>
//       </div>
//     </div>
//   );
// }


import React, { useState } from "react";
import api from "../api/apiClient";
import { useAuth } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState("");

  const handle = async (e) => {
    e.preventDefault();
    setErr("");
    try {
      // NOTE: backend login endpoint should return { token, role } (or token+user)
      const res = await api.post("/api/users/login", { email, password });
      const token = res.data.token ?? res.data.tokenString ?? res.data.tokenString;
      const role = res.data.role ?? (res.data.user && res.data.user.role) ?? "BACKOFFICE";
      if (!token) {
        setErr("Server did not return token. Check backend response format.");
        return;
      }
      login(token, role);
      navigate("/dashboard");
    } catch (error) {
      setErr("Invalid credentials or server error.");
      console.error(error);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="w-full max-w-md bg-white p-8 rounded-2xl shadow">
        <h2 className="text-2xl font-bold mb-4">BackOffice Login</h2>
        {err && <div className="text-red-500 mb-3">{err}</div>}
        <form onSubmit={handle} className="space-y-4">
          <input
            className="w-full p-3 border rounded"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            type="email"
            required
          />
          <input
            className="w-full p-3 border rounded"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            required
          />
          <button className="w-full bg-blue-600 text-white p-3 rounded">Login</button>
        </form>
      </div>
    </div>
  );
}
