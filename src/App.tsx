import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';

import ProductShowcase from './pages/ProductShowcase';
import AdminDashboard from './pages/AdminDashboard';
import BuyerDashboard from './pages/BuyerDashboard';
import SalesmanDashboard from './pages/SalesmanDashboard';
import LoginPage from './pages/LoginPage';
import SignupPage from './pages/SignupPage';
import ProductDetail from './pages/ProductDetail';
import OrderTracking from './pages/OrderTracking';
import Checkout from './pages/Checkout';
import CartPage from './pages/CartPage';
import WishlistPage from './pages/WishlistPage';
import ChangePassword from './pages/ChangePassword';
import AdminNotifications from './pages/AdminNotifications';
import MyComplaints from './pages/MyComplaints';

// Context
import { AuthProvider, useAuth } from './context/AuthContext';

// Components
import Navbar from './components/Navbar';
import AdminSettings from './pages/AdminSettings';
import Footer from './components/Footer';

// ... existing imports ...

// Add AdminSettings route

import UserGuideButton from './components/UserGuideButton';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  if (requiredRole && !requiredRole.includes(user?.role || '')) {
    return <Navigate to="/" />;
  }

  return <>{children}</>;
};

const AppRoutes: React.FC = () => {
  const { user, isAuthenticated } = useAuth();

  const getDashboardPath = () => {
    switch (user?.role) {
      case 'Admin': return '/admin/dashboard';
      case 'Salesman': return '/salesman/dashboard';
      case 'Buyer': return '/buyer/dashboard';
      default: return '/';
    }
  };

  return (
    <Routes>
      {/* Public Routes */}
        <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/product/:id" element={<ProductDetail />} />
      <Route path="/order-tracking" element={<OrderTracking />} />
      <Route path="/login" element={isAuthenticated ? <Navigate to={getDashboardPath()} replace /> : <LoginPage />} />
      <Route path="/signup" element={isAuthenticated ? <Navigate to={getDashboardPath()} replace /> : <SignupPage />} />

      {/* Protected Routes - Buyer */}
      <Route
        path="/buyer/dashboard"
        element={
          <ProtectedRoute requiredRole={['Buyer']}>
            <BuyerDashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/buyer/checkout"
        element={
          <ProtectedRoute requiredRole={['Buyer']}>
            <Checkout />
          </ProtectedRoute>
        }
      />
      <Route
        path="/cart"
        element={
          <ProtectedRoute requiredRole={['Buyer']}>
            <CartPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/wishlist"
        element={
          <ProtectedRoute requiredRole={['Buyer']}>
            <WishlistPage />
          </ProtectedRoute>
        }
      />

      {/* Protected Routes - Admin */}
      <Route
        path="/admin/dashboard"
        element={
          <ProtectedRoute requiredRole={['Admin']}>
            <AdminDashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/admin/notifications"
        element={
          <ProtectedRoute requiredRole={['Admin']}>
            <AdminNotifications />
          </ProtectedRoute>
        }
      />
      <Route
        path="/admin/settings"
        element={
          <ProtectedRoute requiredRole={['Admin']}>
            <AdminSettings />
          </ProtectedRoute>
        }
      />

      <Route
        path="/change-password"
        element={
          <ProtectedRoute>
            <ChangePassword />
          </ProtectedRoute>
        }
      />
      <Route
        path="/my-complaints"
        element={
          <ProtectedRoute requiredRole={['Buyer']}>
            <MyComplaints />
          </ProtectedRoute>
        }
      />

      {/* Protected Routes - Salesman */}
      <Route
        path="/salesman/dashboard"
        element={
          <ProtectedRoute requiredRole={['Salesman']}>
            <SalesmanDashboard />
          </ProtectedRoute>
        }
      />

      {/* Catch all */}
      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  );
};

function App() {
  return (
    <Router>
      <AuthProvider>
        <div className="page-container min-h-screen flex flex-col">
          <Navbar />
          <main className="flex-1">
            <AnimatePresence mode="wait">
              <AppRoutes />
            </AnimatePresence>
          </main>
          <Footer />
          <UserGuideButton />
        </div>
      </AuthProvider>
    </Router>
  );
}

export default App;
