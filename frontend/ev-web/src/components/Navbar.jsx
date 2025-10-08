import React from "react";
import { useAuth } from "../context/AuthContext";
import { FiMenu } from "react-icons/fi";

export default function Navbar({ onToggleSidebar }) {
  const { logout, user } = useAuth();

  return (
    <header className="bg-white shadow px-4 py-3 flex items-center justify-between">
      <div className="flex items-center gap-3">
        <button onClick={onToggleSidebar} className="p-2 rounded-md hover:bg-slate-100">
          <FiMenu size={20} />
        </button>
        <h1 className="text-lg font-semibold">EV Charging — BackOffice</h1>
      </div>

      <div className="flex items-center gap-4">
        <div className="text-sm text-slate-600">Role: <span className="font-medium">{user?.role}</span></div>
        <button
          onClick={logout}
          className="bg-red-600 text-white px-3 py-1 rounded-md hover:bg-red-700"
        >
          Logout
        </button>
      </div>
    </header>
  );
}
