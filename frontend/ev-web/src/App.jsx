import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import ReservationsPage from "./pages/ReservationPage";
import ProtectedRoute from "./components/ProtectedRoute";
import { AuthProvider } from "./context/AuthContext";
import RegisterPage from "./pages/RegisterPage";  

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/reservations"
            element={
              <ProtectedRoute>
                <ReservationsPage />
              </ProtectedRoute>
            }
          />
          {/* Default redirect */}
          <Route path="*" element={<LoginPage />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
