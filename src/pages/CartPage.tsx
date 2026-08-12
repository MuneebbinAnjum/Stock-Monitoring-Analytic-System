import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ShoppingBag, Trash2, Plus, Minus, ArrowRight, CheckCircle, AlertCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { CartApi } from '../api/cart.api';

const CartPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [updating, setUpdating] = useState<string | null>(null);

  const showToast = (type: 'success' | 'error', text: string) => {
    setToast({ type, text });
    setTimeout(() => setToast(null), 3500);
  };

  const loadCart = async (options: { showLoading?: boolean } = {}) => {
    const shouldShowLoading = options.showLoading ?? true;
    if (shouldShowLoading) setLoading(true);
    try {
      const data = await CartApi.getCart();
      setItems(data || []);
    } catch { } finally {
      if (shouldShowLoading) setLoading(false);
    }
  };

  useEffect(() => { loadCart(); }, []);

  const updateQuantity = async (id: string, qty: number) => {
    if (qty < 1) return;
    setUpdating(id);
    try {
      await CartApi.updateQuantity(id, qty);
      await loadCart({ showLoading: false });
    } catch (err: any) {
      showToast('error', err.response?.data?.message || 'Failed to update quantity — may exceed available stock.');
    } finally {
      setUpdating(null);
    }
  };

  const removeItem = async (id: string, name: string) => {
    setUpdating(id);
    try {
      await CartApi.removeItem(id);
      await loadCart({ showLoading: false });
      showToast('success', `"${name}" removed from cart.`);
    } catch {
      showToast('error', 'Failed to remove item.');
    } finally {
      setUpdating(null);
    }
  };

  const total = items.reduce((sum, item) => sum + item.subtotal, 0);
  const deliveryCharge = total > 5000 ? 0 : 250;

  if (loading) {
    return (
      <div className="page-container min-h-screen py-12">
        <div className="max-w-5xl mx-auto px-4 space-y-4">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="card flex items-center gap-4 p-4 animate-pulse">
              <div className="w-20 h-20 bg-gray-200 rounded-lg flex-shrink-0" />
              <div className="flex-1 space-y-2">
                <div className="h-4 bg-gray-200 rounded w-2/3" />
                <div className="h-4 bg-gray-200 rounded w-1/4" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="page-container min-h-screen py-12">
      {/* Toast */}
      <AnimatePresence>
        {toast && (
          <motion.div
            initial={{ opacity: 0, y: -20, x: '-50%' }}
            animate={{ opacity: 1, y: 0, x: '-50%' }}
            exit={{ opacity: 0, y: -20, x: '-50%' }}
            className={`fixed top-20 left-1/2 z-50 flex items-center gap-3 px-5 py-3 rounded-2xl shadow-xl ${
              toast.type === 'success' ? 'bg-emerald-600 text-white' : 'bg-red-600 text-white'
            }`}
          >
            {toast.type === 'success'
              ? <CheckCircle className="w-5 h-5 flex-shrink-0" />
              : <AlertCircle className="w-5 h-5 flex-shrink-0" />
            }
            <span className="font-medium text-sm">{toast.text}</span>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="max-w-5xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-900 mb-8 flex items-center space-x-3">
          <ShoppingBag className="w-8 h-8 text-primary-600" />
          <span>Shopping Cart</span>
          {items.length > 0 && (
            <span className="ml-2 text-lg font-normal text-gray-500">({items.length} item{items.length !== 1 ? 's' : ''})</span>
          )}
        </h1>

        {items.length === 0 ? (
          <div className="card text-center py-16">
            <ShoppingBag className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-gray-700 mb-2">Your cart is empty</h2>
            <p className="text-gray-500 mb-6">Looks like you haven't added anything yet.</p>
            <button onClick={() => navigate('/')} className="btn-primary">
              Continue Shopping
            </button>
          </div>
        ) : (
          <div className="grid lg:grid-cols-3 gap-8">
            <div className="lg:col-span-2 space-y-4">
              <AnimatePresence>
                {items.map(item => (
                  <motion.div
                    key={item.id}
                    layout
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -20, height: 0 }}
                    className="card flex items-center gap-4 p-4"
                  >
                    <img
                      src={item.productImage || `https://via.placeholder.com/100?text=${encodeURIComponent(item.productName || 'Product')}`}
                      alt={item.productName}
                      className="w-20 h-20 object-cover rounded-lg flex-shrink-0"
                    />
                    <div className="flex-1 min-w-0">
                      <h3 className="font-semibold text-gray-900 line-clamp-1">{item.productName}</h3>
                      <p className="text-primary-600 font-bold mt-1">Rs. {item.unitPrice?.toLocaleString()}</p>
                      {item.stockAvailable !== undefined && item.quantity >= item.stockAvailable && (
                        <p className="text-xs text-amber-600 mt-1">Max stock reached</p>
                      )}
                    </div>
                    <div className="flex items-center space-x-3 flex-shrink-0">
                      <div className="flex items-center space-x-1 border border-gray-200 rounded-lg p-1">
                        <button
                          onClick={() => updateQuantity(item.id, item.quantity - 1)}
                          disabled={updating === item.id || item.quantity <= 1}
                          className="p-1.5 hover:bg-gray-100 rounded disabled:opacity-40 transition-colors"
                        >
                          <Minus className="w-3.5 h-3.5 text-gray-600" />
                        </button>
                        <span className="w-8 text-center font-semibold text-sm">
                          {updating === item.id ? (
                            <span className="inline-block w-3.5 h-3.5 border-2 border-gray-400 border-t-primary-600 rounded-full animate-spin" />
                          ) : item.quantity}
                        </span>
                        <button
                          onClick={() => updateQuantity(item.id, item.quantity + 1)}
                          disabled={updating === item.id || (item.stockAvailable !== undefined && item.quantity >= item.stockAvailable)}
                          className="p-1.5 hover:bg-gray-100 rounded disabled:opacity-40 transition-colors"
                        >
                          <Plus className="w-3.5 h-3.5 text-gray-600" />
                        </button>
                      </div>
                      <p className="font-bold text-gray-900 w-24 text-right text-sm">
                        Rs. {item.subtotal?.toLocaleString()}
                      </p>
                      <button
                        onClick={() => removeItem(item.id, item.productName)}
                        disabled={updating === item.id}
                        className="p-2 text-red-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-40"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </motion.div>
                ))}
              </AnimatePresence>
            </div>

            <div className="lg:col-span-1">
              <div className="card sticky top-24">
                <h3 className="text-lg font-bold text-gray-900 mb-4 pb-4 border-b border-gray-100">Order Summary</h3>
                <div className="space-y-3 mb-6">
                  <div className="flex justify-between text-gray-600 text-sm">
                    <span>Subtotal ({items.length} items)</span>
                    <span>Rs. {total.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between text-gray-600 text-sm">
                    <span>Delivery Charge</span>
                    <span className={deliveryCharge === 0 ? 'text-emerald-600 font-semibold' : ''}>
                      {deliveryCharge === 0 ? 'Free' : `Rs. ${deliveryCharge}`}
                    </span>
                  </div>
                  {deliveryCharge > 0 && (
                    <p className="text-xs text-gray-400">Free delivery on orders above Rs. 5,000</p>
                  )}
                  <div className="border-t border-gray-200 pt-3 flex justify-between font-bold text-lg text-gray-900">
                    <span>Estimated Total</span>
                    <span className="text-primary-600">Rs. {(total + deliveryCharge).toLocaleString()}</span>
                  </div>
                </div>
                <button
                  onClick={() => navigate('/buyer/checkout')}
                  className="btn-primary w-full flex items-center justify-center space-x-2 py-3"
                >
                  <span>Proceed to Checkout</span>
                  <ArrowRight className="w-5 h-5" />
                </button>
                <button
                  onClick={() => navigate('/')}
                  className="w-full mt-3 py-2.5 text-sm font-medium text-gray-500 hover:text-gray-700 transition-colors"
                >
                  ← Continue Shopping
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default CartPage;
