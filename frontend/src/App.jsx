import { Routes, Route, Navigate } from 'react-router-dom';
import NavBar from './components/NavBar.jsx';
import Welcome from './pages/Welcome.jsx';
import Login from './pages/Login.jsx';
import Register from './pages/Register.jsx';
import StudentDashboard from './pages/StudentDashboard.jsx';
import TeacherDashboard from './pages/TeacherDashboard.jsx';
import ProtectedRoute from './routes/ProtectedRoute.jsx';
import { useAuth } from './contexts/AuthContext.jsx';

export default function App() {
  const { isAuthenticated, role } = useAuth();

  return (
    <div>
      <NavBar />
      <div className="container gg-container mt-4">
        <Routes>
          <Route path="/" element={isAuthenticated ? (
            role === 'Admin' ? <Navigate to="/teacher" /> : <Navigate to="/student" />
          ) : (
            <Welcome />
          )} />

          <Route path="/login" element={isAuthenticated ? <Navigate to="/" /> : <Login />} />
          <Route path="/register" element={isAuthenticated ? <Navigate to="/" /> : <Register />} />

          <Route
            path="/student"
            element={
              <ProtectedRoute allowedRoles={["Student"]}>
                <StudentDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/teacher"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <TeacherDashboard />
              </ProtectedRoute>
            }
          />

          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </div>
    </div>
  );
}
