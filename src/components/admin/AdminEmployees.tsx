import React, { useState } from 'react';
import { ProductApi } from '../../api/product.api';
import { CommissionApi } from '../../api/commission.api';
import { SalaryApi } from '../../api/salary.api';
import { Search, Shield, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { EmployeeApi } from '../../api/employee.api';

interface AdminEmployeesProps {
  pendingSalesman: any[];
  allEmployees: any[];
  onEmployeesUpdated: () => void;
}

const AdminEmployees: React.FC<AdminEmployeesProps> = ({ pendingSalesman, allEmployees, onEmployeesUpdated }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [tab, setTab] = useState<'pending' | 'all'>('pending');
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [loading, setLoading] = useState<string | null>(null);
  const [commissionModal, setCommissionModal] = useState<{ open: boolean; employee: any | null; products: any[]; commissions: Record<string, any> }>({ open: false, employee: null, products: [], commissions: {} });
  const [salaryLoading, setSalaryLoading] = useState<string | null>(null);
  const [salarySummary, setSalarySummary] = useState<any | null>(null);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 4000);
  };

  const handleSetSalary = async (id: string, name: string) => {
    const val = window.prompt(`Enter monthly salary for ${name} (PKR)`);
    if (val === null) return;
    const num = parseFloat(val || '0');
    if (isNaN(num) || num < 0) {
      showMsg('error', 'Invalid salary amount');
      return;
    }
    setSalaryLoading(id);
    try {
      await SalaryApi.setSalary(id, { monthlySalary: num });
      showMsg('success', `Salary updated for ${name}`);
      onEmployeesUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to set salary');
    } finally {
      setSalaryLoading(null);
    }
  };

  const handleViewSalarySummary = async (id: string) => {
    try {
      const data = await SalaryApi.getSummary(id);
      setSalarySummary(data);
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to load salary summary');
    }
  };

  const handleOpenCommissionModal = async (employee: any) => {
    try {
      const products = await ProductApi.getAll();
      const commissions = await CommissionApi.getByEmployee(employee.id);
      const map: Record<string, any> = {};
      commissions.forEach((c: any) => { map[c.productId] = c; });
      setCommissionModal({ open: true, employee, products, commissions: map });
    } catch (err: any) {
      showMsg('error', 'Failed to load products or commissions');
    }
  };

  const handleSaveCommission = async (productId: string, value: string) => {
    const emp = commissionModal.employee;
    if (!emp) return;
    const num = parseFloat(value || '0');
    if (isNaN(num) || num < 0 || num > 100) { showMsg('error', 'Invalid commission percentage'); return; }

    const existing = commissionModal.commissions[productId];
    try {
      if (existing) {
        await CommissionApi.update(existing.id, { commissionPercentage: num });
        existing.commissionPercentage = num;
      } else {
        const created = await CommissionApi.create({ employeeId: emp.id, productId, commissionPercentage: num });
        commissionModal.commissions[productId] = created;
      }
      showMsg('success', 'Commission saved');
      setCommissionModal({ ...commissionModal });
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to save commission');
    }
  };

  const handleApprove = async (id: string, name: string) => {
    setLoading(id);
    try {
      await EmployeeApi.approve(id);
      showMsg('success', `${name} approved successfully. They can now log in.`);
      onEmployeesUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to approve employee.');
    } finally {
      setLoading(null);
    }
  };

  const handleReject = async (id: string, name: string) => {
    setLoading(id);
    try {
      await EmployeeApi.reject(id);
      showMsg('success', `${name}'s application has been rejected.`);
      onEmployeesUpdated();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to reject employee.');
    } finally {
      setLoading(null);
    }
  };

  const filteredEmployees = (tab === 'pending' ? pendingSalesman : allEmployees).filter(e => 
    e.name?.toLowerCase().includes(searchTerm.toLowerCase()) || 
    e.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    e.email?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <AnimatePresence>
        {msg && (
          <motion.div
            initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            className={`p-4 rounded-xl flex items-center gap-3 ${
              msg.type === 'success' ? 'bg-emerald-50 border border-emerald-200 text-emerald-700' : 'bg-red-50 border border-red-200 text-red-700'
            }`}
          >
            {msg.type === 'success' ? <CheckCircle className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
            <span className="text-sm font-medium">{msg.text}</span>
          </motion.div>
        )}
      </AnimatePresence>
      <div className="card">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div className="flex space-x-2 border border-gray-200 rounded-lg p-1 bg-gray-50">
            <button
              onClick={() => setTab('pending')}
              className={`px-4 py-1.5 text-sm font-medium rounded-md transition-colors ${tab === 'pending' ? 'bg-white shadow-sm text-gray-900' : 'text-gray-500 hover:text-gray-700'}`}
            >
              Pending Approvals ({pendingSalesman.length})
            </button>
            <button
              onClick={() => setTab('all')}
              className={`px-4 py-1.5 text-sm font-medium rounded-md transition-colors ${tab === 'all' ? 'bg-white shadow-sm text-gray-900' : 'text-gray-500 hover:text-gray-700'}`}
            >
              All Employees ({allEmployees.length})
            </button>
          </div>
          
          <div className="relative w-full sm:max-w-xs">
            <Search className="absolute left-3 top-2.5 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search employees..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input-field pl-10 py-2 text-sm"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Employee</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Role</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Status</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Date</th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredEmployees.map((emp) => (
                <tr key={emp.id} className="border-b border-gray-50 hover:bg-gray-50/50">
                  <td className="py-3 px-4">
                    <p className="font-medium text-gray-900">{emp.name || emp.fullName}</p>
                    <p className="text-sm text-gray-500">{emp.email}</p>
                  </td>
                  <td className="py-3 px-4">
                    <span className="px-2.5 py-1 bg-purple-50 text-purple-700 rounded-full text-xs font-semibold">
                      {emp.role || 'Salesman'}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${
                      emp.approvalStatus === 'Approved' ? 'bg-green-100 text-green-700' :
                      emp.approvalStatus === 'Rejected' ? 'bg-red-100 text-red-700' :
                      'bg-amber-100 text-amber-700'
                    }`}>
                      {emp.approvalStatus || 'Pending'}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-500">
                    {new Date(emp.createdAt || emp.hireDate).toLocaleDateString()}
                  </td>
                  <td className="py-3 px-4">
                    {(emp.approvalStatus === 'Pending' || !emp.approvalStatus) ? (
                      <div className="flex space-x-2">
                        <button
                          onClick={() => handleApprove(emp.id, emp.name || emp.fullName)}
                          disabled={loading === emp.id}
                          className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-100 text-emerald-700 hover:bg-emerald-200 rounded-lg transition-colors text-xs font-semibold disabled:opacity-50"
                        >
                          <CheckCircle className="w-3.5 h-3.5" />
                          {loading === emp.id ? 'Saving...' : 'Approve'}
                        </button>
                        <button
                          onClick={() => handleReject(emp.id, emp.name || emp.fullName)}
                          disabled={loading === emp.id}
                          className="flex items-center gap-1.5 px-3 py-1.5 bg-red-100 text-red-700 hover:bg-red-200 rounded-lg transition-colors text-xs font-semibold disabled:opacity-50"
                        >
                          <XCircle className="w-3.5 h-3.5" />
                          Reject
                        </button>
                      </div>
                    ) : (
                      <div className="flex items-center space-x-2">
                        <button
                          onClick={() => handleSetSalary(emp.id, emp.name || emp.fullName)}
                          disabled={salaryLoading === emp.id}
                          className="px-3 py-1.5 bg-sky-50 text-sky-700 hover:bg-sky-100 rounded-md text-xs font-semibold disabled:opacity-50"
                        >
                          {salaryLoading === emp.id ? 'Saving...' : 'Set Salary'}
                        </button>

                        <button
                          onClick={() => handleViewSalarySummary(emp.id)}
                          className="px-3 py-1.5 bg-amber-50 text-amber-700 hover:bg-amber-100 rounded-md text-xs font-semibold"
                        >
                          View Summary
                        </button>

                        <button
                          onClick={() => handleOpenCommissionModal(emp)}
                          className="px-3 py-1.5 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-md text-xs font-semibold"
                        >
                          Manage Commissions
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {filteredEmployees.length === 0 && (
            <div className="py-12 flex flex-col items-center justify-center text-gray-400">
              <Shield className="w-12 h-12 mb-3 text-gray-300" />
              <p>No employees found in this view.</p>
            </div>
          )}
        </div>
      </div>
      {/* Commission modal */}
      {commissionModal.open && commissionModal.employee && (
        <div className="fixed inset-0 z-50 flex items-start justify-center p-6">
          <div className="absolute inset-0 bg-black/40" onClick={() => setCommissionModal({ open: false, employee: null, products: [], commissions: {} })} />
          <div className="bg-white rounded-2xl shadow-xl max-w-4xl w-full p-6 relative z-10">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-bold">Manage Commissions for {commissionModal.employee.fullName || commissionModal.employee.name}</h3>
              <button className="text-sm text-gray-500" onClick={() => setCommissionModal({ open: false, employee: null, products: [], commissions: {} })}>Close</button>
            </div>
            <div className="space-y-3 max-h-[60vh] overflow-y-auto">
              {commissionModal.products.map((p: any) => {
                const existing = commissionModal.commissions[p.id] || commissionModal.commissions[p.productId];
                const val = existing ? existing.commissionPercentage : '';
                return (
                  <div key={p.id} className="flex items-center justify-between border-b border-gray-100 py-3">
                    <div>
                      <div className="font-medium">{p.name}</div>
                      <div className="text-sm text-gray-500">SKU: {p.sku}</div>
                    </div>
                    <div className="flex items-center space-x-2">
                      <input defaultValue={val} placeholder="%" id={`comm-${p.id}`} className="input-field w-24" />
                      <button onClick={async () => {
                        const el = document.getElementById(`comm-${p.id}`) as HTMLInputElement | null;
                        const v = el?.value || '';
                        await handleSaveCommission(p.id, v);
                      }} className="px-3 py-1.5 bg-primary-600 text-white rounded-md text-sm">Save</button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      {/* Salary summary modal */}
      {salarySummary && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-6">
          <div className="absolute inset-0 bg-black/40" onClick={() => setSalarySummary(null)} />
          <div className="bg-white rounded-2xl shadow-xl max-w-md w-full p-6 relative z-10">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-bold">Salary Summary - {salarySummary.salesmanName || salarySummary.salesmanName}</h3>
              <button className="text-sm text-gray-500" onClick={() => setSalarySummary(null)}>Close</button>
            </div>
            <div className="space-y-3">
              <div className="flex justify-between"><span>Month</span><span>{salarySummary.month}/{salarySummary.year}</span></div>
              <div className="flex justify-between"><span>Monthly Salary</span><span>Rs. {salarySummary.monthlySalary?.toLocaleString()}</span></div>
              <div className="flex justify-between"><span>Total Commission Earned</span><span>Rs. {salarySummary.totalCommissionEarned?.toLocaleString()}</span></div>
              <div className="flex justify-between font-bold"><span>Total Amount Due</span><span>Rs. {salarySummary.totalAmountDue?.toLocaleString()}</span></div>
              <div className="pt-4">
                <button onClick={() => setSalarySummary(null)} className="px-4 py-2 bg-primary-600 text-white rounded-md">Close</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminEmployees;
