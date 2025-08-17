import { Link, NavLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext.jsx';

export default function NavBar() {
  const { isAuthenticated, role, username, logout } = useAuth();

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container gg-container">
        <Link className="navbar-brand" to="/">Graphigrade</Link>
        <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#nav" aria-controls="nav" aria-expanded="false" aria-label="Toggle navigation">
          <span className="navbar-toggler-icon"></span>
        </button>
        <div className="collapse navbar-collapse" id="nav">
          {isAuthenticated && (
            <ul className="navbar-nav me-auto">
              {role === 'Admin' ? (
                <li className="nav-item"><NavLink className="nav-link" to="/teacher">Teacher</NavLink></li>
              ) : (
                <li className="nav-item"><NavLink className="nav-link" to="/student">Student</NavLink></li>
              )}
            </ul>
          )}

          <ul className="navbar-nav ms-auto">
            {!isAuthenticated ? (
              <>
                <li className="nav-item"><NavLink className="nav-link" to="/login">Login</NavLink></li>
                <li className="nav-item"><NavLink className="nav-link" to="/register">Register</NavLink></li>
              </>
            ) : (
              <>
                <li className="nav-item"><span className="navbar-text me-3">Hello, {username}</span></li>
                <li className="nav-item"><button className="btn btn-outline-light btn-sm" onClick={logout}>Logout</button></li>
              </>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
}
