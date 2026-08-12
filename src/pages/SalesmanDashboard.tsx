import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import { ShoppingCart, TrendingUp, Users, Zap, Package, DollarSign, Plus, Trash2, CheckCircle, RefreshCw } from 'lucide-react';
import { OrderApi } from '../api/order.api';
import { ProductApi } from '../api/product.api';
import { CommissionApi } from '../api/commission.api';
import { CustomerApi } from '../api/customer.api';
import { OrderResponse } from '../types';

const SalesmanDashboard: React.FC = () => {
  const { user } = useAuth();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [commissionMap, setCommissionMap] = useState<Record<string, number>>({});

  // Cart & Order Form State
  const [cart, setCart] = useState<{ productId: string; name: string; price: number; quantity: number }[]>([]);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [selectedCustomerId, setSelectedCustomerId] = useState('');
  const [isNewCustomer, setIsNewCustomer] = useState(false);
  const [newCustomerForm, setNewCustomerForm] = useState({
    fullName: '',
    email: '',
    phone: '',
    city: 'Karachi',
    province: 'Sindh'
  });

  // Modal / Receipt state
  const [showReceipt, setShowReceipt] = useState<any>(null);

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      const [myOrders, allProducts, allCustomers] = await Promise.all([
        OrderApi.getMySalesmanOrders(),   // secure: only returns this salesman's orders
        ProductApi.getAll(),
        CustomerApi.getAll(),
      ]);

      setOrders(myOrders || []);
      setProducts(allProducts || []);
      setCustomers(allCustomers || []);

      // Load per-product commission configuration for this salesman
      try {
        if (user?.id) {
          const comms = await CommissionApi.getByEmployee(user.id).catch(() => []);
          // comms is expected to be an array of { productId, commissionPercentage }
          const map: Record<string, number> = {};
          (comms || []).forEach((c: any) => {
            if (c && c.productId) map[c.productId] = Number(c.commissionPercentage) || 0;
          });
          setCommissionMap(map);
        }
      } catch { /* ignore commission fetch errors */ }

      // Pre-select Walk-in Customer if present
      const walkin = (allCustomers || []).find((c: any) => c.email === 'walkin@smas.com');
      if (walkin) {
        setSelectedCustomerId(walkin.id);
      } else if (allCustomers && allCustomers.length > 0) {
        setSelectedCustomerId(allCustomers[0].id);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user) loadData();
    // live refresh every 30 seconds
    const id = setInterval(() => { if (user) loadData(); }, 30000);

    const handleNotification = (e: any) => {
      // Reload on inventory updates or new orders
      loadData();
    };
    window.addEventListener('NotificationReceived', handleNotification);

    return () => {
      clearInterval(id);
      window.removeEventListener('NotificationReceived', handleNotification);
    };
  }, [user]);

  const totalRevenue = orders.reduce((sum, o) => sum + o.totalAmount, 0);
  const completedOrders = orders.filter(o => o.status === 'Received' || o.status === 'Delivered').length;
  const pendingOrders = orders.filter(o => o.status === 'Pending').length;
  // Calculate commission using per-product commissions where available, fallback to 5%
  const commissionEarned = orders.reduce((sum, o) => {
    const items = o.items || [];
    return sum + items.reduce((s: number, item: any) => {
      const pct = commissionMap[item.productId] !== undefined ? commissionMap[item.productId] : 5;
      const line = (item.quantity * (item.unitPrice || 0)) * (pct / 100);
      return s + line;
    }, 0);
  }, 0);
  
  const targetOrders = 50;
  const progressPercent = Math.min((completedOrders / targetOrders) * 100, 100);

  const stats = [
    { icon: ShoppingCart, label: 'Total Sales', value: orders.length.toString(), iconBg: 'bg-blue-500/10 text-blue-500' },
    { icon: DollarSign, label: 'Total Revenue', value: `Rs. ${totalRevenue.toLocaleString()}`, iconBg: 'bg-emerald-500/10 text-emerald-500' },
    { icon: TrendingUp, label: 'Commission (5%)', value: `Rs. ${commissionEarned.toLocaleString()}`, iconBg: 'bg-purple-500/10 text-purple-500' },
    { icon: Zap, label: 'Pending Orders', value: pendingOrders.toString(), iconBg: 'bg-amber-500/10 text-amber-500' },
  ];

  // Cart actions
  const handleAddToCart = () => {
    if (!selectedProductId) return;
    const product = products.find(p => p.id === selectedProductId);
    if (!product) return;

    if (quantity > product.stockQuantity) {
      setError(`Cannot add ${quantity} units. Only ${product.stockQuantity} items in stock.`);
      return;
    }

    const existingCartItem = cart.find(item => item.productId === selectedProductId);
    if (existingCartItem) {
      const newQty = existingCartItem.quantity + quantity;
      if (newQty > product.stockQuantity) {
        setError(`Cannot update quantity to ${newQty}. Only ${product.stockQuantity} items in stock.`);
        return;
      }
      setCart(cart.map(item => item.productId === selectedProductId ? { ...item, quantity: newQty } : item));
    } else {
      setCart([...cart, {
        productId: product.id,
        name: product.name,
        price: product.discountPrice || product.unitPrice,
        quantity: quantity
      }]);
    }
    setError('');
    setQuantity(1);
  };

  const handleRemoveFromCart = (index: number) => {
    setCart(cart.filter((_, i) => i !== index));
  };

  const handleLogOrder = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');

    if (cart.length === 0) {
      setError('Please add at least one product to the cart.');
      return;
    }

    try {
      let customerId = selectedCustomerId;

      // 1. Create Customer if registering on the fly
      if (isNewCustomer) {
        if (!newCustomerForm.fullName || !newCustomerForm.email) {
          setError('Please fill in new customer details.');
          return;
        }
        const newCust = await CustomerApi.create(newCustomerForm);
        customerId = newCust.id;
        // Refresh customer list
        const updatedCustomers = await CustomerApi.getAll();
        setCustomers(updatedCustomers);
      }

      // 2. Submit Physical Order
      const payload = {
        customerId,
        employeeId: user?.id,
        items: cart.map(item => ({ productId: item.productId, quantity: item.quantity })),
        orderType: 'Physical',
        deliveryCity: 'In-Store Pick-up',
        deliveryAddress: 'In-Store pick up completed',
        paymentMethod: 'Cash',
        deliveryPeriod: 'Immediate'
      };

      const result = await OrderApi.create(payload);

      // Success
      setSuccessMsg(`Physical order successfully logged: ${result.orderNumber}`);
      setShowReceipt(result);
      setCart([]);
      setIsNewCustomer(false);
      setNewCustomerForm({ fullName: '', email: '', phone: '', city: 'Karachi', province: 'Sindh' });

      // Refresh stats/history
      const updatedOrders = await OrderApi.getAll();
      const myOrders = (updatedOrders || []).filter((o: OrderResponse) => o.employeeId === user?.id);
      setOrders(myOrders);

      // Refresh product stock
      const updatedProducts = await ProductApi.getAll();
      setProducts(updatedProducts);

    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to log physical order. Check stock availability.');
    }
  };

  const selectedProduct = products.find(p => p.id === selectedProductId);

  return (
    <div className="page-container min-h-screen py-8 relative">
      <div className="max-w-7xl mx-auto px-4">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col md:flex-row md:items-center md:justify-between mb-8"
        >
          <div>
            <h1 className="text-3xl font-extrabold text-gray-900 mb-1 tracking-tight">Salesperson Panel</h1>
            <p className="text-gray-500">Welcome, {user?.fullName}. Register walked-in orders and track your sales targets.</p>
          </div>
          <button
            onClick={loadData}
            className="mt-4 md:mt-0 flex items-center space-x-2 bg-white text-gray-700 border border-gray-200 px-4 py-2 rounded-xl shadow-sm hover:bg-gray-50 transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
            <span>Refresh Data</span>
          </button>
        </motion.div>

        {error && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
            {error}
          </motion.div>
        )}

        {successMsg && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="mb-6 p-4 bg-emerald-50 border border-emerald-200 rounded-xl text-emerald-700 text-sm flex items-center space-x-2">
            <CheckCircle className="w-5 h-5 text-emerald-500" />
            <span>{successMsg}</span>
          </motion.div>
        )}

        {/* Stats */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
          {stats.map((stat, index) => {
            const Icon = stat.icon;
            return (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.08 }}
                className="card group"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-gray-500 text-sm font-medium">{stat.label}</p>
                    <p className="text-3xl font-extrabold text-gray-900 mt-1 tracking-tight">{stat.value}</p>
                  </div>
                  <div className={`p-3 rounded-xl ${stat.iconBg} transition-transform group-hover:scale-110`}>
                    <Icon className="w-6 h-6" />
                  </div>
                </div>
              </motion.div>
            );
          })}
        </div>

        {/* Target Progress */}
        <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="card mb-8">
          <div className="flex justify-between items-end mb-2">
            <div>
              <h3 className="font-bold text-gray-900">Monthly Sales Target</h3>
              <p className="text-sm text-gray-500">Reach {targetOrders} completed orders to earn a bonus.</p>
            </div>
            <span className="font-bold text-primary-600">{completedOrders} / {targetOrders}</span>
          </div>
          <div className="w-full bg-gray-100 rounded-full h-3">
            <div className="bg-primary-500 h-3 rounded-full transition-all duration-1000" style={{ width: `${progressPercent}%` }}></div>
          </div>
        </motion.div>

        {/* Main section: Left: Log physical order form, Right: History */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
          
          {/* Left Panel: In-store order creation */}
          <div className="lg:col-span-7">
            <motion.div
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              className="card h-full"
            >
              <h2 className="text-xl font-bold text-gray-900 mb-6 flex items-center space-x-2">
                <ShoppingCart className="w-6 h-6 text-primary-500" />
                <span>Log Walk-in Physical Order</span>
              </h2>

              <form onSubmit={handleLogOrder} className="space-y-6">
                
                {/* 1. Customer Section */}
                <div className="bg-gray-50/50 p-4 rounded-2xl border border-gray-100">
                  <div className="flex justify-between items-center mb-4">
                    <label className="block text-sm font-semibold text-gray-800">1. Customer Information</label>
                    <button
                      type="button"
                      onClick={() => setIsNewCustomer(!isNewCustomer)}
                      className="text-xs text-primary-600 hover:text-primary-700 font-semibold"
                    >
                      {isNewCustomer ? "Select Existing Customer" : "Register New Walk-in Buyer"}
                    </button>
                  </div>

                  {!isNewCustomer ? (
                    <div>
                      <select
                        value={selectedCustomerId}
                        onChange={(e) => setSelectedCustomerId(e.target.value)}
                        className="input-field"
                      >
                        {customers.map((c) => (
                          <option key={c.id} value={c.id}>
                            {c.fullName} ({c.email})
                          </option>
                        ))}
                      </select>
                    </div>
                  ) : (
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <div>
                        <input
                          type="text"
                          placeholder="Full Name"
                          value={newCustomerForm.fullName}
                          onChange={(e) => setNewCustomerForm({ ...newCustomerForm, fullName: e.target.value })}
                          className="input-field"
                        />
                      </div>
                      <div>
                        <input
                          type="email"
                          placeholder="Email Address"
                          value={newCustomerForm.email}
                          onChange={(e) => setNewCustomerForm({ ...newCustomerForm, email: e.target.value })}
                          className="input-field"
                        />
                      </div>
                      <div>
                        <input
                          type="text"
                          placeholder="Phone Number"
                          value={newCustomerForm.phone}
                          onChange={(e) => setNewCustomerForm({ ...newCustomerForm, phone: e.target.value })}
                          className="input-field"
                        />
                      </div>
                      <div className="grid grid-cols-2 gap-2">
                        <input
                          type="text"
                          placeholder="City"
                          value={newCustomerForm.city}
                          onChange={(e) => setNewCustomerForm({ ...newCustomerForm, city: e.target.value })}
                          className="input-field"
                        />
                        <input
                          type="text"
                          placeholder="Province"
                          value={newCustomerForm.province}
                          onChange={(e) => setNewCustomerForm({ ...newCustomerForm, province: e.target.value })}
                          className="input-field"
                        />
                      </div>
                    </div>
                  )}
                </div>

                {/* 2. Product and Quantity selection */}
                <div className="bg-gray-50/50 p-4 rounded-2xl border border-gray-100">
                  <label className="block text-sm font-semibold text-gray-800 mb-3">2. Add Products</label>
                  
                  <div className="grid grid-cols-1 sm:grid-cols-12 gap-3 items-end">
                    <div className="sm:col-span-6">
                      <select
                        value={selectedProductId}
                        onChange={(e) => setSelectedProductId(e.target.value)}
                        className="input-field"
                      >
                        <option value="">-- Select Product --</option>
                        {products.map((p) => (
                          <option key={p.id} value={p.id}>
                            {p.name} - SKU: {p.sku} (Stock: {p.stockQuantity})
                          </option>
                        ))}
                      </select>
                    </div>

                    <div className="sm:col-span-3">
                      <input
                        type="number"
                        min="1"
                        placeholder="Qty"
                        value={quantity}
                        onChange={(e) => setQuantity(parseInt(e.target.value) || 1)}
                        className="input-field text-center"
                      />
                    </div>

                    <div className="sm:col-span-3">
                      <button
                        type="button"
                        onClick={handleAddToCart}
                        disabled={!selectedProductId}
                        className="btn-primary w-full flex items-center justify-center space-x-1 py-3"
                      >
                        <Plus className="w-4 h-4" />
                        <span>Add</span>
                      </button>
                    </div>
                  </div>

                  {selectedProduct && (
                    <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="mt-3 text-sm text-gray-600 flex justify-between bg-white p-3 rounded-xl border border-gray-100">
                      <span>Brand: <strong>{selectedProduct.brandName || 'N/A'}</strong></span>
                      <span>Price: <strong>Rs. {(selectedProduct.discountPrice || selectedProduct.unitPrice).toLocaleString()}</strong></span>
                      <span>Available Stock: <strong className={selectedProduct.stockQuantity <= selectedProduct.reorderLevel ? 'text-red-500 font-bold' : 'text-green-600'}>{selectedProduct.stockQuantity}</strong></span>
                    </motion.div>
                  )}
                </div>

                {/* 3. Cart Summary */}
                <div className="bg-gray-50/50 p-4 rounded-2xl border border-gray-100">
                  <label className="block text-sm font-semibold text-gray-800 mb-3">3. Cart Receipts Summary</label>

                  {cart.length === 0 ? (
                    <p className="text-gray-400 text-sm py-4 text-center">Cart is empty. Add products above.</p>
                  ) : (
                    <div className="space-y-3">
                      {cart.map((item, index) => (
                        <div key={index} className="flex justify-between items-center p-3 bg-white border border-gray-100 rounded-xl shadow-xs">
                          <div>
                            <p className="font-semibold text-gray-900 text-sm">{item.name}</p>
                            <p className="text-xs text-gray-500">
                              Rs. {item.price.toLocaleString()} x {item.quantity}
                            </p>
                          </div>
                          <div className="flex items-center space-x-3">
                            <span className="font-bold text-gray-900 text-sm">
                              Rs. {(item.price * item.quantity).toLocaleString()}
                            </span>
                            <button
                              type="button"
                              onClick={() => handleRemoveFromCart(index)}
                              className="text-red-500 hover:text-red-700 p-1"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>
                        </div>
                      ))}

                      <div className="flex justify-between items-center pt-3 border-t border-gray-200 mt-2">
                        <span className="font-bold text-gray-800">Total Bill Amount:</span>
                        <span className="text-xl font-extrabold text-primary-600">
                          Rs. {cart.reduce((sum, item) => sum + item.price * item.quantity, 0).toLocaleString()}
                        </span>
                      </div>
                    </div>
                  )}
                </div>

                <motion.button
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  type="submit"
                  disabled={cart.length === 0}
                  className="btn-primary w-full py-4 text-base font-bold shadow-md shadow-primary-500/10 flex items-center justify-center space-x-2"
                >
                  <CheckCircle className="w-5 h-5" />
                  <span>Log Physical Sale & Print Receipt</span>
                </motion.button>

              </form>
            </motion.div>
          </div>

          {/* Right Panel: History */}
          <div className="lg:col-span-5 flex flex-col gap-6">
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              className="card flex-1"
            >
              <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center space-x-2">
                <TrendingUp className="w-5 h-5 text-primary-500" />
                <span>Your Sales History</span>
              </h2>

              <div className="space-y-3 max-h-[500px] overflow-y-auto pr-1">
                {orders.length === 0 ? (
                  <p className="text-gray-400 text-sm py-8 text-center">No sales logged yet.</p>
                ) : (
                  orders.map((order) => (
                    <motion.div
                      key={order.id}
                      layout
                      className="p-4 bg-white border border-gray-100 hover:border-gray-200 rounded-xl transition-all"
                    >
                      <div className="flex justify-between items-start mb-2">
                        <div>
                          <p className="font-semibold text-gray-900 text-sm">{order.orderNumber}</p>
                          <p className="text-xs text-gray-500">Customer: {order.customerName}</p>
                        </div>
                        <p className="font-bold text-gray-900 text-sm">Rs. {order.totalAmount.toLocaleString()}</p>
                      </div>
                      <div className="flex justify-between items-center">
                        <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold ${
                          order.status === 'Received' || order.status === 'Delivered' ? 'bg-emerald-100 text-emerald-700' :
                          'bg-amber-100 text-amber-700'
                        }`}>
                          {order.status}
                        </span>
                        <p className="text-[10px] text-gray-400">{new Date(order.orderDate).toLocaleDateString()}</p>
                      </div>
                    </motion.div>
                  ))
                )}
              </div>
            </motion.div>
          </div>

        </div>

      </div>

      {/* 4. Beautiful Receipt Modal Popup */}
      <AnimatePresence>
        {showReceipt && (
          <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50">
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-white max-w-md w-full rounded-3xl p-6 shadow-2xl relative border border-gray-100"
            >
              {/* Receipt Design */}
              <div className="text-center pb-6 border-b border-dashed border-gray-200">
                <div className="w-12 h-12 bg-emerald-100 text-emerald-600 rounded-full flex items-center justify-center mx-auto mb-3">
                  <CheckCircle className="w-6 h-6" />
                </div>
                <h3 className="text-xl font-bold text-gray-900">SMAS Store Receipt</h3>
                <p className="text-xs text-gray-500 mt-1">Physical Walk-in Invoice</p>
              </div>

              <div className="py-4 space-y-3 text-sm text-gray-600">
                <div className="flex justify-between">
                  <span>Invoice No:</span>
                  <strong className="text-gray-900">{showReceipt.orderNumber}</strong>
                </div>
                <div className="flex justify-between">
                  <span>Date:</span>
                  <span className="text-gray-900">{new Date(showReceipt.orderDate || showReceipt.createdAt).toLocaleString()}</span>
                </div>
                <div className="flex justify-between">
                  <span>Salesperson:</span>
                  <strong className="text-gray-900">{user?.fullName}</strong>
                </div>
                <div className="flex justify-between">
                  <span>Customer:</span>
                  <span className="text-gray-900">{showReceipt.customerName}</span>
                </div>

                <div className="border-t border-dashed border-gray-200 pt-3 mt-3">
                  <p className="font-semibold text-gray-800 mb-2">Items Purchased:</p>
                  <div className="space-y-2">
                    {showReceipt.items?.map((item: any, i: number) => (
                      <div key={i} className="flex justify-between text-xs">
                        <span>{item.productName || item.name} (x{item.quantity})</span>
                        <strong className="text-gray-900">Rs. {(item.unitPrice * item.quantity).toLocaleString()}</strong>
                      </div>
                    ))}
                  </div>

                  {/* Price breakdown */}
                  <div className="mt-4 space-y-1 text-sm text-gray-700">
                    <div className="flex justify-between">
                      <span>Items Total</span>
                      <span>Rs. {((showReceipt.items || []).reduce((s: number, it: any) => s + (it.unitPrice * it.quantity), 0)).toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Discount</span>
                      <span className="text-red-600">- Rs. {(showReceipt.discountAmount || 0).toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Tax</span>
                      <span>Rs. {(showReceipt.taxAmount || 0).toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between font-semibold text-gray-900 border-t pt-2">
                      <span>Total Paid</span>
                      <span>Rs. {showReceipt.totalAmount.toLocaleString()}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="mt-6 flex space-x-3">
                <button
                  onClick={() => window.print()}
                  className="btn-primary flex-1 flex items-center justify-center space-x-2 py-3"
                >
                  <CheckCircle className="w-4 h-4" />
                  <span>Print</span>
                </button>
                <button
                  onClick={() => setShowReceipt(null)}
                  className="bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold px-4 py-3 rounded-2xl flex-1 text-center"
                >
                  Close Receipt
                </button>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default SalesmanDashboard;
