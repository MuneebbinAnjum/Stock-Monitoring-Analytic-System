import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import { Layout, ShoppingCart, Package, LayoutGrid, Users, AlertCircle, Settings, Activity } from 'lucide-react';
import { OrderApi } from '../api/order.api';
import { EmployeeApi } from '../api/employee.api';
import { ProductApi } from '../api/product.api';
import { CategoryApi } from '../api/category.api';

// Subcomponents
import AdminOverview from '../components/admin/AdminOverview';
import AdminOrders from '../components/admin/AdminOrders';
import AdminProducts from '../components/admin/AdminProducts';
import AdminCategories from '../components/admin/AdminCategories';
import AdminEmployees from '../components/admin/AdminEmployees';
import AdminComplaints from '../components/admin/AdminComplaints';
import AdminSettings from '../components/admin/AdminSettings';
import AdminAuditLogs from '../components/admin/AdminAuditLogs';

const tabs = [
  { id: 'overview', label: 'Overview', icon: Layout },
  { id: 'orders', label: 'Orders', icon: ShoppingCart },
  { id: 'products', label: 'Products', icon: Package },
  { id: 'categories', label: 'Categories', icon: LayoutGrid },
  { id: 'employees', label: 'Employees', icon: Users },
  { id: 'complaints', label: 'Complaints', icon: AlertCircle },
  { id: 'settings', label: 'Settings', icon: Settings },
  { id: 'audit', label: 'Audit Logs', icon: Activity },
];

const AdminDashboard: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('overview');
  
  // Data state
  const [orders, setOrders] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [allEmployees, setAllEmployees] = useState<any[]>([]);
  const [pendingSalesman, setPendingSalesman] = useState<any[]>([]);
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      const [ordersData, productsData, categoriesData] = await Promise.all([
        OrderApi.getAll().catch(() => []),
        ProductApi.getAll().catch(() => []),
        CategoryApi.getAll().catch(() => [])
      ]);
      setOrders(ordersData || []);
      setProducts(productsData || []);
      setCategories(categoriesData || []);

      loadEmployees();
    } catch (err: any) {
      setError(err.message || 'Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  const loadEmployees = async () => {
    try {
      const pendingData = await EmployeeApi.getPending().catch(() => []);
      setPendingSalesman(pendingData || []);
      const all = await EmployeeApi.getAll().catch(() => []);
      setAllEmployees(all || []);
    } catch { }
  };

  const loadProducts = async () => {
    const data = await ProductApi.getAll().catch(() => []);
    setProducts(data || []);
  };

  const loadOrders = async () => {
    const data = await OrderApi.getAll().catch(() => []);
    setOrders(data || []);
  };

  const loadCategories = async () => {
    const data = await CategoryApi.getAll().catch(() => []);
    setCategories(data || []);
  };

  useEffect(() => {
    loadData();

    const handleNotification = (e: any) => {
      const type = e.detail?.notificationType;
      // Refresh relevant data based on notification
      if (type === 'NewOrder' || type === 'OrderApproved' || type === 'OrderRejected' || type === 'OrderStatusChanged') {
        loadOrders();
      }
      if (type === 'SalesmanRegistered') {
        loadEmployees();
      }
      // Product changes can trigger global re-fetch if we introduce a ProductUpdated notification
      // Complaints are handled inside AdminComplaints, but we can do a global refresh if needed
    };

    const handleInventory = (e: any) => {
      // payload: { ProductId, NewQuantity }
      loadProducts();
    };

    window.addEventListener('NotificationReceived', handleNotification);
    window.addEventListener('InventoryUpdated', handleInventory);
    window.addEventListener('StockAlertCreated', handleInventory);
    return () => {
      window.removeEventListener('NotificationReceived', handleNotification);
      window.removeEventListener('InventoryUpdated', handleInventory);
      window.removeEventListener('StockAlertCreated', handleInventory);
    };
  }, []);

  if (loading) {
    return (
      <div className="page-container min-h-screen flex items-center justify-center">
        <motion.div
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
          className="w-12 h-12 border-4 border-primary-200 border-t-primary-600 rounded-full"
        />
      </div>
    );
  }

  return (
    <div className="page-container min-h-screen py-8">
      <div className="max-w-7xl mx-auto px-4">
        
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <h1 className="text-3xl font-bold text-gray-900 mb-1">Admin Control Panel</h1>
          <p className="text-gray-500">Welcome back, {user?.fullName}. Manage your store efficiently.</p>
        </motion.div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">{error}</div>
        )}

        {/* Tab Navigation */}
        <div className="flex overflow-x-auto pb-4 mb-6 gap-2 no-scrollbar border-b border-gray-200">
          {tabs.map(tab => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center space-x-2 px-4 py-2.5 rounded-xl font-medium text-sm transition-all whitespace-nowrap ${
                  isActive 
                    ? 'bg-primary-600 text-white shadow-md shadow-primary-500/20' 
                    : 'bg-white text-gray-600 hover:bg-gray-50 border border-gray-200'
                }`}
              >
                <Icon className={`w-4 h-4 ${isActive ? 'text-primary-100' : 'text-gray-400'}`} />
                <span>{tab.label}</span>
              </button>
            )
          })}
        </div>

        {/* Tab Content */}
        <motion.div
          key={activeTab}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.2 }}
        >
          {activeTab === 'overview' && <AdminOverview orders={orders} pendingSalesman={pendingSalesman} products={products} />}
          {activeTab === 'orders' && <AdminOrders orders={orders} onOrderUpdated={loadOrders} />}
          {activeTab === 'products' && <AdminProducts products={products} categories={categories} onProductsUpdated={loadProducts} />}
          {activeTab === 'categories' && <AdminCategories categories={categories} onCategoriesUpdated={loadCategories} />}
          {activeTab === 'employees' && <AdminEmployees pendingSalesman={pendingSalesman} allEmployees={allEmployees} onEmployeesUpdated={loadEmployees} />}
          {activeTab === 'complaints' && <AdminComplaints />}
          {activeTab === 'settings' && <AdminSettings />}
          {activeTab === 'audit' && <AdminAuditLogs />}
        </motion.div>

      </div>
    </div>
  );
};

export default AdminDashboard;
