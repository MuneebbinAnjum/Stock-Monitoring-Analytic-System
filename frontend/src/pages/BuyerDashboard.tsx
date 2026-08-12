import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ShoppingCart, Package, AlertCircle, CheckCircle, Briefcase, File, X, Heart, RefreshCw } from 'lucide-react';
import { OrderApi } from '../api/order.api';
import { ComplaintApi } from '../api/complaint.api';
import { OrderResponse } from '../types';

const BuyerDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [complaints, setComplaints] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [actionMsg, setActionMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [complaintType, setComplaintType] = useState('Product');
  const [selectedOrderId, setSelectedOrderId] = useState('');
  const [complaintTitle, setComplaintTitle] = useState('');
  const [complaintDesc, setComplaintDesc] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setActionMsg({ type, text });
    setTimeout(() => setActionMsg(null), 4000);
  };

  const loadData = async () => {
    if (!user) return;
    setLoading(true);
    try {
      // Uses the secure /orders/my endpoint - only returns this buyer's orders
      const [myOrders, myComplaints] = await Promise.all([
        OrderApi.getMyOrders().catch(() => []),
        ComplaintApi.getMyComplaints().catch(() => [])
      ]);
      setOrders(myOrders || []);
      setComplaints(myComplaints || []);
    } catch (err: any) {
      showMsg('error', 'Failed to load dashboard data.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();

    const handleNotification = (e: any) => {
      const type = e.detail?.notificationType;
      if (
        type === 'OrderApproved' ||
        type === 'OrderRejected' ||
        type === 'OrderCancelled' ||
        type === 'OrderStatusChanged' ||
        type === 'ComplaintResponse' ||
        type === 'ComplaintMessage' ||
        type === 'ComplaintReply'
      ) {
        loadData();
      }
    };

    window.addEventListener('NotificationReceived', handleNotification);
    return () => window.removeEventListener('NotificationReceived', handleNotification);
  }, [user]);

  const handleMarkAsReceived = async (orderId: string) => {
    try {
      await OrderApi.receive(orderId);
      showMsg('success', 'Order marked as received successfully!');
      loadData();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to update order status.');
    }
  };

  const submitComplaint = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedOrderId || !complaintTitle || !complaintDesc) return;
    setSubmitting(true);
    try {
      await ComplaintApi.create({
        orderId: selectedOrderId,
        complaintType,
        title: complaintTitle,
        description: complaintDesc
      });
      showMsg('success', 'Complaint submitted successfully. Admin will review it shortly.');
      setIsModalOpen(false);
      setComplaintTitle('');
      setComplaintDesc('');
      setSelectedOrderId('');
      loadData();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to submit complaint.');
    } finally {
      setSubmitting(false);
    }
  };

  const getStatusColor = (status: string) => {
    if (['Received', 'Delivered', 'Resolved'].includes(status)) return 'bg-emerald-100 text-emerald-700';
    if (status === 'Dispatched') return 'bg-blue-100 text-blue-700 font-semibold';
    if (status === 'Approved') return 'bg-green-100 text-green-700';
    if (['Pending', 'Open', 'In Review'].includes(status)) return 'bg-amber-100 text-amber-700';
    if (['Cancelled', 'Rejected', 'Returned'].includes(status)) return 'bg-red-100 text-red-700';
    return 'bg-gray-100 text-gray-700';
  };

  const totalSpent = orders.reduce((sum, o) =>
    ['Received', 'Delivered', 'Approved', 'Dispatched'].includes(o.status) ? sum + o.totalAmount : sum, 0);
  const activeComplaints = complaints.filter(c => ['Open', 'In Review'].includes(c.status)).length;
  const returnedItems = complaints.filter(c => c.complaintType === 'Return Request' && c.returnApproved).length;

  return (
    <div className="page-container min-h-screen py-8">
      <div className="max-w-6xl mx-auto px-4">
        <motion.div initial={{ opacity: 0, y: -20 }} animate={{ opacity: 1, y: 0 }} className="mb-8 flex flex-col sm:flex-row justify-between items-start sm:items-end gap-4">
          <div>
            <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">Welcome, {user?.fullName}!</h1>
            <p className="text-gray-500 mt-1">Manage your orders, returns, and track your activity.</p>
          </div>
          <div className="flex items-center gap-3">
            <Link to="/cart" className="flex items-center gap-1.5 text-sm font-semibold text-primary-600 bg-primary-50 px-3 py-2 rounded-xl hover:bg-primary-100 transition-colors">
              <ShoppingCart className="w-4 h-4" /> Cart
            </Link>
            <Link to="/wishlist" className="flex items-center gap-1.5 text-sm font-semibold text-rose-600 bg-rose-50 px-3 py-2 rounded-xl hover:bg-rose-100 transition-colors">
              <Heart className="w-4 h-4" /> Wishlist
            </Link>
            <button onClick={() => setIsModalOpen(true)} className="btn-primary text-sm px-4 py-2">
              File Complaint / Return
            </button>
            <button onClick={loadData} className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-xl transition-colors" title="Refresh">
              <RefreshCw className="w-4 h-4" />
            </button>
          </div>
        </motion.div>

        {/* Feedback Messages */}
        <AnimatePresence>
          {actionMsg && (
            <motion.div
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className={`mb-6 p-4 rounded-xl flex items-center gap-3 ${actionMsg.type === 'success'
                ? 'bg-emerald-50 border border-emerald-200 text-emerald-700'
                : 'bg-red-50 border border-red-200 text-red-700'}`}
            >
              {actionMsg.type === 'success'
                ? <CheckCircle className="w-5 h-5 flex-shrink-0" />
                : <AlertCircle className="w-5 h-5 flex-shrink-0" />}
              <span className="text-sm font-medium">{actionMsg.text}</span>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Stats */}
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          {[
            { icon: Briefcase, label: 'Total Orders', value: orders.length.toString(), color: 'bg-blue-100 text-blue-600' },
            { icon: CheckCircle, label: 'Money Spent', value: `Rs. ${totalSpent.toLocaleString()}`, color: 'bg-emerald-100 text-emerald-600' },
            { icon: AlertCircle, label: 'Active Complaints', value: activeComplaints.toString(), color: 'bg-amber-100 text-amber-600' },
            { icon: Package, label: 'Returns Approved', value: returnedItems.toString(), color: 'bg-purple-100 text-purple-600' },
          ].map((stat, i) => {
            const Icon = stat.icon;
            return (
              <motion.div key={i} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }} className="card flex items-center justify-between p-5">
                <div>
                  <p className="text-gray-500 text-sm font-medium">{stat.label}</p>
                  <p className="text-2xl font-bold text-gray-900 mt-1">{stat.value}</p>
                </div>
                <div className={`p-3 rounded-xl ${stat.color}`}><Icon className="w-6 h-6" /></div>
              </motion.div>
            );
          })}
        </div>

        <div className="grid lg:grid-cols-3 gap-6">
          {/* Orders */}
          <div className="lg:col-span-2 space-y-6">
            <div className="card">
              <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center space-x-2">
                <ShoppingCart className="w-5 h-5 text-primary-500" />
                <span>My Orders</span>
              </h2>
              {loading ? (
                <div className="py-8 text-center">
                  <motion.div animate={{ rotate: 360 }} transition={{ duration: 1, repeat: Infinity, ease: 'linear' }} className="w-8 h-8 border-4 border-primary-200 border-t-primary-600 rounded-full mx-auto" />
                </div>
              ) : orders.length === 0 ? (
                <div className="py-12 text-center">
                  <ShoppingCart className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                  <p className="text-gray-400 font-medium">No orders yet.</p>
                  <Link to="/" className="mt-3 inline-block text-sm text-primary-600 font-semibold hover:underline">Browse Products →</Link>
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-gray-200">
                        <th className="text-left py-3 px-2 text-gray-500 text-sm font-semibold">Order</th>
                        <th className="text-left py-3 px-2 text-gray-500 text-sm font-semibold">Amount</th>
                        <th className="text-left py-3 px-2 text-gray-500 text-sm font-semibold">Status</th>
                        <th className="text-left py-3 px-2 text-gray-500 text-sm font-semibold">Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      {orders.map((order) => (
                        <tr key={order.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                          <td className="py-3 px-2">
                            <p className="font-semibold text-sm">{order.orderNumber}</p>
                            <p className="text-xs text-gray-500">{new Date(order.orderDate).toLocaleDateString()}</p>
                          </td>
                          <td className="py-3 px-2 font-bold text-sm">Rs. {order.totalAmount.toLocaleString()}</td>
                          <td className="py-3 px-2">
                            <span className={`px-2 py-0.5 rounded text-xs font-semibold ${getStatusColor(order.status)}`}>{order.status}</span>
                          </td>
                          <td className="py-3 px-2 space-x-2">
                            <button
                              onClick={() => navigate('/order-tracking', { state: { orderNumber: order.orderNumber } })}
                              className="text-primary-600 text-xs font-semibold hover:underline"
                            >
                              Track
                            </button>
                            {order.status === 'Dispatched' && (
                              <button
                                onClick={() => handleMarkAsReceived(order.id)}
                                className="bg-emerald-500 hover:bg-emerald-600 text-white px-2 py-1 rounded text-xs font-bold transition-colors"
                              >
                                Received
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>

          {/* Complaints & Returns */}
          <div className="space-y-6">
            <div className="card h-full">
              <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center space-x-2">
                <File className="w-5 h-5 text-amber-500" />
                <span>My Complaints &amp; Returns</span>
              </h2>
              <div className="space-y-3 max-h-[400px] overflow-y-auto pr-1">
                {loading ? (
                  <p className="text-gray-500 text-center text-sm">Loading...</p>
                ) : complaints.length === 0 ? (
                  <div className="py-8 text-center">
                    <CheckCircle className="w-10 h-10 mx-auto mb-2 text-gray-300" />
                    <p className="text-gray-400 text-sm">No complaints filed.</p>
                  </div>
                ) : (
                  complaints.map(c => (
                    <div key={c.id} className="p-3 border border-gray-100 rounded-xl bg-gray-50/50 hover:border-gray-200 transition-colors">
                      <div className="flex justify-between items-start mb-1">
                        <span className="font-semibold text-gray-900 text-sm line-clamp-1">{c.title}</span>
                        <span className={`px-2 py-0.5 rounded text-[10px] font-bold flex-shrink-0 ml-2 ${getStatusColor(c.status)}`}>{c.status}</span>
                      </div>
                      <p className="text-xs text-gray-500 mb-1">Order: {c.orderNumber} · {c.complaintType}</p>
                      {c.returnApproved === true && (
                        <p className="text-xs text-emerald-600 font-semibold">✓ Return Approved - Inventory Restored</p>
                      )}
                      {c.returnApproved === false && c.complaintType === 'Return Request' && (
                        <p className="text-xs text-red-600 font-semibold">✕ Return Rejected</p>
                      )}
                      {c.adminNotes && (
                        <div className="p-2 bg-blue-50 text-blue-800 text-xs rounded-lg border border-blue-100 mt-2">
                          <strong>Admin Note:</strong> {c.adminNotes}
                        </div>
                      )}
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Complaint Modal */}
      <AnimatePresence>
        {isModalOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl w-full max-w-md overflow-hidden shadow-2xl"
            >
              <div className="p-5 border-b border-gray-100 flex justify-between items-center bg-gray-50">
                <h3 className="font-bold text-lg text-gray-900">File a Complaint / Return</h3>
                <button onClick={() => setIsModalOpen(false)} className="text-gray-400 hover:text-gray-600 transition-colors">
                  <X className="w-5 h-5" />
                </button>
              </div>
              <form onSubmit={submitComplaint} className="p-5 space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Select Order</label>
                  <select
                    required
                    value={selectedOrderId}
                    onChange={e => setSelectedOrderId(e.target.value)}
                    className="input-field py-2"
                  >
                    <option value="">-- Choose an order --</option>
                    {orders.filter(o => !['Pending', 'Cancelled'].includes(o.status)).map(o => (
                      <option key={o.id} value={o.id}>{o.orderNumber} (Rs. {o.totalAmount.toLocaleString()})</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Complaint Type</label>
                  <select value={complaintType} onChange={e => setComplaintType(e.target.value)} className="input-field py-2">
                    <option value="Product">Product Issue</option>
                    <option value="Service">Service / Delivery Issue</option>
                    <option value="Return Request">Return &amp; Refund Request</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
                  <input
                    required
                    type="text"
                    value={complaintTitle}
                    onChange={e => setComplaintTitle(e.target.value)}
                    className="input-field py-2"
                    placeholder="Brief summary of the issue"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <textarea
                    required
                    value={complaintDesc}
                    onChange={e => setComplaintDesc(e.target.value)}
                    className="input-field py-2 h-24 resize-none"
                    placeholder="Provide detailed information..."
                  />
                </div>
                <div className="pt-2 flex justify-end gap-2">
                  <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">
                    Cancel
                  </button>
                  <button type="submit" disabled={submitting} className="btn-primary py-2 px-6">
                    {submitting ? 'Submitting...' : 'Submit Complaint'}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default BuyerDashboard;
