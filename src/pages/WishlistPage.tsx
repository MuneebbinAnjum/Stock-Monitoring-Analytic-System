import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Heart, Trash2, ShoppingCart } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { WishlistApi } from '../api/wishlist.api';
import { CartApi } from '../api/cart.api';

const WishlistPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  const loadWishlist = async () => {
    try {
      const data = await WishlistApi.getWishlist();
      setItems(data || []);
    } catch { } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadWishlist();
  }, []);

  const removeItem = async (productId: string) => {
    try {
      await WishlistApi.removeItem(productId);
      setItems(items.filter(i => i.productId !== productId));
    } catch { }
  };

  const addToCart = async (productId: string) => {
    try {
      await CartApi.addItem({ productId, quantity: 1 });
      alert('Added to cart!');
    } catch {
      alert('Failed to add to cart or out of stock');
    }
  };

  if (loading) return <div className="page-container py-20 text-center">Loading wishlist...</div>;

  return (
    <div className="page-container min-h-screen py-12">
      <div className="max-w-5xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-900 mb-8 flex items-center space-x-3">
          <Heart className="w-8 h-8 text-red-500" />
          <span>My Wishlist</span>
        </h1>

        {items.length === 0 ? (
          <div className="card text-center py-16">
            <Heart className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-gray-700 mb-2">Your wishlist is empty</h2>
            <p className="text-gray-500 mb-6">Save items you love here.</p>
            <button onClick={() => navigate('/')} className="btn-primary">
              Discover Products
            </button>
          </div>
        ) : (
          <div className="grid-responsive">
            {items.map((item, index) => (
              <motion.div
                key={item.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.05 }}
                className="card group flex flex-col"
              >
                <div className="relative h-48 bg-gray-100 rounded-lg overflow-hidden mb-4 cursor-pointer" onClick={() => navigate(`/product/${item.productId}`)}>
                  <img
                    src={item.productImage || 'https://via.placeholder.com/300'}
                    alt={item.productName}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300"
                  />
                  <button
                    onClick={(e) => { e.stopPropagation(); removeItem(item.productId); }}
                    className="absolute top-2 right-2 p-2 bg-white/80 backdrop-blur rounded-full text-red-500 hover:bg-red-50 hover:text-red-600 transition-colors"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
                
                <div className="flex-1">
                  <h3 className="font-bold text-gray-900 mb-1 line-clamp-2">{item.productName}</h3>
                  <p className="text-primary-600 font-bold mb-4">Rs. {item.unitPrice.toLocaleString()}</p>
                </div>

                <button
                  onClick={() => addToCart(item.productId)}
                  disabled={!item.inStock}
                  className={`w-full py-2.5 rounded-lg font-semibold flex items-center justify-center space-x-2 transition-all ${
                    item.inStock ? 'bg-primary-600 hover:bg-primary-700 text-white' : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                  }`}
                >
                  <ShoppingCart className="w-4 h-4" />
                  <span>{item.inStock ? 'Add to Cart' : 'Out of Stock'}</span>
                </button>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default WishlistPage;
