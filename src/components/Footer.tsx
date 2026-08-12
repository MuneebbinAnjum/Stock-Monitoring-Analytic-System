import React from 'react';
import { Link } from 'react-router-dom';
import { Mail, Phone } from 'lucide-react';

const Footer: React.FC = () => {
  return (
    <footer className="relative mt-20 overflow-hidden" style={{
      background: 'linear-gradient(180deg, #0f172a 0%, #1e293b 100%)',
    }}>
      {/* Top gradient line */}
      <div className="h-px w-full" style={{
        background: 'linear-gradient(90deg, transparent 0%, rgba(14, 165, 233, 0.4) 50%, transparent 100%)',
      }} />

      <div className="max-w-7xl mx-auto px-4 py-14">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-10 mb-12">
          {/* About */}
          <div className="md:col-span-1">
            <div className="flex items-center space-x-3 mb-4">
              <div className="w-8 h-8 rounded-lg flex items-center justify-center" style={{
                background: 'linear-gradient(135deg, #0284c7 0%, #7c3aed 100%)',
              }}>
                <span className="text-white font-bold text-xs">S</span>
              </div>
              <span className="text-lg font-bold text-white tracking-tight">SMAS</span>
            </div>
            <p className="text-gray-400 text-sm leading-relaxed">
              Complete inventory and sales management system with real-time data sharing across all platforms.
            </p>
          </div>

          {/* Quick Links */}
          <div>
            <h3 className="text-sm font-semibold text-gray-300 uppercase tracking-wider mb-4">Quick Links</h3>
            <ul className="space-y-2.5">
              <li>
                <Link to="/" className="text-sm text-gray-400 hover:text-white transition-colors duration-200">
                  Products
                </Link>
              </li>
              <li>
                <Link to="/order-tracking" className="text-sm text-gray-400 hover:text-white transition-colors duration-200">
                  Track Order
                </Link>
              </li>
              <li>
                <a href="#" className="text-sm text-gray-400 hover:text-white transition-colors duration-200">
                  About Us
                </a>
              </li>
              <li>
                <a href="#" className="text-sm text-gray-400 hover:text-white transition-colors duration-200">
                  Contact Us
                </a>
              </li>
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h3 className="text-sm font-semibold text-gray-300 uppercase tracking-wider mb-4">Contact</h3>
            <ul className="space-y-3">
              <li className="flex items-center space-x-3">
                <div className="p-1.5 rounded-lg bg-white/5">
                  <Phone className="w-3.5 h-3.5 text-gray-400" />
                </div>
                <span className="text-sm text-gray-400">+92 300 123 4567</span>
              </li>
              <li className="flex items-center space-x-3">
                <div className="p-1.5 rounded-lg bg-white/5">
                  <Mail className="w-3.5 h-3.5 text-gray-400" />
                </div>
                <span className="text-sm text-gray-400">support@smas.pk</span>
              </li>
            </ul>
          </div>

          {/* Newsletter */}
          <div>
            <h3 className="text-sm font-semibold text-gray-300 uppercase tracking-wider mb-4">Stay Updated</h3>
            <p className="text-sm text-gray-400 mb-3">Get notified about new features and updates.</p>
            <div className="flex">
              <input
                type="email"
                placeholder="Enter your email"
                className="flex-1 px-3 py-2 bg-white/5 border border-white/10 rounded-l-lg text-sm text-white placeholder-gray-500 outline-none focus:border-primary-500/50 transition-colors"
              />
              <button className="px-4 py-2 rounded-r-lg text-sm font-semibold text-white transition-colors" style={{
                background: 'linear-gradient(135deg, #0284c7 0%, #7c3aed 100%)',
              }}>
                Join
              </button>
            </div>
          </div>
        </div>

        {/* Divider */}
        <div className="border-t border-white/5 pt-8">
          <div className="flex flex-col md:flex-row justify-between items-center text-sm">
            <p className="text-gray-500">&copy; {new Date().getFullYear()} SMAS — Stock Monitoring and Analytic System. All rights reserved.</p>
            <div className="flex space-x-6 mt-4 md:mt-0">
              <a href="#" className="text-gray-500 hover:text-gray-300 transition-colors">
                Privacy Policy
              </a>
              <a href="#" className="text-gray-500 hover:text-gray-300 transition-colors">
                Terms of Service
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
