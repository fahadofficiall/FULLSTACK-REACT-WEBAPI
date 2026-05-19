import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="navbar">
      <Link to="/products" className="navbar__brand">
        📦 Products Manager
      </Link>
      <div className="navbar__right">
        <span className="navbar__user">
          Signed in as <span>{user?.username}</span>
          {user?.role === 'Admin' && (
            <span className="badge badge-blue" style={{ marginLeft: 8 }}>Admin</span>
          )}
        </span>
        <button className="btn btn-ghost" onClick={handleLogout}>
          Logout
        </button>
      </div>
    </nav>
  );
}
