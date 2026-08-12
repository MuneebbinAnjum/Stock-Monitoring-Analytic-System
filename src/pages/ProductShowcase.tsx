import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, ShoppingCart, Heart, Filter, ArrowDownUp, CheckCircle, AlertCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { ProductApi } from '../api/product.api';
import { CategoryApi } from '../api/category.api';
import { CartApi } from '../api/cart.api';
import { WishlistApi } from '../api/wishlist.api';
import { useAuth } from '../context/AuthContext';
import { Product } from '../types';

const ProductShowcase: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [sortBy, setSortBy] = useState<string>('newest');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [toast, setToast] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null); // productId being acted upon

  const showToast = (type: 'success' | 'error', text: string) => {
    setToast({ type, text });
    setTimeout(() => setToast(null), 3000);
  };

  useEffect(() => {
    const loadData = async () => {
      try {
        const [prodData, catData] = await Promise.all([
          ProductApi.getAll().catch(() => []),
          CategoryApi.getAll().catch(() => [])
        ]);
        setProducts(prodData || []);
        setCategories([{ id: 'All', name: 'All Categories' }, ...(catData || [])]);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load products');
      } finally {
        setLoading(false);
      }
    };
    loadData();

    const handleInventory = (e: any) => {
      // refresh product list when inventory changes
      loadData();
    };

    window.addEventListener('InventoryUpdated', handleInventory);
    window.addEventListener('StockAlertCreated', handleInventory);
    return () => {
      window.removeEventListener('InventoryUpdated', handleInventory);
      window.removeEventListener('StockAlertCreated', handleInventory);
    };
  }, []);

  const handleAddToCart = async (e: React.MouseEvent, product: Product) => {
    e.stopPropagation();
    if (product.stockQuantity <= 0) return;
    setActionLoading(`cart-${product.id}`);
    try {
      await CartApi.addItem({ productId: product.id, quantity: 1 });
      showToast('success', `"${product.name}" added to cart!`);
    } catch (err: any) {
      if (err.response?.status === 401) {
        navigate('/login');
      } else {
        showToast('error', err.response?.data?.message || 'Failed to add to cart');
      }
    } finally {
      setActionLoading(null);
    }
  };

  const handleAddToWishlist = async (e: React.MouseEvent, product: Product) => {
    e.stopPropagation();
    setActionLoading(`wish-${product.id}`);
    try {
      await WishlistApi.addItem(product.id);
      showToast('success', `"${product.name}" added to wishlist!`);
    } catch (err: any) {
      if (err.response?.status === 401) {
        navigate('/login');
      } else {
        showToast('error', err.response?.data?.message || 'Failed to add to wishlist');
      }
    } finally {
      setActionLoading(null);
    }
  };

  let filteredProducts = products.filter((product) =>
    (selectedCategory === 'All' || product.categoryId === selectedCategory || product.categoryName === selectedCategory) &&
    (product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (product.brandName ?? '').toLowerCase().includes(searchQuery.toLowerCase()) ||
      (product.tags ?? '').toLowerCase().includes(searchQuery.toLowerCase()))
  );

  if (sortBy === 'price_asc') {
    filteredProducts = [...filteredProducts].sort((a, b) => (a.discountPrice || a.unitPrice) - (b.discountPrice || b.unitPrice));
  } else if (sortBy === 'price_desc') {
    filteredProducts = [...filteredProducts].sort((a, b) => (b.discountPrice || b.unitPrice) - (a.discountPrice || a.unitPrice));
  }
  // 'newest' = default API order (latest first)

  return (
    <div className="page-container min-h-screen py-8">
      {/* Toast notification */}
      <AnimatePresence>
        {toast && (
          <motion.div
            initial={{ opacity: 0, y: -20, x: '-50%' }}
            animate={{ opacity: 1, y: 0, x: '-50%' }}
            exit={{ opacity: 0, y: -20, x: '-50%' }}
            className={`fixed top-20 left-1/2 z-50 flex items-center gap-3 px-5 py-3 rounded-2xl shadow-xl ${
              toast.type === 'success'
                ? 'bg-emerald-600 text-white'
                : 'bg-red-600 text-white'
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

      <div className="max-w-7xl mx-auto px-4">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-12 text-center"
        >
          <h1 className="section-title">Discover Our Products</h1>
          <p className="section-subtitle">
            Quality products at competitive prices, delivered fast to your doorstep
          </p>
        </motion.div>

        {/* Filters and Search Bar */}
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="card mb-8 p-4 flex flex-col md:flex-row items-center gap-4"
        >
          <div className="relative flex-1 w-full">
            <Search className="absolute left-4 top-3.5 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search products by name, brand, or tag..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="input-field pl-12 py-3 bg-gray-50 border-gray-200"
            />
          </div>
          <div className="flex gap-4 w-full md:w-auto">
            <div className="relative flex-1 md:w-48">
              <Filter className="absolute left-3 top-3.5 w-5 h-5 text-gray-400 pointer-events-none" />
              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                className="input-field pl-10 py-3 bg-gray-50 border-gray-200 cursor-pointer appearance-none"
              >
                {categories.map(c => (
                  <option key={c.id} value={c.id === 'All' ? 'All' : c.name}>{c.name}</option>
                ))}
              </select>
            </div>
            <div className="relative flex-1 md:w-48">
              <ArrowDownUp className="absolute left-3 top-3.5 w-5 h-5 text-gray-400 pointer-events-none" />
              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                className="input-field pl-10 py-3 bg-gray-50 border-gray-200 cursor-pointer appearance-none"
              >
                <option value="newest">Newest First</option>
                <option value="price_asc">Price: Low to High</option>
                <option value="price_desc">Price: High to Low</option>
              </select>
            </div>
          </div>
        </motion.div>

        {loading ? (
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-5">
            {[...Array(8)].map((_, i) => (
              <div key={i} className="card animate-pulse">
                <div className="h-48 bg-gray-200 rounded-lg mb-4" />
                <div className="h-4 bg-gray-200 rounded mb-2 w-2/3" />
                <div className="h-4 bg-gray-200 rounded w-1/2" />
              </div>
            ))}
          </div>
        ) : error ? (
          <div className="text-center py-20 text-red-600">{error}</div>
        ) : (
          <>
            <div className="grid-responsive">
              {filteredProducts.map((product, index) => (
                <motion.div
                  key={product.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: Math.min(index * 0.04, 0.5) }}
                  onClick={() => navigate(`/product/${product.id}`)}
                  className="card card-hover group flex flex-col cursor-pointer"
                >
                  <div className="relative h-56 bg-gray-100 rounded-lg overflow-hidden mb-4">
                    <img
                      src={product.productImages?.[0]?.imageUrl || `https://via.placeholder.com/300x300?text=${encodeURIComponent(product.name)}`}
                      alt={product.name}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                      loading="lazy"
                    />
                    {product.discountPrice && (
                      <div className="absolute top-3 left-3 bg-red-500 text-white px-2.5 py-1 rounded text-xs font-bold shadow-sm">
                        {Math.round(((product.unitPrice - product.discountPrice) / product.unitPrice) * 100)}% OFF
                      </div>
                    )}
                    {product.stockQuantity === 0 && (
                      <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
                        <span className="bg-white text-gray-800 text-xs font-bold px-3 py-1 rounded-full">Out of Stock</span>
                      </div>
                    )}
                    <button
                      onClick={(e) => handleAddToWishlist(e, product)}
                      disabled={actionLoading === `wish-${product.id}`}
                      className="absolute top-3 right-3 p-2 bg-white/90 backdrop-blur rounded-full text-gray-400 hover:text-red-500 hover:bg-white transition-colors shadow-sm"
                    >
                      <Heart className="w-4 h-4" />
                    </button>
                  </div>

                  <div className="flex-1 flex flex-col">
                    <p className="text-xs text-primary-600 font-semibold mb-1 uppercase tracking-wider">{product.brandName || product.categoryName}</p>
                    <h3 className="font-bold text-gray-900 mb-2 line-clamp-2 leading-tight">{product.name}</h3>

                    <div className="mt-auto pt-4 flex items-end justify-between">
                      <div>
                        {product.discountPrice ? (
                          <>
                            <p className="text-xl font-bold text-gray-900">Rs. {product.discountPrice.toLocaleString()}</p>
                            <p className="text-xs text-gray-500 line-through">Rs. {product.unitPrice.toLocaleString()}</p>
                          </>
                        ) : (
                          <p className="text-xl font-bold text-gray-900">Rs. {product.unitPrice.toLocaleString()}</p>
                        )}
                      </div>
                      {user?.role !== 'Salesman' ? (
                        <button
                          disabled={product.stockQuantity <= 0 || actionLoading === `cart-${product.id}`}
                          onClick={(e) => handleAddToCart(e, product)}
                          className={`p-2.5 rounded-lg flex items-center justify-center transition-colors shadow-sm ${
                            product.stockQuantity > 0
                              ? 'bg-primary-600 hover:bg-primary-700 text-white'
                              : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                          }`}
                          title={product.stockQuantity > 0 ? 'Add to Cart' : 'Out of Stock'}
                        >
                          {actionLoading === `cart-${product.id}`
                            ? <span className="w-5 h-5 border-2 border-white/50 border-t-white rounded-full animate-spin" />
                            : <ShoppingCart className="w-5 h-5" />
                          }
                        </button>
                      ) : (
                        <div className="text-xs bg-gray-100 text-gray-600 px-2 py-1.5 rounded-lg font-medium">
                          N/A for Salesmen
                        </div>
                      )}
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>

            {filteredProducts.length === 0 && (
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                className="card text-center py-16 mt-8"
              >
                <Search className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-1">No products found</h3>
                <p className="text-gray-500">Try adjusting your search or filters to find what you're looking for.</p>
                <button
                  onClick={() => { setSearchQuery(''); setSelectedCategory('All'); }}
                  className="mt-6 text-primary-600 font-medium hover:text-primary-700"
                >
                  Clear all filters
                </button>
              </motion.div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default ProductShowcase;
