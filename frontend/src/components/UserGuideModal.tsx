import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { X, Info as HelpCircle, Lock, ShoppingCart, TrendingUp, Users, AlertCircle, Settings, Package, Activity } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

interface UserGuideModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const UserGuideModal: React.FC<UserGuideModalProps> = ({ isOpen, onClose }) => {
  const { user } = useAuth();

  const guides = {
    Admin: {
      title: 'Admin Portal Guide',
      description: 'System administration and business management',
      features: [
        {
          icon: ShoppingCart,
          title: 'Order Management',
          description: 'View, approve, and process all orders. Track order status from pending to delivery.'
        },
        {
          icon: Package,
          title: 'Product Management',
          description: 'Add, edit, and manage products. Set prices, discounts, and inventory levels.'
        },
        {
          icon: Users,
          title: 'Employee Management',
          description: 'Approve salesman registrations, set commissions and salaries, track performance.'
        },
        {
          icon: TrendingUp,
          title: 'Analytics & Reports',
          description: 'View real-time charts, revenue trends, sales by location, and employee performance.'
        },
        {
          icon: AlertCircle,
          title: 'Complaint Management',
          description: 'View and respond to customer complaints. Track complaint resolution.'
        },
        {
          icon: Settings,
          title: 'System Settings',
          description: 'Manage system configurations, discounts, and business settings.'
        }
      ],
      quickActions: [
        'Dashboard → See real-time business metrics',
        'Orders Tab → Approve/process customer orders',
        'Products Tab → Manage inventory and pricing',
        'Employees Tab → Approve salesmen and set commissions',
        'Reports → Export data in CSV/Excel',
        'Audit Logs → Track all system changes'
      ]
    },
    Salesman: {
      title: 'Salesman Portal Guide',
      description: 'Sales order creation and commission tracking',
      features: [
        {
          icon: ShoppingCart,
          title: 'Create Orders',
          description: 'Create physical (walk-in) orders for customers. System automatically deducts inventory.'
        },
        {
          icon: TrendingUp,
          title: 'Track Commission',
          description: 'View your earned commissions based on products sold. Commission percentage set by admin.'
        },
        {
          icon: Package,
          title: 'View Products',
          description: 'Browse complete product catalog with prices and availability.'
        },
        {
          icon: Activity,
          title: 'View Your Orders',
          description: 'Track all orders you created and their current status.'
        },
        {
          icon: Lock,
          title: 'Approval Status',
          description: 'You must be approved by admin before you can log in and create orders.'
        }
      ],
      quickActions: [
        'NOTE: You cannot log in until admin approves your registration',
        'Once approved, you can create new customer orders',
        'View your commission earnings per month',
        'Track order fulfillment status',
        'Cannot use shopping cart (online ordering disabled)'
      ]
    },
    Buyer: {
      title: 'Buyer Portal Guide',
      description: 'Online shopping and order tracking',
      features: [
        {
          icon: ShoppingCart,
          title: 'Shopping Cart',
          description: 'Browse products, add to cart, and checkout. Automatic discount calculation.'
        },
        {
          icon: TrendingUp,
          title: 'Order Tracking',
          description: 'Track your orders from placement to delivery. View order status and details.'
        },
        {
          icon: Package,
          title: 'Order History',
          description: 'View all your past orders with detailed information and invoices.'
        },
        {
          icon: AlertCircle,
          title: 'File Complaints',
          description: 'Report issues with products or delivery. Communicate with admin about concerns.'
        },
        {
          icon: Users,
          title: 'Wishlist',
          description: 'Save favorite products for later purchase.'
        },
        {
          icon: Lock,
          title: 'Account Security',
          description: 'Change password anytime. Secure checkout with encrypted sessions.'
        }
      ],
      quickActions: [
        'Dashboard → View your recent orders',
        'Products → Browse and search for items',
        'Add to Cart → Build your order',
        'Checkout → Review total with tax and delivery charges',
        'Order Tracking → Check delivery status',
        'My Complaints → File and track issues'
      ]
    }
  };

  const roleGuide = user?.role ? guides[user.role as keyof typeof guides] : null;

  if (!isOpen) return null;

  const GuideIcon = {
    Admin: Lock,
    Salesman: TrendingUp,
    Buyer: ShoppingCart
  }[user?.role || 'Buyer'] || HelpCircle;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.2 }}
        className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto"
      >
        {/* Header */}
        <div className="sticky top-0 bg-gradient-to-r from-primary-600 to-primary-700 text-white p-6 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-white/20 rounded-lg">
              <GuideIcon className="w-6 h-6" />
            </div>
            <div>
              <h2 className="text-2xl font-bold">{roleGuide?.title}</h2>
              <p className="text-sm text-white/80">{roleGuide?.description}</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-white/20 rounded-lg transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Features Grid */}
          <div>
            <h3 className="text-lg font-bold text-gray-900 mb-4">Available Features</h3>
            <div className="grid md:grid-cols-2 gap-4">
              {roleGuide?.features.map((feature, idx) => {
                const Icon = feature.icon;
                return (
                  <motion.div
                    key={idx}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: idx * 0.05 }}
                    className="p-4 border border-gray-200 rounded-xl hover:border-primary-400 hover:bg-primary-50 transition-all"
                  >
                    <div className="flex gap-3">
                      <div className="p-2 bg-primary-100 rounded-lg h-fit">
                        <Icon className="w-5 h-5 text-primary-600" />
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-900">{feature.title}</h4>
                        <p className="text-sm text-gray-600 mt-1">{feature.description}</p>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          </div>

          {/* Quick Actions */}
          <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
            <h3 className="font-bold text-blue-900 mb-3 flex items-center gap-2">
              <TrendingUp className="w-5 h-5" />
              Quick Start Guide
            </h3>
            <ul className="space-y-2">
              {roleGuide?.quickActions.map((action, idx) => (
                <li key={idx} className="text-sm text-blue-800 flex gap-2">
                  <span className="text-blue-600 font-bold">•</span>
                  <span>{action}</span>
                </li>
              ))}
            </ul>
          </div>

          {/* Common Actions */}
          <div className="bg-amber-50 border border-amber-200 rounded-xl p-4">
            <h3 className="font-bold text-amber-900 mb-3">Available for All Users</h3>
            <ul className="space-y-2 text-sm text-amber-800">
              <li className="flex gap-2">
                <span className="text-amber-600 font-bold">•</span>
                <span><strong>Change Password:</strong> Go to profile and change your password anytime</span>
              </li>
              <li className="flex gap-2">
                <span className="text-amber-600 font-bold">•</span>
                <span><strong>View Notifications:</strong> Click the notification bell to see all updates</span>
              </li>
              <li className="flex gap-2">
                <span className="text-amber-600 font-bold">•</span>
                <span><strong>Logout:</strong> Click your profile menu to logout securely</span>
              </li>
              {user?.role === 'Admin' && (
                <>
                  <li className="flex gap-2">
                    <span className="text-amber-600 font-bold">•</span>
                    <span><strong>View History:</strong> Notifications tab shows all past notifications</span>
                  </li>
                  <li className="flex gap-2">
                    <span className="text-amber-600 font-bold">•</span>
                    <span><strong>Audit Logs:</strong> Track all user activities and system changes</span>
                  </li>
                </>
              )}
            </ul>
          </div>
        </div>

        {/* Footer */}
        <div className="sticky bottom-0 bg-gray-50 border-t border-gray-200 p-4 flex justify-end gap-3">
          <button
            onClick={onClose}
            className="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
          >
            Close
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default UserGuideModal;
