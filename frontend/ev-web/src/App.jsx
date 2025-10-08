// import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
// import LoginPage from "./pages/LoginPage";
// import ReservationsPage from "./pages/ReservationPage";
// import ProtectedRoute from "./components/ProtectedRoute";
// import { AuthProvider } from "./context/AuthContext";
// import RegisterPage from "./pages/RegisterPage";  

// function App() {
//   return (
//     <AuthProvider>
//       <Router>
//         <Routes>
//           <Route path="/login" element={<LoginPage />} />
//           <Route path="/register" element={<RegisterPage />} />
//           <Route
//             path="/reservations"
//             element={
//               <ProtectedRoute>
//                 <ReservationsPage />
//               </ProtectedRoute>
//             }
//           />
//           {/* Default redirect */}
//           <Route path="*" element={<LoginPage />} />
//         </Routes>
//       </Router>
//     </AuthProvider>
//   );
// }

// export default App;



import React, { useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Navbar from "./components/Navbar";
import Sidebar from "./components/Sidebar";

import LoginPage from "./pages/LoginPage";
import Dashboard from "./pages/Dashboard";
import SlotsPage from "./pages/SlotsPage";
import ReservationsPage from "./pages/ReservationPage";
import StationsPage from "./pages/StationsPage.jsx";

function AppLayout() {
  const [collapsed, setCollapsed] = useState(false);
  return (
    <div className="flex min-h-screen">
      <Sidebar collapsed={collapsed} />
      <div className="flex-1 flex flex-col">
        <Navbar onToggleSidebar={() => setCollapsed(!collapsed)} />
        <main className="p-4 flex-1 overflow-auto bg-slate-50">
          <Routes>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/slots" element={<SlotsPage />} />
            <Route path="/reservations" element={<ReservationsPage />} />
            <Route path="/stations" element={<StationsPage />} />
          </Routes>
        </main>
      </div>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/*"
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
