import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { ShoppingCart, Truck, Shield, Heart, CheckCircle, AlertCircle } from 'lucide-react';
import { ProductApi } from '../api/product.api';
import { CartApi } from '../api/cart.api';
import { WishlistApi } from '../api/wishlist.api';
import { Product } from '../types';
import { useAuth } from '../context/AuthContext';

const ProductDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedImage, setSelectedImage] = useState<string>('');
  const [toast, setToast] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [cartLoading, setCartLoading] = useState(false);
  const [wishLoading, setWishLoading] = useState(false);

  const showToast = (type: 'success' | 'error', text: string) => {
    setToast({ type, text });
    setTimeout(() => setToast(null), 3500);
  };

  useEffect(() => {
    const fetchProduct = async () => {
      if (!id) return;
      try {
        const data = await ProductApi.getById(id);
        setProduct(data);
        if (data.productImages && data.productImages.length > 0) {
          setSelectedImage(data.productImages[0].imageUrl);
        }
      } catch (err: any) {
        setError(err.response?.data?.message || 'Unable to load product');
      } finally {
        setLoading(false);
      }
    };
    fetchProduct();

    const handleInventory = (e: any) => {
      const pid = e?.detail?.ProductId || e?.detail?.productId;
      if (!pid || !id) return;
      if (pid.toString() === id.toString()) {
        fetchProduct();
      }
    };

    window.addEventListener('InventoryUpdated', handleInventory);
    window.addEventListener('StockAlertCreated', handleInventory);
    window.addEventListener('StockAlertResolved', handleInventory);
    return () => {
      window.removeEventListener('InventoryUpdated', handleInventory);
      window.removeEventListener('StockAlertCreated', handleInventory);
      window.removeEventListener('StockAlertResolved', handleInventory);
    };
  }, [id]);

  const handleAddToCart = async () => {
    if (!product) return;
    setCartLoading(true);
    try {
      await CartApi.addItem({ productId: product.id, quantity: 1 });
      showToast('success', `"${product.name}" added to cart!`);
    } catch (err: any) {
      if (err.response?.status === 401) navigate('/login');
      else showToast('error', err.response?.data?.message || 'Failed to add to cart.');
    } finally {
      setCartLoading(false);
    }
  };

  const handleAddToWishlist = async () => {
    if (!product) return;
    setWishLoading(true);
    try {
      await WishlistApi.addItem(product.id);
      showToast('success', `"${product.name}" added to wishlist!`);
    } catch (err: any) {
      if (err.response?.status === 401) navigate('/login');
      else showToast('error', err.response?.data?.message || 'Failed to add to wishlist.');
    } finally {
      setWishLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="page-container min-h-screen py-8">
        <div className="max-w-6xl mx-auto px-4">
          <div className="grid lg:grid-cols-2 gap-12 animate-pulse">
            <div className="h-[500px] bg-gray-200 rounded-2xl" />
            <div className="space-y-4">
              <div className="h-4 bg-gray-200 rounded w-1/3" />
              <div className="h-8 bg-gray-200 rounded w-full" />
              <div className="h-8 bg-gray-200 rounded w-2/3" />
              <div className="h-16 bg-gray-200 rounded" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="page-container min-h-screen py-8 text-center text-red-600">{error || 'Product not found.'}</div>
    );
  }

  const effectivePrice = product.discountPrice || product.unitPrice;

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
              ? <CheckCircle className="w-5 h-5" />
              : <AlertCircle className="w-5 h-5" />
            }
            <span className="font-medium text-sm">{toast.text}</span>
          </motion.div>
        )}
      </AnimatePresence>
      <div className="max-w-6xl mx-auto px-4">
        <div className="grid lg:grid-cols-2 gap-12">
          {/* Image Gallery */}
          <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }}>
            <div className="card p-2 mb-4 bg-gray-50 border border-gray-100">
              <img
                src={selectedImage || `https://via.placeholder.com/600x600?text=${encodeURIComponent(product.name)}`}
                alt={product.name}
                className="w-full h-[500px] object-contain rounded-xl"
              />
            </div>
            {product.productImages && product.productImages.length > 1 && (
              <div className="flex gap-4 overflow-x-auto pb-2">
                {product.productImages.map((img) => (
                  <button
                    key={img.id || img.imageId || img.imageUrl}
                    onClick={() => setSelectedImage(img.imageUrl)}
                    className={`flex-shrink-0 w-24 h-24 rounded-lg overflow-hidden border-2 transition-all ${
                      selectedImage === img.imageUrl ? 'border-primary-500 ring-2 ring-primary-500/20' : 'border-transparent opacity-70 hover:opacity-100'
                    }`}
                  >
                    <img src={img.imageUrl} alt="" className="w-full h-full object-cover" />
                  </button>
                ))}
              </div>
            )}
          </motion.div>

          {/* Product Info */}
          <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }}>
            <div className="mb-8">
              <p className="text-primary-600 font-bold mb-2 uppercase tracking-wide text-sm">{product.brandName || product.categoryName}</p>
              <h1 className="text-3xl sm:text-4xl font-extrabold text-gray-900 mb-4 leading-tight">{product.name}</h1>
              
              <div className="flex items-end gap-4 mb-6">
                {product.discountPrice ? (
                  <>
                    <p className="text-4xl font-black text-gray-900">Rs. {product.discountPrice.toLocaleString()}</p>
                    <p className="text-xl text-gray-400 line-through mb-1">Rs. {product.unitPrice.toLocaleString()}</p>
                    <div className="bg-red-100 text-red-700 px-3 py-1 rounded-full text-sm font-bold mb-1">
                      {Math.round(((product.unitPrice - product.discountPrice) / product.unitPrice) * 100)}% OFF
                    </div>
                  </>
                ) : (
                  <p className="text-4xl font-black text-gray-900">Rs. {product.unitPrice.toLocaleString()}</p>
                )}
              </div>

              <p className="text-gray-600 leading-relaxed mb-8">{product.description}</p>
            </div>

            <div className="grid grid-cols-2 gap-4 mb-8">
              <div className="flex items-center space-x-3 p-4 bg-gray-50 rounded-2xl border border-gray-100">
                <div className="p-2 bg-white rounded-xl shadow-sm"><Truck className="w-6 h-6 text-primary-600" /></div>
                <div>
                  <p className="font-bold text-gray-900 text-sm">Delivery Time</p>
                  <p className="text-xs text-gray-500">{product.deliveryPeriod || '3-5 business days'}</p>
                </div>
              </div>
              <div className="flex items-center space-x-3 p-4 bg-gray-50 rounded-2xl border border-gray-100">
                <div className="p-2 bg-white rounded-xl shadow-sm"><Shield className="w-6 h-6 text-primary-600" /></div>
                <div>
                  <p className="font-bold text-gray-900 text-sm">Warranty</p>
                  <p className="text-xs text-gray-500">{product.warrantyInfo || 'Standard Warranty'}</p>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-4 mb-10">
              {user?.role !== 'Salesman' ? (
                <motion.button
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  onClick={handleAddToCart}
                  disabled={product.stockQuantity <= 0 || cartLoading}
                  className={`flex-1 flex items-center justify-center space-x-2 py-4 rounded-xl font-bold text-lg transition-all ${
                    product.stockQuantity > 0 ? 'bg-primary-600 hover:bg-primary-700 text-white shadow-lg shadow-primary-500/30' : 'bg-gray-200 text-gray-500 cursor-not-allowed'
                  }`}
                >
                  {cartLoading
                    ? <span className="w-6 h-6 border-2 border-white/50 border-t-white rounded-full animate-spin" />
                    : <ShoppingCart className="w-6 h-6" />
                  }
                  <span>{product.stockQuantity > 0 ? 'Add to Cart' : 'Out of Stock'}</span>
                </motion.button>
              ) : (
                <div className="flex-1 py-4 px-6 rounded-xl bg-gray-100 text-gray-600 font-bold text-center">
                  Shopping not available for Salesmen
                </div>
              )}
              
              <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={handleAddToWishlist}
                disabled={wishLoading}
                className="p-4 rounded-xl bg-gray-50 border border-gray-200 text-gray-600 hover:text-red-500 hover:bg-red-50 hover:border-red-100 transition-all disabled:opacity-50"
              >
                <Heart className="w-6 h-6" />
              </motion.button>
            </div>

            {/* Specifications Table */}
            <div className="border-t border-gray-200 pt-8">
              <h3 className="text-xl font-bold text-gray-900 mb-6">Product Specifications</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-4">
                {[
                  { label: 'Brand', value: product.brandName || product.supplierName },
                  { label: 'Model', value: product.model || 'N/A' },
                  { label: 'SKU', value: product.sku },
                  { label: 'Weight', value: product.weight || 'N/A' },
                  { label: 'Dimensions', value: product.dimensions || 'N/A' },
                  { label: 'Tax', value: `${product.taxPercentage || 0}%` },
                  { label: 'Stock', value: product.stockQuantity > 0 ? `${product.stockQuantity} units` : 'Out of Stock' },
                  { label: 'Views', value: (product.viewCount || 0).toString() }
                ].filter(s => s.value).map((spec, index) => (
                  <div key={index} className="flex justify-between items-center py-2 border-b border-gray-100">
                    <span className="text-gray-500">{spec.label}</span>
                    <span className={`font-semibold text-right ${
                      spec.label === 'Stock' && product.stockQuantity === 0 ? 'text-red-500' : 'text-gray-900'
                    }`}>{spec.value}</span>
                  </div>
                ))}
              </div>
            </div>

          </motion.div>
        </div>
      </div>
    </div>
  );
};

export default ProductDetail;

