import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { ShoppingCart, MapPin, Phone, CreditCard, ArrowLeft } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { OrderApi } from '../api/order.api';
import { CartApi } from '../api/cart.api';
import { OrderResponse, Product } from '../types';

const Checkout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated } = useAuth();
  
  // Single product checkout vs Cart checkout
  const selectedProduct = (location.state as { selectedProduct?: Product } | null)?.selectedProduct;

  const [cartItems, setCartItems] = useState<any[]>([]);
  const [loadingCart, setLoadingCart] = useState(!selectedProduct);

  const [formData, setFormData] = useState({
    fullName: user?.fullName ?? '',
    phone: user?.phone ?? '',
    address: '',
    city: '',
    paymentMethod: 'Cash on Delivery',
  });
  
  const [orderResult, setOrderResult] = useState<OrderResponse | null>(null);
  const [formError, setFormError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!selectedProduct && isAuthenticated) {
      CartApi.getCart().then(items => {
        setCartItems(items || []);
        setLoadingCart(false);
      }).catch(() => {
        setLoadingCart(false);
      });
    }
  }, [selectedProduct, isAuthenticated]);

  if (!isAuthenticated || !user) {
    return (
      <div className="page-container min-h-screen flex items-center justify-center px-4">
        <div className="card max-w-lg w-full text-center">
          <h2 className="text-2xl font-bold mb-4">Login Required</h2>
          <p className="text-gray-600 mb-6">Please log in to proceed with checkout.</p>
          <Link to="/login" className="btn-primary">Go to Login</Link>
        </div>
      </div>
    );
  }

  if (!selectedProduct && !loadingCart && cartItems.length === 0) {
    return (
      <div className="page-container min-h-screen flex items-center justify-center px-4 py-8">
        <div className="card max-w-lg w-full text-center">
          <ShoppingCart className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h2 className="text-2xl font-bold mb-2">Checkout is empty</h2>
          <p className="text-gray-600 mb-6">You have nothing to checkout.</p>
          <Link to="/" className="btn-primary">Back to Shop</Link>
        </div>
      </div>
    );
  }

  // Calculate Totals
  const subtotal = selectedProduct 
    ? (selectedProduct.discountPrice || selectedProduct.unitPrice)
    : cartItems.reduce((sum, item) => sum + item.subtotal, 0);

  const deliveryCharge = subtotal > 5000 ? 0 : 250;
  
  // Calculate dynamic tax based on each item's tax percentage
  const taxCharge = selectedProduct
    ? (selectedProduct.discountPrice || selectedProduct.unitPrice) * ((selectedProduct.taxPercentage || 0) / 100)
    : cartItems.reduce((sum, item) => sum + (item.subtotal * (item.taxPercentage || 0) / 100), 0);
    
  const totalAmount = subtotal + deliveryCharge + taxCharge;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
    setFormError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.address || !formData.city) {
      setFormError('Please enter your delivery address and city.');
      return;
    }

    setIsSubmitting(true);
    try {
      let itemsToOrder = [];
      let currentCartItems = cartItems;

      if (!selectedProduct) {
        currentCartItems = await CartApi.getCart().catch(() => cartItems) || cartItems;
        setCartItems(currentCartItems);
      }

      if (selectedProduct) {
        itemsToOrder.push({ productId: selectedProduct.id, quantity: 1 });
      } else {
        itemsToOrder = currentCartItems.map(i => ({ productId: i.productId, quantity: i.quantity }));
      }

      // We omit customerId because the backend securely extracts it from the JWT
      const order = await OrderApi.create({
        items: itemsToOrder,
        deliveryCity: formData.city,
        deliveryAddress: formData.address,
        paymentMethod: formData.paymentMethod,
        deliveryPeriod: selectedProduct?.deliveryPeriod || '3-5 business days',
      });

      // Clear cart if we just checked out the cart
      if (!selectedProduct && currentCartItems.length > 0) {
        await CartApi.clearCart().catch(() => {});
      }

      setOrderResult(order);
      setFormError('');
    } catch (err: any) {
      setFormError(err.response?.data?.message || 'Could not place order. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="page-container min-h-screen py-12">
      <div className="max-w-5xl mx-auto px-4">
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Checkout</h1>
          <p className="text-gray-500">Provide your delivery details to complete your purchase.</p>
        </motion.div>

        {formError && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700">
            {formError}
          </motion.div>
        )}

        {orderResult ? (
          <motion.div initial={{ opacity: 0, scale: 0.98 }} animate={{ opacity: 1, scale: 1 }} className="card text-center py-12">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-8 h-8 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
            </div>
            <h2 className="text-3xl font-bold mb-4">Order Confirmed!</h2>
            <p className="text-gray-600 mb-8 max-w-md mx-auto">Your order <strong>{orderResult.orderNumber}</strong> has been successfully placed. We'll send you an update when it ships.</p>
            
            <div className="flex justify-center gap-4">
              <Link to="/" className="btn-secondary">Continue Shopping</Link>
              <button onClick={() => navigate('/order-tracking', { state: { orderNumber: orderResult.orderNumber } })} className="btn-primary">Track Order</button>
            </div>
          </motion.div>
        ) : loadingCart ? (
          <div className="text-center py-20">Loading checkout details...</div>
        ) : (
          <div className="grid lg:grid-cols-3 gap-8">
            <div className="lg:col-span-2">
              <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }} className="card">
                <h3 className="text-lg font-bold text-gray-900 mb-6 border-b border-gray-100 pb-4">Delivery Information</h3>
                <form onSubmit={handleSubmit} className="space-y-5">
                  <div className="grid sm:grid-cols-2 gap-5">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
                      <input type="text" name="fullName" value={formData.fullName} className="input-field py-2" disabled />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
                      <div className="relative">
                        <Phone className="absolute left-3 top-2.5 w-5 h-5 text-gray-400" />
                        <input type="tel" name="phone" value={formData.phone} onChange={handleChange} className="input-field pl-10 py-2" required />
                      </div>
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">City</label>
                    <input type="text" name="city" value={formData.city} onChange={handleChange} className="input-field py-2" required placeholder="e.g. Karachi" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Complete Address</label>
                    <div className="relative">
                      <MapPin className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                      <textarea name="address" value={formData.address} onChange={handleChange} className="input-field pl-10 py-2 resize-none h-24" required placeholder="Street, House No, Area..." />
                    </div>
                  </div>

                  <div className="pt-4 border-t border-gray-100">
                    <h4 className="font-semibold text-gray-900 mb-3">Payment Method</h4>
                    <div className="p-4 border border-primary-200 bg-primary-50 rounded-xl flex items-center justify-between cursor-pointer">
                      <div className="flex items-center space-x-3">
                        <CreditCard className="w-6 h-6 text-primary-600" />
                        <span className="font-semibold text-primary-900">Cash on Delivery</span>
                      </div>
                      <div className="w-5 h-5 rounded-full border-4 border-primary-600"></div>
                    </div>
                  </div>

                  <motion.button whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }} type="submit" disabled={isSubmitting} className="btn-primary w-full py-3.5 text-lg shadow-lg shadow-primary-500/30">
                    {isSubmitting ? 'Processing...' : 'Confirm & Place Order'}
                  </motion.button>
                </form>
              </motion.div>
            </div>

            <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} className="lg:col-span-1">
              <div className="card sticky top-24">
                <h3 className="font-bold text-lg mb-4 text-gray-900 border-b border-gray-100 pb-4">Order Summary</h3>
                
                <div className="space-y-4 mb-6 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                  {selectedProduct ? (
                    <div className="flex gap-3">
                      <img src={selectedProduct.productImages?.[0]?.imageUrl || 'https://via.placeholder.com/60'} alt={selectedProduct.name} className="w-16 h-16 rounded-lg object-cover" />
                      <div>
                        <p className="font-semibold text-sm line-clamp-2">{selectedProduct.name}</p>
                        <p className="text-gray-500 text-xs mt-1">Qty: 1</p>
                        <p className="font-bold text-primary-600 text-sm mt-1">Rs. {(selectedProduct.discountPrice || selectedProduct.unitPrice).toLocaleString()}</p>
                      </div>
                    </div>
                  ) : (
                    cartItems.map((item, idx) => (
                      <div key={idx} className="flex gap-3">
                        <img src={item.productImage || 'https://via.placeholder.com/60'} alt={item.productName} className="w-16 h-16 rounded-lg object-cover" />
                        <div>
                          <p className="font-semibold text-sm line-clamp-2">{item.productName}</p>
                          <p className="text-gray-500 text-xs mt-1">Qty: {item.quantity}</p>
                          <p className="font-bold text-primary-600 text-sm mt-1">Rs. {item.unitPrice.toLocaleString()}</p>
                        </div>
                      </div>
                    ))
                  )}
                </div>

                <div className="space-y-3 mb-6 pt-4 border-t border-gray-100">
                  <div className="flex justify-between text-gray-600">
                    <span>Subtotal</span>
                    <span>Rs. {subtotal.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between text-gray-600">
                    <span>Delivery Charge</span>
                    <span>{deliveryCharge === 0 ? <span className="text-emerald-500 font-semibold">Free</span> : `Rs. ${deliveryCharge}`}</span>
                  </div>
                  <div className="flex justify-between text-gray-600">
                    <span>Tax (5%)</span>
                    <span>Rs. {taxCharge.toLocaleString()}</span>
                  </div>
                </div>

                <div className="flex justify-between items-end pt-4 border-t border-gray-200">
                  <span className="font-bold text-gray-900">Total Payable</span>
                  <div className="text-right">
                    <span className="text-2xl font-black text-primary-600">Rs. {totalAmount.toLocaleString()}</span>
                    <p className="text-xs text-gray-400 mt-1">Including VAT & Taxes</p>
                  </div>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Checkout;
