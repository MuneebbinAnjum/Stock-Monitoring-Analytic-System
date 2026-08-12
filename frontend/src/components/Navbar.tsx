import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { motion, AnimatePresence } from 'framer-motion';
import { LogOut, Menu, X, Bell, Info as HelpCircle } from 'lucide-react';

import api from '../api/client';
import { NotificationApi } from '../api/notification.api';
import { startConnection, getConnection, createConnection } from '../lib/signalr';
import ChangePasswordModal from './ChangePasswordModal';
import UserGuideModal from './UserGuideModal';

interface NotifItem {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  notificationType: string;
}

const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const [isOpen, setIsOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);
  const [notifications, setNotifications] = useState<NotifItem[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [showChangeModal, setShowChangeModal] = useState(false);
  const [showGuideModal, setShowGuideModal] = useState(false);
  const notifRef = useRef<HTMLDivElement>(null);





  // Load notification count on mount and every 30s when authenticated
  useEffect(() => {
    if (!isAuthenticated) return;
    const load = async () => {
      try {
        const [countRes, notifRes] = await Promise.all([
          NotificationApi.getCount(),
          NotificationApi.getAll(false, 50)
        ]);
        let items = (notifRes || []) as NotifItem[];
        // Hide admin-only notifications (SalesmanRegistered) from non-admin users
        if (user?.role !== 'Admin') {
          items = items.filter(i => i.notificationType !== 'SalesmanRegistered');
        }
        setNotifications(items.slice(0, 10));
        // compute unread count from returned notifications (best-effort)
        const unread = items.filter(i => !i.isRead).length;
        setUnreadCount(unread || (countRes || 0));
      } catch { }
    };
    load();
    const interval = setInterval(load, 30000);
    // Start SignalR connection for realtime notifications
    (async () => {
      try {
        const conn = await startConnection();
        if (user?.id) {
          try { await conn.invoke('JoinGroup', user.id); } catch { }
        }
        conn.on('NotificationCreated', (payload: any) => {
          // push new notification if visible to this user
          if (user?.role !== 'Admin' && payload.notificationType === 'SalesmanRegistered') return;
          setNotifications(prev => [payload, ...prev].slice(0, 50));
          setUnreadCount(c => c + 1);
          window.dispatchEvent(new CustomEvent('NotificationReceived', { detail: payload }));
        });

        // Complaint messages (from admin or buyer replies)
        conn.on('ComplaintMessage', (payload: any) => {
          setNotifications(prev => [{ id: payload.id || 'cm-' + Math.random().toString(36).substr(2, 5), title: 'New Message', message: payload.message, isRead: false, createdAt: payload.createdAt, notificationType: 'ComplaintMessage' }, ...prev].slice(0, 50));
          setUnreadCount(c => c + 1);
          window.dispatchEvent(new CustomEvent('NotificationReceived', { detail: payload }));
        });

        // Inventory and stock events — dispatch both notification and specific events
        conn.on('InventoryUpdated', (payload: any) => {
          try { window.dispatchEvent(new CustomEvent('InventoryUpdated', { detail: payload })); } catch { }
          try { window.dispatchEvent(new CustomEvent('NotificationReceived', { detail: payload })); } catch { }
        });

        conn.on('StockAlertCreated', (payload: any) => {
          try { window.dispatchEvent(new CustomEvent('StockAlertCreated', { detail: payload })); } catch { }
          try { window.dispatchEvent(new CustomEvent('NotificationReceived', { detail: payload })); } catch { }
        });

        conn.on('StockAlertResolved', (payload: any) => {
          try { window.dispatchEvent(new CustomEvent('StockAlertResolved', { detail: payload })); } catch { }
          try { window.dispatchEvent(new CustomEvent('NotificationReceived', { detail: payload })); } catch { }
        });
      } catch (ex) { /* ignore */ }
    })();
    return () => clearInterval(interval);
  }, [isAuthenticated]);

  // Close notification panel when clicking outside
  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (notifRef.current && !notifRef.current.contains(e.target as Node)) {
        setNotifOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const markAllRead = async () => {
    try {
      await NotificationApi.markAllRead();
      setUnreadCount(0);
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    } catch { }
  };

  const markOneRead = async (id: string) => {
    try {
      await NotificationApi.markOneRead(id);
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch { }
  };

  const handleLogout = () => {
    logout();
    navigate('/');
    setIsOpen(false);
  };

  const getDashboardLink = () => {
    if (!isAuthenticated) return null;
    switch (user?.role) {
      case 'Admin': return '/admin/dashboard';
      case 'Salesman': return '/salesman/dashboard';
      case 'Buyer': return '/buyer/dashboard';
      default: return null;
    }
  };

  return (
    <>
    <nav className="sticky top-0 z-50 border-b" style={{
      background: 'rgba(255, 255, 255, 0.6)',
      backdropFilter: 'blur(24px) saturate(180%)',
      WebkitBackdropFilter: 'blur(24px) saturate(180%)',
      borderColor: 'rgba(255, 255, 255, 0.4)',
      boxShadow: '0 1px 12px rgba(0, 0, 0, 0.04)',
    }}>
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <motion.div
            whileHover={{ scale: 1.03 }}
            className="flex items-center space-x-3 cursor-pointer"
            onClick={() => navigate('/')}
          >
            <div className="w-9 h-9 rounded-xl flex items-center justify-center" style={{
              background: 'linear-gradient(135deg, #0284c7 0%, #7c3aed 100%)',
              boxShadow: '0 4px 12px rgba(2, 132, 199, 0.3)',
            }}>
              <span className="text-white font-bold text-sm tracking-wider">S</span>
            </div>
            <span className="text-lg font-bold text-gray-900 hidden sm:inline tracking-tight">SMAS</span>
          </motion.div>

          {/* Desktop Menu */}
          <div className="hidden md:flex items-center space-x-1">
            <Link to="/" className="px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 rounded-lg hover:bg-gray-100/60 transition-all duration-200">
              Products
            </Link>
            <Link to="/order-tracking" className="px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 rounded-lg hover:bg-gray-100/60 transition-all duration-200">
              Track Order
            </Link>

            {(user?.role === 'Buyer') && (
              <div className="flex items-center space-x-1 ml-2 border-l border-gray-200 pl-2">
                <Link to="/wishlist" className="p-2 text-gray-500 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors" title="Wishlist">
                  <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z" /></svg>
                </Link>
                {user?.role === 'Buyer' && (
                  <Link to="/cart" className="p-2 text-gray-500 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors" title="Cart">
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4Z" /><path d="M3 6h18" /><path d="M16 10a4 4 0 0 1-8 0" /></svg>
                  </Link>
                )}
              </div>
            )}

            {!isAuthenticated ? (
              <div className="flex items-center space-x-2 ml-4">
                <motion.button
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  onClick={() => navigate('/login')}
                  className="px-4 py-2 text-sm font-semibold text-primary-600 border border-primary-200 rounded-xl hover:bg-primary-50 transition-all duration-200"
                >
                  Login
                </motion.button>
                <motion.button
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  onClick={() => navigate('/signup')}
                  className="btn-primary text-sm py-2"
                >
                  Sign Up
                </motion.button>
              </div>
            ) : (
              <div className="flex items-center space-x-3 ml-4">
                {getDashboardLink() && (
                  <motion.button
                    whileHover={{ scale: 1.03 }}
                    onClick={() => navigate(getDashboardLink()!)}
                    className="px-4 py-2 text-sm font-semibold text-primary-600 bg-primary-50/80 rounded-xl hover:bg-primary-100 transition-all duration-200"
                  >
                    Dashboard
                  </motion.button>
                )}
                {user?.role === 'Buyer' && (
                  <motion.button
                    whileHover={{ scale: 1.03 }}
                    onClick={() => navigate('/my-complaints')}
                    className="px-4 py-2 text-sm font-semibold text-gray-700 bg-white/80 rounded-xl hover:bg-gray-50 transition-all duration-200"
                  >
                    My Complaints
                  </motion.button>
                )}

                {/* Notification Bell */}
                <div ref={notifRef} className="relative">
                  <button
                    onClick={() => setNotifOpen(p => !p)}
                    className="relative p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100/60 rounded-xl transition-colors"
                    title="Notifications"
                  >
                    <Bell className="w-5 h-5" />
                    {unreadCount > 0 && (
                      <motion.span
                        initial={{ scale: 0 }}
                        animate={{ scale: 1 }}
                        className="absolute -top-0.5 -right-0.5 w-4.5 h-4.5 min-w-[1.1rem] text-[10px] font-bold bg-red-500 text-white rounded-full flex items-center justify-center px-0.5"
                      >
                        {unreadCount > 99 ? '99+' : unreadCount}
                      </motion.span>
                    )}
                  </button>

                  <AnimatePresence>
                    {notifOpen && (
                      <motion.div
                        initial={{ opacity: 0, y: -8, scale: 0.95 }}
                        animate={{ opacity: 1, y: 0, scale: 1 }}
                        exit={{ opacity: 0, y: -8, scale: 0.95 }}
                        transition={{ duration: 0.15 }}
                        className="absolute right-0 top-full mt-2 w-80 bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden z-50"
                      >
                        <div className="flex justify-between items-center px-4 py-3 border-b border-gray-100 bg-gray-50">
                          <span className="font-semibold text-gray-900 text-sm">Notifications</span>
                          {unreadCount > 0 && (
                            <button onClick={markAllRead} className="text-xs text-primary-600 font-medium hover:underline">
                              Mark all read
                            </button>
                          )}
                        </div>
                        <div className="max-h-80 overflow-y-auto">
                          {notifications.length === 0 ? (
                            <p className="text-center text-gray-400 text-sm py-6">No notifications</p>
                          ) : notifications.map(n => (
                            <button
                              key={n.id}
                              onClick={() => markOneRead(n.id)}
                              className={`w-full text-left px-4 py-3 border-b border-gray-50 hover:bg-gray-50 transition-colors ${!n.isRead ? 'bg-blue-50/50' : ''}`}
                            >
                              <div className="flex items-start gap-2">
                                {!n.isRead && <div className="w-2 h-2 rounded-full bg-blue-500 mt-1.5 flex-shrink-0" />}
                                <div className={!n.isRead ? '' : 'ml-4'}>
                                  <p className={`text-sm font-semibold ${!n.isRead ? 'text-gray-900' : 'text-gray-600'}`}>{n.title}</p>
                                  <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{n.message}</p>
                                  <p className="text-xs text-gray-400 mt-1">{new Date(n.createdAt).toLocaleDateString()}</p>
                                </div>
                              </div>
                            </button>
                          ))}
                        </div>
                      </motion.div>
                    )}
                  </AnimatePresence>
                </div>

                <div className="flex items-center space-x-2 px-3 py-1.5 bg-gray-50/80 rounded-xl">
                  <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-primary-500 to-violet-500 flex items-center justify-center">
                    <span className="text-white text-xs font-bold">{user?.fullName?.charAt(0) || 'U'}</span>
                  </div>
                  <div className="hidden lg:block">
                    <p className="text-sm font-medium text-gray-700">{user?.fullName}</p>
                    <p className="text-xs text-gray-400">{user?.role}</p>
                  </div>
                </div>

                <motion.button
                  whileHover={{ scale: 1.1 }}
                  whileTap={{ scale: 0.95 }}
                  onClick={handleLogout}
                  className="p-2 hover:bg-red-50 rounded-xl transition-colors"
                  title="Logout"
                >
                  <LogOut className="w-4 h-4 text-red-500" />
                </motion.button>
                <button onClick={() => setShowGuideModal(true)} className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100/60 rounded-xl transition-colors" title="User Guide">
                  <HelpCircle className="w-5 h-5" />
    </button>
    {/* Admin Settings button */}
    <button onClick={() => navigate('/admin/settings')} className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100/60 rounded-xl transition-colors" title="Settings">
      Settings
    </button>
    <button onClick={() => setShowChangeModal(true)} className="ml-2 text-sm text-gray-600 hover:underline">
      Change Password
    </button>
    </div>
            )}
          </div>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden p-2 rounded-xl hover:bg-gray-100/60 transition-colors"
            onClick={() => setIsOpen(!isOpen)}
          >
            {isOpen ? <X className="w-5 h-5 text-gray-700" /> : <Menu className="w-5 h-5 text-gray-700" />}
          </button>
        </div>

        {/* Mobile Menu */}
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className="md:hidden py-4 space-y-1 border-t border-gray-100/50"
          >
            <Link to="/" className="block px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-100/60 rounded-xl transition-colors" onClick={() => setIsOpen(false)}>
              Products
            </Link>
            <Link to="/order-tracking" className="block px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-100/60 rounded-xl transition-colors" onClick={() => setIsOpen(false)}>
              Track Order
            </Link>

            {!isAuthenticated ? (
              <div className="pt-2 space-y-2">
                <button onClick={() => { navigate('/login'); setIsOpen(false); }} className="block w-full text-left px-4 py-2.5 text-sm font-semibold text-primary-600 hover:bg-primary-50 rounded-xl transition-colors">
                  Login
                </button>
                <button onClick={() => { navigate('/signup'); setIsOpen(false); }} className="block w-full text-left px-4 py-2.5 btn-primary text-sm">
                  Sign Up
                </button>
              </div>
            ) : (
              <div className="pt-2 space-y-1">
                {getDashboardLink() && (
                  <button onClick={() => { navigate(getDashboardLink()!); setIsOpen(false); }} className="block w-full text-left px-4 py-2.5 text-sm font-semibold text-primary-600 hover:bg-primary-50 rounded-xl transition-colors">
                    Dashboard {unreadCount > 0 && <span className="ml-2 bg-red-500 text-white text-xs px-1.5 py-0.5 rounded-full">{unreadCount}</span>}
                  </button>
                )}
                <button onClick={handleLogout} className="block w-full text-left px-4 py-2.5 text-sm font-semibold text-red-600 hover:bg-red-50 rounded-xl transition-colors">
                  Logout
                </button>
              </div>
            )}
          </motion.div>
        )}
      </div>
    </nav>
    <ChangePasswordModal open={showChangeModal} onClose={() => setShowChangeModal(false)} />
    <UserGuideModal isOpen={showGuideModal} onClose={() => setShowGuideModal(false)} />
    </>
  );
};

export default Navbar;
