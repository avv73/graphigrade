import { Routes, Route, Navigate } from 'react-router-dom';
import { Suspense, lazy } from 'react';
import NavBar from './components/NavBar.jsx';
import ProtectedRoute from './routes/ProtectedRoute.jsx';
import { useAuth } from './contexts/AuthContext.jsx';

const Welcome = lazy(() => import('./pages/Welcome.jsx'));
const Login = lazy(() => import('./pages/Login.jsx'));
const Register = lazy(() => import('./pages/Register.jsx'));
const StudentDashboard = lazy(() => import('./pages/StudentDashboard.jsx'));
const TeacherDashboard = lazy(() => import('./pages/TeacherDashboard.jsx'));

export default function App() {
  const { isAuthenticated, role } = useAuth();

  return (
    <div>
      <NavBar />
      <div className="container gg-container mt-4">
        <Suspense fallback={<div className="text-center my-4">Loading…</div>}>
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
        </Suspense>
      </div>
    </div>
  );
}
