import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AuthApi } from '../api/auth.api'
import { Check } from 'lucide-react'

const ChangePassword: React.FC = () => {
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text })
    setTimeout(() => setMsg(null), 4000)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!currentPassword || !newPassword) {
      showMsg('error', 'Please fill all fields')
      return
    }
    if (newPassword !== confirmPassword) {
      showMsg('error', 'New passwords do not match')
      return
    }
    setLoading(true)
    try {
      await AuthApi.changePassword({ currentPassword, newPassword })
      showMsg('success', 'Password changed successfully')
      setTimeout(() => navigate('/'), 1000)
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to change password')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page-container min-h-screen py-12">
      <div className="max-w-md mx-auto">
        {msg && (
          <div className={`p-3 rounded mb-4 ${msg.type === 'success' ? 'bg-emerald-50 text-emerald-800' : 'bg-red-50 text-red-800'}`}>
            {msg.text}
          </div>
        )}

        <div className="card">
          <h2 className="text-xl font-semibold mb-4">Change Password</h2>
          <form onSubmit={handleSubmit} className="space-y-4">
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
              <button type="submit" disabled={loading} className="px-4 py-2 bg-primary-600 text-white rounded-md">
                {loading ? 'Saving...' : 'Change Password'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}

export default ChangePassword
