import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/client';
import { motion } from 'framer-motion';

const AdminSettings: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string>('');
  const [form, setForm] = useState({
    company_name: '',
    company_phone: '',
    company_email: '',
    tax_percentage: '',
  });

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        const res = await api.get('/api/settings');
        // The backend returns { success: true, data: [ { key, value }, ... ] }
        const settingsArray = res.data.data || [];
        
        const getVal = (key: string) => settingsArray.find((s: any) => s.key === key)?.value || '';

        setForm({
          company_name: getVal('company_name'),
          company_phone: getVal('company_phone'),
          company_email: getVal('company_email'),
          tax_percentage: getVal('tax_percentage'),
        });
      } catch (e) {
        setError('Failed to load settings');
      } finally {
        setLoading(false);
      }
    };
    fetchSettings();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      // Create a PUT request for each setting
      const promises = Object.entries(form).map(([key, value]) => {
        return api.put(`/api/settings/${key}`, { value });
      });

      await Promise.all(promises);
      
      // Navigate back to dashboard upon successful save
      navigate('/admin/dashboard');
    } catch (e) {
      setError('Failed to save settings. Please ensure all settings exist in the system.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="p-8">Loading settings…</div>;
  }

  return (
    <motion.div
      className="max-w-2xl mx-auto p-8"
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -10 }}
    >
      <h1 className="text-2xl font-bold mb-6">Company Settings</h1>
      {error && <div className="text-red-600 mb-4">{error}</div>}
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1" htmlFor="company_name">
            Company Name
          </label>
          <input
            id="company_name"
            name="company_name"
            type="text"
            className="w-full rounded border-gray-300 focus:border-primary-500 focus:ring-primary-500"
            value={form.company_name}
            onChange={handleChange}
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1" htmlFor="company_phone">
            Phone Number
          </label>
          <input
            id="company_phone"
            name="company_phone"
            type="tel"
            className="w-full rounded border-gray-300 focus:border-primary-500 focus:ring-primary-500"
            value={form.company_phone}
            onChange={handleChange}
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1" htmlFor="company_email">
            Company Email
          </label>
          <input
            id="company_email"
            name="company_email"
            type="email"
            className="w-full rounded border-gray-300 focus:border-primary-500 focus:ring-primary-500"
            value={form.company_email}
            onChange={handleChange}
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1" htmlFor="tax_percentage">
            Tax Percentage
          </label>
          <input
            id="tax_percentage"
            name="tax_percentage"
            type="number"
            step="0.01"
            className="w-full rounded border-gray-300 focus:border-primary-500 focus:ring-primary-500"
            value={form.tax_percentage}
            onChange={handleChange}
            required
          />
        </div>
        <div className="flex space-x-4 mt-6">
          <button
            type="submit"
            disabled={saving}
            className="px-4 py-2 bg-primary-600 text-white rounded hover:bg-primary-700 transition"
          >
            {saving ? 'Saving…' : 'Save Changes'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/admin/dashboard')}
            className="px-4 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300 transition"
          >
            Cancel
          </button>
        </div>
      </form>
    </motion.div>
  );
};

export default AdminSettings;
