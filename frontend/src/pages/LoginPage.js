import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { authApi } from '../api/api';
import { useAuth } from '../context/AuthContext';

// ── Tab styles ────────────────────────────────────────────────────────────────
const tabStyle = (active) => ({
  flex: 1,
  padding: '10px',
  border: 'none',
  borderBottom: active ? '2px solid #2563eb' : '2px solid transparent',
  background: 'none',
  fontWeight: active ? 600 : 400,
  color: active ? '#2563eb' : '#6b7280',
  cursor: 'pointer',
  fontSize: 14,
  transition: 'all 150ms ease',
});

// ── Login Form ────────────────────────────────────────────────────────────────
function LoginForm({ onSuccess }) {
  const [form, setForm]       = useState({ username: '', password: '' });
  const [error, setError]     = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();

  const set = (field) => (e) => {
    setForm((f) => ({ ...f, [field]: e.target.value }));
    setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.username.trim() || !form.password) {
      setError('Username and password are required.');
      return;
    }
    setLoading(true);
    try {
      const res = await authApi.login({ username: form.username.trim(), password: form.password });
      const { token, username, email, role, expires } = res.data;
      login(token, { username, email, role, expires });
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error">{error}</div>}

      <div className="form-group">
        <label className="form-label">Username</label>
        <input
          className="form-control"
          type="text"
          value={form.username}
          onChange={set('username')}
          placeholder="Enter your username"
          autoComplete="username"
          autoFocus
        />
      </div>

      <div className="form-group">
        <label className="form-label">Password</label>
        <input
          className="form-control"
          type="password"
          value={form.password}
          onChange={set('password')}
          placeholder="Enter your password"
          autoComplete="current-password"
        />
      </div>

      <button type="submit" className="btn btn-primary login-card__submit" disabled={loading}>
        {loading ? <><span className="spinner" /> Signing in…</> : 'Sign In'}
      </button>
    </form>
  );
}

// ── Register Form ─────────────────────────────────────────────────────────────
function RegisterForm({ onRegistered }) {
  const [form, setForm]         = useState({ username: '', email: '', password: '', confirm: '' });
  const [error, setError]       = useState('');
  const [success, setSuccess]   = useState('');
  const [loading, setLoading]   = useState(false);

  const set = (field) => (e) => {
    setForm((f) => ({ ...f, [field]: e.target.value }));
    setError('');
    setSuccess('');
  };

  const validate = () => {
    if (!form.username.trim())          return 'Username is required.';
    if (form.username.trim().length < 3) return 'Username must be at least 3 characters.';
    if (!form.email.trim())             return 'Email is required.';
    if (!/\S+@\S+\.\S+/.test(form.email)) return 'Enter a valid email address.';
    if (!form.password)                 return 'Password is required.';
    if (form.password.length < 6)       return 'Password must be at least 6 characters.';
    if (form.password !== form.confirm) return 'Passwords do not match.';
    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const err = validate();
    if (err) { setError(err); return; }

    setLoading(true);
    try {
      await authApi.register({
        username: form.username.trim(),
        email:    form.email.trim().toLowerCase(),
        password: form.password,
      });
      setSuccess('Account created! You can now sign in.');
      setForm({ username: '', email: '', password: '', confirm: '' });
      setTimeout(() => onRegistered(), 1500);
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error   && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="form-group">
        <label className="form-label">Username</label>
        <input
          className="form-control"
          type="text"
          value={form.username}
          onChange={set('username')}
          placeholder="Choose a username"
          autoComplete="username"
          autoFocus
        />
      </div>

      <div className="form-group">
        <label className="form-label">Email</label>
        <input
          className="form-control"
          type="email"
          value={form.email}
          onChange={set('email')}
          placeholder="you@example.com"
          autoComplete="email"
        />
      </div>

      <div className="form-group">
        <label className="form-label">Password</label>
        <input
          className="form-control"
          type="password"
          value={form.password}
          onChange={set('password')}
          placeholder="At least 6 characters"
          autoComplete="new-password"
        />
      </div>

      <div className="form-group">
        <label className="form-label">Confirm Password</label>
        <input
          className="form-control"
          type="password"
          value={form.confirm}
          onChange={set('confirm')}
          placeholder="Repeat your password"
          autoComplete="new-password"
        />
      </div>

      <button type="submit" className="btn btn-primary login-card__submit" disabled={loading}>
        {loading ? <><span className="spinner" /> Creating account…</> : 'Create Account'}
      </button>
    </form>
  );
}

// ── Main Page ─────────────────────────────────────────────────────────────────
export default function LoginPage() {
  const [tab, setTab] = useState('login'); // 'login' | 'register'
  const navigate      = useNavigate();
  const location      = useLocation();
  const from          = location.state?.from?.pathname || '/products';

  return (
    <div className="login-wrap">
      <div className="login-card" style={{ maxWidth: 420 }}>

        {/* Logo */}
        <div className="login-card__logo">
          <h1>📦 Products Manager</h1>
          <p>{tab === 'login' ? 'Sign in to your account' : 'Create a new account'}</p>
        </div>

        {/* Tabs */}
        <div style={{ display: 'flex', borderBottom: '1px solid #e5e7eb', marginBottom: 24 }}>
          <button style={tabStyle(tab === 'login')}    onClick={() => setTab('login')}>Sign In</button>
          <button style={tabStyle(tab === 'register')} onClick={() => setTab('register')}>Register</button>
        </div>

        {/* Form */}
        {tab === 'login'
          ? <LoginForm    onSuccess={() => navigate(from, { replace: true })} />
          : <RegisterForm onRegistered={() => setTab('login')} />
        }

      </div>
    </div>
  );
}