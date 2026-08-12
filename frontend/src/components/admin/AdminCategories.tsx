import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, Plus, Edit, Trash2, LayoutGrid, X, CheckCircle, AlertCircle } from 'lucide-react';
import { CategoryApi } from '../../api/category.api';

interface AdminCategoriesProps {
  categories: any[];
  onCategoriesUpdated: () => void;
}

const emptyForm = { name: '', description: '', imageUrl: '' };

const AdminCategories: React.FC<AdminCategoriesProps> = ({ categories, onCategoriesUpdated }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editCat, setEditCat] = useState<any | null>(null);
  const [form, setForm] = useState({ ...emptyForm });
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 4000);
  };

  const openAdd = () => {
    setEditCat(null);
    setForm({ ...emptyForm });
    setShowModal(true);
  };

  const openEdit = (cat: any) => {
    setEditCat(cat);
    setForm({ name: cat.name || '', description: cat.description || '', imageUrl: cat.imageUrl || '' });
    setShowModal(true);
  };

  const closeModal = () => { setShowModal(false); setEditCat(null); };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim()) { showMsg('error', 'Category name is required.'); return; }
    setSaving(true);
    try {
      const payload = { name: form.name.trim(), description: form.description || undefined };
      if (editCat) {
        await CategoryApi.update(editCat.id, payload);
        showMsg('success', `Category "${form.name}" updated.`);
      } else {
        await CategoryApi.create(payload);
        showMsg('success', `Category "${form.name}" created.`);
      }
      closeModal();
      onCategoriesUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to save category.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Delete category "${name}"? Products in this category may be affected.`)) return;
    try {
      await CategoryApi.delete(id);
      showMsg('success', `Category "${name}" deleted.`);
      onCategoriesUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to delete category. It may have associated products.');
    }
  };

  const filteredCategories = categories.filter(c =>
    c.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    c.description?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
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
              placeholder="Search categories..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input-field pl-10"
            />
          </div>
          <button onClick={openAdd} className="btn-primary flex items-center space-x-2">
            <Plus className="w-5 h-5" />
            <span>Add Category</span>
          </button>
        </div>

        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredCategories.map((cat) => (
            <motion.div
              key={cat.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="border border-gray-200 rounded-xl p-4 hover:border-primary-200 hover:shadow-sm transition-all"
            >
              <div className="flex justify-between items-start">
                <div className="flex items-center gap-3">
                  {cat.imageUrl ? (
                    <img src={cat.imageUrl} alt={cat.name} className="w-10 h-10 rounded-lg object-cover" />
                  ) : (
                    <div className="w-10 h-10 rounded-lg bg-primary-50 flex items-center justify-center">
                      <LayoutGrid className="w-5 h-5 text-primary-400" />
                    </div>
                  )}
                  <div>
                    <p className="font-semibold text-gray-900">{cat.name}</p>
                    <p className="text-xs text-gray-500 line-clamp-1">{cat.description || 'No description'}</p>
                  </div>
                </div>
                <div className="flex gap-1 ml-2">
                  <button
                    onClick={() => openEdit(cat)}
                    className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(cat.id, cat.name)}
                    className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </motion.div>
          ))}
        </div>

        {filteredCategories.length === 0 && (
          <div className="py-12 text-center">
            <LayoutGrid className="w-12 h-12 mx-auto mb-3 text-gray-300" />
            <p className="text-gray-500">No categories found. Add your first category.</p>
          </div>
        )}
      </div>

      {/* Add/Edit Modal */}
      <AnimatePresence>
        {showModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl w-full max-w-md shadow-2xl"
            >
              <div className="flex justify-between items-center p-5 border-b border-gray-100 bg-gray-50 rounded-t-2xl">
                <h3 className="text-xl font-bold text-gray-900">{editCat ? 'Edit Category' : 'Add Category'}</h3>
                <button onClick={closeModal} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
              </div>
              <form onSubmit={handleSubmit} className="p-5 space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Category Name *</label>
                  <input name="name" value={form.name} onChange={handleChange} required className="input-field py-2" placeholder="e.g. Electronics" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <textarea name="description" value={form.description} onChange={handleChange} className="input-field py-2 resize-none h-20" placeholder="Category description..." />
                </div>
                <div className="flex justify-end gap-3 pt-2 border-t border-gray-100">
                  <button type="button" onClick={closeModal} className="px-5 py-2.5 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-xl transition-colors">Cancel</button>
                  <button type="submit" disabled={saving} className="btn-primary px-6 py-2.5">
                    {saving ? 'Saving...' : editCat ? 'Update Category' : 'Create Category'}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default AdminCategories;
