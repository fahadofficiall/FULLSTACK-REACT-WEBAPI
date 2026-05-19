import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import PrivateRoute from './components/PrivateRoute';
import Navbar      from './components/Navbar';
import LoginPage   from './pages/LoginPage';
import ProductsPage from './pages/ProductsPage';

// Layout wrapper for authenticated pages
function AuthLayout() {
  return (
    <div className="app-layout">
      <Navbar />
      <Outlet />
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected — wrapped in Navbar layout */}
          <Route element={<PrivateRoute><AuthLayout /></PrivateRoute>}>
            <Route path="/products" element={<ProductsPage />} />
          </Route>

          {/* Default redirect */}
          <Route path="*" element={<Navigate to="/products" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
