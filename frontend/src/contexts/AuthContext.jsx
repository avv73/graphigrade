import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { jwtDecode } from 'jwt-decode';

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

const AuthContext = createContext(null);

function parseToken(token) {
  try {
    const payload = jwtDecode(token);
    const role = payload[ROLE_CLAIM] || payload.role || (Array.isArray(payload.roles) ? payload.roles[0] : undefined) || 'Student';
    const username = payload[NAMEID_CLAIM] || payload.username || payload.sub || payload.name || 'user';
    const exp = payload.exp ? payload.exp * 1000 : null;
    return { role, username, exp };
  } catch {
    return { role: 'Student', username: 'user', exp: null };
  }
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('gg_token') || null);
  const [role, setRole] = useState('Student');
  const [username, setUsername] = useState(null);

  useEffect(() => {
    if (token) {
      localStorage.setItem('gg_token', token);
      const { role, username, exp } = parseToken(token);
      setRole(role);
      setUsername(username);
      if (exp && Date.now() > exp) {
        // token expired
        logout();
      }
    } else {
      localStorage.removeItem('gg_token');
      setRole('Student');
      setUsername(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const login = (newToken, overrideUsername) => {
    setToken(newToken);
    if (overrideUsername) setUsername(overrideUsername);
  };
  const logout = () => setToken(null);

  const value = useMemo(() => ({
    token,
    role,
    username,
    isAuthenticated: Boolean(token),
    login,
    logout,
  }), [token, role, username]);

  return (
    <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
