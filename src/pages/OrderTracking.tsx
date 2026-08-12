import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useLocation } from 'react-router-dom';
import { Search, CheckCircle, Truck, MapPin, Clock } from 'lucide-react';
import { OrderApi } from '../api/order.api';
import { OrderResponse } from '../types';

const OrderTracking: React.FC = () => {
  const location = useLocation();
  const initialOrderNumber = (location.state as { orderNumber?: string } | null)?.orderNumber || '';
  const [orderNumber, setOrderNumber] = useState(initialOrderNumber);
  const [trackedOrder, setTrackedOrder] = useState<OrderResponse | null>(null);
  const [formError, setFormError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (initialOrderNumber) {
      fetchOrder(initialOrderNumber);
    }
  }, [initialOrderNumber]);

  const fetchOrder = async (number: string) => {
    setFormError('');
    setIsLoading(true);
    try {
      const order = await OrderApi.getByNumber(number);
      setTrackedOrder(order);
    } catch (err: any) {
      setTrackedOrder(null);
      setFormError(err.response?.data?.message || 'Unable to find order with that number.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    if (!orderNumber.trim()) {
      setFormError('Enter a valid order number.');
      return;
    }

    await fetchOrder(orderNumber.trim());
  };

  const statusSteps = [
    'Pending',
    'Approved',
    'Rejected',
    'Dispatched',
    'Delivered',
    'Cancelled',
  ];

  const renderStatusLabel = (status: string) => {
    if (status === 'Pending') return 'Order Received';
    if (status === 'Approved') return 'Order Approved';
    if (status === 'Dispatched') return 'Out for Delivery';
    if (status === 'Delivered') return 'Delivered';
    if (status === 'Rejected') return 'Rejected';
    if (status === 'Cancelled') return 'Cancelled';
    return status;
  };

  return (
    <div className="page-container min-h-screen py-8">
      <div className="max-w-3xl mx-auto px-4">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center mb-8"
        >
          <h1 className="section-title">Track Your Order</h1>
          <p className="section-subtitle">Enter your order number to see the latest status</p>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="card"
        >
          <form onSubmit={handleSearch} className="mb-6">
            <div className="relative">
              <Search className="absolute left-4 top-4 w-6 h-6 text-gray-400" />
              <input
                type="text"
                placeholder="Enter order number (e.g., ORD-240516123456-ABC)"
                value={orderNumber}
                onChange={(e) => setOrderNumber(e.target.value)}
                className="input-field pl-12 py-3"
              />
            </div>
            <motion.button
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              type="submit"
              disabled={isLoading}
              className="btn-primary w-full mt-4"
            >
              {isLoading ? 'Searching...' : 'Track Order'}
            </motion.button>
          </form>

          {formError && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
              {formError}
            </div>
          )}

          {trackedOrder && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="space-y-6"
            >
              <div className="rounded-xl border border-gray-200 p-6 bg-white">
                <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm text-gray-500">Order Number</p>
                    <p className="text-lg font-semibold text-gray-900">{trackedOrder.orderNumber}</p>
                  </div>
                  <div className="inline-flex items-center gap-2 rounded-full bg-primary-50 px-4 py-2 text-primary-700">
                    <CheckCircle className="w-5 h-5" />
                    <span>{trackedOrder.status}</span>
                  </div>
                </div>

                <div className="mt-4 grid gap-4 sm:grid-cols-2">
                  <div>
                    <p className="text-sm text-gray-500">Customer</p>
                    <p className="font-medium text-gray-900">{trackedOrder.customerName}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Delivery City</p>
                    <p className="font-medium text-gray-900">{trackedOrder.deliveryCity}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Delivery Address</p>
                    <p className="font-medium text-gray-900">{trackedOrder.deliveryAddress}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Ordered On</p>
                    <p className="font-medium text-gray-900">{new Date(trackedOrder.orderDate).toLocaleDateString()}</p>
                  </div>
                </div>
              </div>

              <div className="rounded-xl border border-gray-200 p-6 bg-white">
                <h3 className="text-lg font-semibold mb-4">Order Items</h3>
                <div className="space-y-4">
                  {trackedOrder.items.map((item) => (
                    <div key={item.id} className="flex justify-between gap-4">
                      <div>
                        <p className="font-medium text-gray-900">{item.productName}</p>
                        <p className="text-sm text-gray-500">Qty: {item.quantity}</p>
                      </div>
                      <p className="font-semibold text-gray-900">Rs. {(item.unitPrice * item.quantity).toLocaleString()}</p>
                    </div>
                  ))}
                </div>
              </div>

              <div className="rounded-xl border border-gray-200 p-6 bg-white">
                <div className="grid gap-4 sm:grid-cols-2">
                  <div>
                    <p className="text-sm text-gray-500">Delivery Method</p>
                    <p className="font-medium text-gray-900">{trackedOrder.deliveryPeriod || 'Standard'}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Payment Method</p>
                    <p className="font-medium text-gray-900">{trackedOrder.paymentMethod || 'Cash on Delivery'}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Totals</p>
                    <div className="text-sm text-gray-600">
                      <div className="flex justify-between"><span>Subtotal</span><span>Rs. {trackedOrder.items.reduce((s, it) => s + it.unitPrice * it.quantity, 0).toLocaleString()}</span></div>
                      <div className="flex justify-between"><span>Discount</span><span>- Rs. {(trackedOrder.discountAmount || 0).toLocaleString()}</span></div>
                      <div className="flex justify-between"><span>Tax</span><span>Rs. {(trackedOrder.taxAmount || 0).toLocaleString()}</span></div>
                      <div className="flex justify-between font-bold pt-2"><span>Total</span><span>Rs. {trackedOrder.totalAmount.toLocaleString()}</span></div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="rounded-xl border border-gray-200 p-6 bg-white">
                <h3 className="text-lg font-semibold mb-4">Order Status Timeline</h3>
                <div className="space-y-4">
                  {['Pending', 'Approved', 'Dispatched', 'Delivered'].map((status) => {
                    const active = statusSteps.indexOf(trackedOrder.status) >= statusSteps.indexOf(status);
                    return (
                      <div key={status} className="flex items-center gap-3">
                        <div className={`w-10 h-10 rounded-full flex items-center justify-center ${active ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-500'}`}>
                          <Clock className="w-5 h-5" />
                        </div>
                        <div>
                          <p className={`font-semibold ${active ? 'text-gray-900' : 'text-gray-500'}`}>{renderStatusLabel(status)}</p>
                          <p className="text-sm text-gray-500">{active ? 'Completed' : 'Waiting for update'}</p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </motion.div>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default OrderTracking;
