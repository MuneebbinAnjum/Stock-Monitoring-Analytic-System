import React, { useState } from 'react'
import { AuthApi } from '../api/auth.api'

interface Props { open: boolean; onClose: () => void }

const ChangePasswordModal: React.FC<Props> = ({ open, onClose }) => {
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  if (!open) return null

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text })
    setTimeout(() => setMsg(null), 4000)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!currentPassword || !newPassword) { showMsg('error', 'Please fill all fields'); return }
    if (newPassword !== confirmPassword) { showMsg('error', 'New passwords do not match'); return }
    setLoading(true)
    try {
      await AuthApi.changePassword({ currentPassword, newPassword })
      showMsg('success', 'Password changed')
      setTimeout(() => onClose(), 800)
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to change password')
    } finally { setLoading(false) }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-6">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="bg-white rounded-2xl shadow-xl max-w-md w-full p-6 relative z-10">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-bold">Change Password</h3>
          <button className="text-sm text-gray-500" onClick={onClose}>Close</button>
        </div>
        {msg && <div className={`p-2 mb-3 rounded ${msg.type === 'success' ? 'bg-emerald-50 text-emerald-800' : 'bg-red-50 text-red-800'}`}>{msg.text}</div>}
        <form onSubmit={handleSubmit} className="space-y-3">
          <div>
            <label className="text-sm text-gray-600">Current Password</label>
            <input type="password" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} className="input-field w-full mt-1" />
          </div>
          <div>
            <label className="text-sm text-gray-600">New Password</label>
            <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} className="input-field w-full mt-1" />
          </div>
          <div>
            <label className="text-sm text-gray-600">Confirm New Password</label>
            <input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} className="input-field w-full mt-1" />
          </div>
          <div className="flex justify-end">
            <button type="submit" disabled={loading} className="px-4 py-2 bg-primary-600 text-white rounded-md">{loading ? 'Saving...' : 'Change'}</button>
          </div>
        </form>
      </div>
    </div>
  )
}

export default ChangePasswordModal
