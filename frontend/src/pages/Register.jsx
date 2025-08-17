import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import * as api from '../services/api.js';
import { useAuth } from '../contexts/AuthContext.jsx';

export default function Register() {
  const navigate = useNavigate();
  const { login: authLogin } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const [hint, setHint] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setHint(null);
    if (username.length < 3 || username.length > 30) {
      setHint('Username must be 3-30 characters.');
      return;
    }
    if (password.length < 6) {
      setHint('Password should be at least 6 characters.');
      return;
    }
    setLoading(true);
    try {
      const { token } = await api.register({ username, password });
      // If backend returns a token on register, auto-login; else go to login page
      if (token) {
        authLogin(token, username);
        navigate('/');
      } else {
        navigate('/login');
      }
    } catch (err) {
      setError(err.message || 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="row justify-content-center">
      <div className="col-12 col-sm-10 col-md-6 col-lg-5">
        <div className="card shadow-sm">
          <div className="card-body">
            <h2 className="card-title h4 mb-3">Register</h2>
            {error && <div className="alert alert-danger py-2">{error}</div>}
            {hint && !error && <div className="alert alert-warning py-2">{hint}</div>}
            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label">Username</label>
                <input className="form-control" value={username} onChange={(e) => setUsername(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">Password</label>
                <input type="password" className="form-control" value={password} onChange={(e) => setPassword(e.target.value)} required />
              </div>
              {/* Role is determined by backend; no role input here per OpenAPI */}
              <button className="btn btn-primary w-100" disabled={loading}>
                {loading ? 'Creating…' : 'Register'}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
