import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, Plus, Edit, Trash2, Package, X, CheckCircle, AlertCircle } from 'lucide-react';
import { ProductApi } from '../../api/product.api';

interface AdminProductsProps {
  products: any[];
  categories?: any[];
  onProductsUpdated: () => void;
}

const emptyForm = {
  name: '',
  sku: '',
  categoryId: '',
  unitPrice: '',
  purchasePrice: '',
  discountPrice: '',
  stockQuantity: '',
  reorderLevel: '10',
  description: '',
  brandName: '',
  companyName: '',
  model: '',
  deliveryPeriod: '3-5 business days',
  warrantyInfo: '',
  weight: '',
  dimensions: '',
  tags: '',
  taxPercentage: '0',
  imageUrls: '' // comma-separated
};

const AdminProducts: React.FC<AdminProductsProps> = ({ products, categories = [], onProductsUpdated }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editProduct, setEditProduct] = useState<any | null>(null);
  const [form, setForm] = useState({ ...emptyForm });
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 4000);
  };

  const openAdd = () => {
    setEditProduct(null);
    setForm({ ...emptyForm });
    setShowModal(true);
  };

  const openEdit = (product: any) => {
    setEditProduct(product);
    setForm({
      name: product.name || '',
      sku: product.sku || '',
      categoryId: product.categoryId || '',
      unitPrice: product.unitPrice?.toString() || '',
      purchasePrice: product.purchasePrice?.toString() || '',
      discountPrice: product.discountPrice?.toString() || '',
      stockQuantity: product.stockQuantity?.toString() || '',
      reorderLevel: product.reorderLevel?.toString() || '10',
      description: product.description || '',
      brandName: product.brandName || '',
      companyName: product.companyName || '',
      model: product.model || '',
      deliveryPeriod: product.deliveryPeriod || '3-5 business days',
      warrantyInfo: product.warrantyInfo || '',
      weight: product.weight || '',
      dimensions: product.dimensions || '',
      tags: product.tags || '',
      taxPercentage: product.taxPercentage?.toString() || '0',
      imageUrls: product.productImages?.map((i: any) => i.imageUrl).join(', ') || ''
    });
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditProduct(null);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

    const isGuid = (val: string) => {
      const guidRegex = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/
      return guidRegex.test(val)
    }

    const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name || !form.sku || !form.unitPrice || !form.stockQuantity || !form.categoryId) {
      showMsg('error', 'Name, SKU, Category, Unit Price, and Stock are required.');
      return;
    }

    setSaving(true);
    try {
      // Validate categoryId is a proper GUID string
      if (!isGuid(form.categoryId)) {
        showMsg('error', 'Please select a valid category.');
        setSaving(false);
        return;
      }

      const payload: any = {
        name: form.name.trim(),
        sku: form.sku.trim(),
        categoryId: form.categoryId,
        unitPrice: parseFloat(form.unitPrice),
        purchasePrice: parseFloat(form.purchasePrice || '0'),
        discountPrice: form.discountPrice ? parseFloat(form.discountPrice) : undefined,
        stockQuantity: parseInt(form.stockQuantity),
        reorderLevel: parseInt(form.reorderLevel || '10'),
        description: form.description || undefined,
        brandName: form.brandName || undefined,
        companyName: form.companyName || undefined,
        model: form.model || undefined,
        deliveryPeriod: form.deliveryPeriod || '3-5 business days',
        warrantyInfo: form.warrantyInfo || undefined,
        weight: form.weight || undefined,
        dimensions: form.dimensions || undefined,
        tags: form.tags || undefined,
        taxPercentage: parseFloat(form.taxPercentage || '0'),
        imageUrls: form.imageUrls
          ? form.imageUrls.split(',').map(u => u.trim()).filter(u => u.length > 0)
          : []
      };

      if (editProduct) {
        await ProductApi.update(editProduct.id, payload);
        showMsg('success', `Product "${form.name}" updated successfully.`);
      } else {
        await ProductApi.create(payload);
        showMsg('success', `Product "${form.name}" created successfully.`);
      }

      closeModal();
      onProductsUpdated();
    } catch (err: any) {
      // Prefer server-provided message and errors array
      const serverData = err?.response?.data
      const serverMsg = serverData?.message || serverData?.Message
      const serverErrors = serverData?.errors || serverData?.Errors
      if (serverMsg) {
        showMsg('error', serverMsg)
      } else if (Array.isArray(serverErrors) && serverErrors.length > 0) {
        showMsg('error', serverErrors.join('; '))
      } else {
        showMsg('error', 'Failed to save product. Check all required fields.')
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete "${name}"? This action is reversible (soft delete).`)) return;
    try {
      await ProductApi.delete(id);
      showMsg('success', `Product "${name}" deleted.`);
      onProductsUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to delete product.');
    }
  };

  const filteredProducts = products.filter(p =>
    p.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.sku?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.categoryName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Feedback message */}
      <AnimatePresence>
        {msg && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            className={`p-4 rounded-xl flex items-center gap-3 ${msg.type === 'success'
              ? 'bg-emerald-50 border border-emerald-200 text-emerald-700'
              : 'bg-red-50 border border-red-200 text-red-700'}`}
          >
            {msg.type === 'success' ? <CheckCircle className="w-5 h-5 flex-shrink-0" /> : <AlertCircle className="w-5 h-5 flex-shrink-0" />}
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
              placeholder="Search products by name, SKU, or category..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input-field pl-10"
            />
          </div>
          <button onClick={openAdd} className="btn-primary flex items-center space-x-2">
            <Plus className="w-5 h-5" />
            <span>Add Product</span>
          </button>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Product</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">SKU</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Price</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Stock</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Status</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredProducts.map((product) => (
                <tr key={product.id} className="border-b border-gray-50 hover:bg-gray-50/50">
                  <td className="py-3 px-4">
                    <div className="flex items-center space-x-3">
                      {product.productImages && product.productImages[0] ? (
                        <img src={product.productImages[0].imageUrl} alt="" className="w-10 h-10 rounded-lg object-cover" />
                      ) : (
                        <div className="w-10 h-10 rounded-lg bg-gray-100 flex items-center justify-center">
                          <Package className="w-5 h-5 text-gray-400" />
                        </div>
                      )}
                      <div>
                        <p className="font-medium text-gray-900 text-sm">{product.name}</p>
                        <p className="text-xs text-gray-500">{product.categoryName}</p>
                      </div>
                    </div>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-600 font-mono">{product.sku}</td>
                  <td className="py-3 px-4 text-sm font-semibold">
                    Rs. {(product.discountPrice || product.unitPrice).toLocaleString()}
                    {product.discountPrice && (
                      <span className="text-xs text-gray-400 line-through ml-1">Rs. {product.unitPrice.toLocaleString()}</span>
                    )}
                  </td>
                  <td className="py-3 px-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${product.stockQuantity <= product.reorderLevel
                      ? 'bg-red-100 text-red-700'
                      : product.stockQuantity === 0
                        ? 'bg-gray-100 text-gray-700'
                        : 'bg-green-100 text-green-700'
                      }`}>
                      {product.stockQuantity} units
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${product.stockQuantity === 0
                      ? 'bg-gray-100 text-gray-700'
                      : 'bg-blue-100 text-blue-700'
                      }`}>
                      {product.stockQuantity === 0 ? 'Out of Stock' : 'Active'}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex space-x-2">
                      <button
                        onClick={() => openEdit(product)}
                        className="p-1.5 text-gray-500 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                        title="Edit product"
                      >
                        <Edit className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(product.id, product.name)}
                        className="p-1.5 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        title="Delete product"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {filteredProducts.length === 0 && (
            <div className="py-12 text-center">
              <Package className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p className="text-gray-500">No products found.</p>
            </div>
          )}
        </div>
      </div>

      {/* Add/Edit Product Modal */}
      <AnimatePresence>
        {showModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm overflow-y-auto">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl w-full max-w-2xl shadow-2xl my-4"
            >
              <div className="flex justify-between items-center p-5 border-b border-gray-100 bg-gray-50 rounded-t-2xl">
                <h3 className="text-xl font-bold text-gray-900">
                  {editProduct ? 'Edit Product' : 'Add New Product'}
                </h3>
                <button onClick={closeModal} className="text-gray-400 hover:text-gray-600 transition-colors">
                  <X className="w-5 h-5" />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-5 space-y-4 max-h-[70vh] overflow-y-auto">
                <div className="grid grid-cols-2 gap-4">
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Product Name *</label>
                    <input name="name" value={form.name} onChange={handleChange} required className="input-field py-2" placeholder="e.g. Samsung Galaxy S24" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">SKU *</label>
                    <input name="sku" value={form.sku} onChange={handleChange} required className="input-field py-2" placeholder="e.g. SAM-S24-001" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
                    <select name="categoryId" value={form.categoryId} onChange={handleChange} className="input-field py-2">
                      <option value="">-- Select Category --</option>
                      {categories.map((c: any) => (
                        <option key={c.id} value={c.id}>{c.name}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Unit Price (Rs.) *</label>
                    <input name="unitPrice" type="number" min="0" step="0.01" value={form.unitPrice} onChange={handleChange} required className="input-field py-2" placeholder="0.00" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Purchase Price (Rs.)</label>
                    <input name="purchasePrice" type="number" min="0" step="0.01" value={form.purchasePrice} onChange={handleChange} className="input-field py-2" placeholder="0.00" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Discount Price (Rs.)</label>
                    <input name="discountPrice" type="number" min="0" step="0.01" value={form.discountPrice} onChange={handleChange} className="input-field py-2" placeholder="Leave blank for no discount" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Tax % </label>
                    <input name="taxPercentage" type="number" min="0" max="100" step="0.1" value={form.taxPercentage} onChange={handleChange} className="input-field py-2" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Stock Quantity *</label>
                    <input name="stockQuantity" type="number" min="0" value={form.stockQuantity} onChange={handleChange} required className="input-field py-2" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Reorder Level</label>
                    <input name="reorderLevel" type="number" min="0" value={form.reorderLevel} onChange={handleChange} className="input-field py-2" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Brand</label>
                    <input name="brandName" value={form.brandName} onChange={handleChange} className="input-field py-2" placeholder="e.g. Samsung" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Company</label>
                    <input name="companyName" value={form.companyName} onChange={handleChange} className="input-field py-2" placeholder="e.g. Samsung Electronics" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Model</label>
                    <input name="model" value={form.model} onChange={handleChange} className="input-field py-2" placeholder="e.g. SM-S921B" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Delivery Period</label>
                    <input name="deliveryPeriod" value={form.deliveryPeriod} onChange={handleChange} className="input-field py-2" placeholder="e.g. 3-5 business days" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Warranty Info</label>
                    <input name="warrantyInfo" value={form.warrantyInfo} onChange={handleChange} className="input-field py-2" placeholder="e.g. 1 Year Manufacturer" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Weight</label>
                    <input name="weight" value={form.weight} onChange={handleChange} className="input-field py-2" placeholder="e.g. 195g" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Dimensions</label>
                    <input name="dimensions" value={form.dimensions} onChange={handleChange} className="input-field py-2" placeholder="e.g. 147 x 70 x 7.6 mm" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Tags</label>
                    <input name="tags" value={form.tags} onChange={handleChange} className="input-field py-2" placeholder="smartphone, flagship, 5G" />
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                    <textarea name="description" value={form.description} onChange={handleChange} className="input-field py-2 resize-none h-20" placeholder="Product description..." />
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Image URLs (comma-separated)</label>
                    <input name="imageUrls" value={form.imageUrls} onChange={handleChange} className="input-field py-2" placeholder="https://example.com/img1.jpg, https://example.com/img2.jpg" />
                  </div>
                </div>
                <div className="flex justify-end gap-3 pt-2 border-t border-gray-100">
                  <button type="button" onClick={closeModal} className="px-5 py-2.5 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-xl transition-colors">
                    Cancel
                  </button>
                  <button type="submit" disabled={saving} className="btn-primary px-6 py-2.5">
                    {saving ? 'Saving...' : editProduct ? 'Update Product' : 'Create Product'}
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

export default AdminProducts;
