import React from "react";
import { NavLink } from "react-router-dom";
import { FiGrid, FiClock, FiSliders, FiMap } from "react-icons/fi";

export default function Sidebar({ collapsed }) {
  const linkClass = ({ isActive }) =>
    "flex items-center gap-3 px-4 py-2 rounded-md " + (isActive ? "bg-slate-200" : "hover:bg-slate-100");

  return (
    <aside className={`bg-white border-r p-4 ${collapsed ? "w-16" : "w-64"} min-h-screen`}>
      <nav className="space-y-1">
        <NavLink to="/dashboard" className={linkClass}>
          <FiGrid /> {!collapsed && <span>Dashboard</span>}
        </NavLink>
        <NavLink to="/slots" className={linkClass}>
          <FiSliders /> {!collapsed && <span>Manage Slots</span>}
        </NavLink>
        <NavLink to="/reservations" className={linkClass}>
          <FiClock /> {!collapsed && <span>Reservations</span>}
        </NavLink>
        <NavLink to="/stations" className={linkClass}>
          <FiMap /> {!collapsed && <span>Stations</span>}
        </NavLink>
      </nav>
    </aside>
  );
}
