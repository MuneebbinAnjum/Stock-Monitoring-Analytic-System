import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Package, Search, CheckCircle, AlertCircle, X, Truck, Eye } from 'lucide-react';
import { OrderApi } from '../../api/order.api';

interface AdminOrdersProps {
  orders: any[];
  onOrderUpdated: () => void;
}

const COURIERS = ['TCS', 'DHL', 'Leopards', 'M&P', 'FedEx', 'PostEx', 'Trax'];

const statusColor = (status: string) => {
  if (['Received', 'Delivered'].includes(status)) return 'bg-emerald-100 text-emerald-700';
  if (status === 'Dispatched') return 'bg-blue-100 text-blue-700';
  if (status === 'Approved') return 'bg-green-100 text-green-700';
  if (status === 'Pending') return 'bg-amber-100 text-amber-700';
  if (['Cancelled', 'Rejected', 'Returned'].includes(status)) return 'bg-red-100 text-red-700';
  return 'bg-gray-100 text-gray-700';
};

const AdminOrders: React.FC<AdminOrdersProps> = ({ orders, onOrderUpdated }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filter, setFilter] = useState('All');
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Dispatch modal state
  const [dispatchModal, setDispatchModal] = useState<{ open: boolean; orderId: string; orderNumber: string } | null>(null);
  const [selectedCourier, setSelectedCourier] = useState('TCS');
  const [dispatching, setDispatching] = useState(false);

  // Order detail modal
  const [detailOrder, setDetailOrder] = useState<any | null>(null);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 4000);
  };

  const handleApproveOrder = async (orderId: string, orderNumber: string) => {
    try {
      await OrderApi.approve(orderId);
      showMsg('success', `Order ${orderNumber} approved successfully.`);
      onOrderUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || `Failed to approve order ${orderNumber}.`);
    }
  };

  const handleRejectOrder = async (orderId: string, orderNumber: string) => {
    if (!window.confirm(`Reject order ${orderNumber}? Stock will be restored.`)) return;
    try {
      await OrderApi.reject(orderId);
      showMsg('success', `Order ${orderNumber} rejected. Stock has been restored.`);
      onOrderUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || `Failed to reject order ${orderNumber}.`);
    }
  };

  const openDispatch = (order: any) => {
    setSelectedCourier('TCS');
    setDispatchModal({ open: true, orderId: order.id, orderNumber: order.orderNumber });
  };

  const handleDispatch = async () => {
    if (!dispatchModal) return;
    setDispatching(true);
    try {
      await OrderApi.dispatch(dispatchModal.orderId, selectedCourier);
      showMsg('success', `Order ${dispatchModal.orderNumber} dispatched via ${selectedCourier}.`);
      setDispatchModal(null);
      onOrderUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to dispatch order.');
    } finally {
      setDispatching(false);
    }
  };

  const filteredOrders = orders.filter(o =>
    (filter === 'All' || o.status === filter) &&
    ((o.orderNumber || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
      (o.customerName || '').toLowerCase().includes(searchTerm.toLowerCase()))
  );

  return (
    <div className="space-y-6">
      {/* Feedback */}
      <AnimatePresence>
        {msg && (
          <motion.div
            initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            className={`p-4 rounded-xl flex items-center gap-3 ${msg.type === 'success'
              ? 'bg-emerald-50 border border-emerald-200 text-emerald-700'
              : 'bg-red-50 border border-red-200 text-red-700'}`}
          >
            {msg.type === 'success' ? <CheckCircle className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
            <span className="text-sm font-medium">{msg.text}</span>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="card">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-2.5 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search by order number or customer..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input-field pl-10"
            />
          </div>
          <div className="flex gap-2 flex-wrap">
            {['All', 'Pending', 'Approved', 'Dispatched', 'Delivered', 'Rejected', 'Cancelled'].map(f => (
              <button
                key={f}
                onClick={() => setFilter(f)}
                className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${filter === f ? 'bg-primary-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
              >
                {f}
                {f !== 'All' && (
                  <span className="ml-1 opacity-70">({orders.filter(o => o.status === f).length})</span>
                )}
              </button>
            ))}
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50/50">
                <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Order #</th>
                <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Customer</th>
                <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
                <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
                <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Date</th>
                <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredOrders.map((order) => (
                <tr key={order.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td className="py-3 px-4">
                    <p className="font-mono font-semibold text-sm text-gray-900">{order.orderNumber}</p>
                    <p className="text-xs text-gray-400">{order.paymentMethod}</p>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-700">{order.customerName}</td>
                  <td className="py-3 px-4 font-semibold text-sm text-gray-900">Rs. {order.totalAmount?.toLocaleString()}</td>
                  <td className="py-3 px-4">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${statusColor(order.status)}`}>
                      {order.status}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-gray-500 text-xs">{new Date(order.orderDate).toLocaleDateString()}</td>
                  <td className="py-3 px-4">
                    <div className="flex items-center gap-1.5">
                      <button
                        onClick={() => setDetailOrder(order)}
                        className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                        title="View details"
                      >
                        <Eye className="w-3.5 h-3.5" />
                      </button>
                      {order.status === 'Pending' && (
                        <>
                          <button
                            onClick={() => handleApproveOrder(order.id, order.orderNumber)}
                            className="bg-emerald-500 hover:bg-emerald-600 text-white text-xs font-bold py-1 px-2.5 rounded-lg transition-colors"
                          >
                            Approve
                          </button>
                          <button
                            onClick={() => handleRejectOrder(order.id, order.orderNumber)}
                            className="bg-red-500 hover:bg-red-600 text-white text-xs font-bold py-1 px-2.5 rounded-lg transition-colors"
                          >
                            Reject
                          </button>
                        </>
                      )}
                      {order.status === 'Approved' && (
                        <button
                          onClick={() => openDispatch(order)}
                          className="bg-blue-500 hover:bg-blue-600 text-white text-xs font-bold py-1 px-2.5 rounded-lg flex items-center gap-1 transition-colors"
                        >
                          <Truck className="w-3 h-3" />
                          Dispatch
                        </button>
                      )}
                      {order.status === 'Dispatched' && (
                        <span className="text-xs text-blue-600 font-semibold font-mono">{order.courierRef}</span>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {filteredOrders.length === 0 && (
            <div className="py-12 text-center">
              <Package className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p className="text-gray-500 font-medium">No orders found.</p>
            </div>
          )}
        </div>
      </div>

      {/* Dispatch Modal */}
      <AnimatePresence>
        {dispatchModal?.open && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl w-full max-w-md shadow-2xl"
            >
              <div className="flex justify-between items-center p-5 border-b border-gray-100 bg-gray-50 rounded-t-2xl">
                <div>
                  <h3 className="text-xl font-bold text-gray-900">Dispatch Order</h3>
                  <p className="text-sm text-gray-500 mt-0.5">{dispatchModal.orderNumber}</p>
                </div>
                <button onClick={() => setDispatchModal(null)} className="text-gray-400 hover:text-gray-600 transition-colors">
                  <X className="w-5 h-5" />
                </button>
              </div>
              <div className="p-5 space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Select Courier Provider</label>
                  <div className="grid grid-cols-3 gap-2">
                    {COURIERS.map(courier => (
                      <button
                        key={courier}
                        type="button"
                        onClick={() => setSelectedCourier(courier)}
                        className={`py-2.5 px-3 rounded-xl text-sm font-semibold border-2 transition-all ${selectedCourier === courier
                          ? 'border-primary-500 bg-primary-50 text-primary-700'
                          : 'border-gray-200 bg-white text-gray-600 hover:border-gray-300'
                          }`}
                      >
                        {courier}
                      </button>
                    ))}
                  </div>
                </div>
                <div className="flex justify-end gap-3 pt-2 border-t border-gray-100">
                  <button
                    type="button"
                    onClick={() => setDispatchModal(null)}
                    className="px-5 py-2.5 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-xl transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleDispatch}
                    disabled={dispatching}
                    className="btn-primary px-6 py-2.5 flex items-center gap-2"
                  >
                    <Truck className="w-4 h-4" />
                    {dispatching ? 'Dispatching...' : `Dispatch via ${selectedCourier}`}
                  </button>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

      {/* Order Detail Modal */}
      <AnimatePresence>
        {detailOrder && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl w-full max-w-lg shadow-2xl max-h-[85vh] overflow-y-auto"
            >
              <div className="flex justify-between items-center p-5 border-b border-gray-100 bg-gray-50 rounded-t-2xl sticky top-0">
                <div>
                  <h3 className="text-xl font-bold text-gray-900">Order Details</h3>
                  <p className="text-sm font-mono text-primary-600 mt-0.5">{detailOrder.orderNumber}</p>
                </div>
                <button onClick={() => setDetailOrder(null)} className="text-gray-400 hover:text-gray-600 transition-colors">
                  <X className="w-5 h-5" />
                </button>
              </div>
              <div className="p-5 space-y-4">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div><span className="text-gray-500">Customer</span><p className="font-semibold">{detailOrder.customerName}</p></div>
                  <div><span className="text-gray-500">Status</span><p><span className={`px-2 py-0.5 rounded text-xs font-semibold ${statusColor(detailOrder.status)}`}>{detailOrder.status}</span></p></div>
                  <div><span className="text-gray-500">Date</span><p className="font-semibold">{new Date(detailOrder.orderDate).toLocaleDateString()}</p></div>
                  <div><span className="text-gray-500">Payment</span><p className="font-semibold">{detailOrder.paymentMethod}</p></div>
                  <div className="col-span-2"><span className="text-gray-500">Delivery Address</span><p className="font-semibold">{detailOrder.deliveryAddress}, {detailOrder.deliveryCity}</p></div>
                  {detailOrder.courierRef && (
                    <div className="col-span-2"><span className="text-gray-500">Courier Ref</span><p className="font-mono font-semibold text-blue-600">{detailOrder.courierRef}</p></div>
                  )}
                </div>
                <div>
                  <h4 className="font-semibold text-gray-800 mb-2">Order Items</h4>
                  <div className="space-y-2">
                    {(detailOrder.items || []).map((item: any) => (
                      <div key={item.id} className="flex justify-between items-center p-3 bg-gray-50 rounded-xl text-sm">
                        <div>
                          <p className="font-medium">{item.productName}</p>
                          <p className="text-gray-500">Qty: {item.quantity} × Rs. {item.unitPrice?.toLocaleString()}</p>
                        </div>
                        <p className="font-semibold">Rs. {(item.quantity * item.unitPrice)?.toLocaleString()}</p>
                      </div>
                    ))}
                  </div>
                </div>
                <div className="flex justify-between items-center pt-3 border-t border-gray-200">
                  <span className="font-bold text-gray-800">Total Amount</span>
                  <span className="text-xl font-extrabold text-primary-600">Rs. {detailOrder.totalAmount?.toLocaleString()}</span>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default AdminOrders;
