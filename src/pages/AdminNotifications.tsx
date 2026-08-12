import React, { useEffect, useState } from 'react'
import { NotificationApi } from '../api/notification.api'
import { useAuth } from '../context/AuthContext'

const AdminNotifications: React.FC = () => {
  const [notifications, setNotifications] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const { user } = useAuth()

  const load = async () => {
    setLoading(true)
    try {
      const data = await NotificationApi.getHistory(200)
      // Filter notifications so users only see notifications relevant to them.
      // Backend may return global notifications; filter defensively by recipient/target fields.
      const filtered = (data || []).filter((n: any) => {
        if (!user) return false
        if (user.role === 'Admin') return true // admins see all
        // if notification explicitly targets a user id
        if (n.recipientId && n.recipientId === user.id) return true
        if (n.targetUserId && n.targetUserId === user.id) return true
        // if notification targets roles
        if (n.targetRoles && Array.isArray(n.targetRoles) && n.targetRoles.includes(user.role)) return true
        // fallback: some notifications include a "forRole" field
        if (n.forRole && n.forRole === user.role) return true
        return false
      })
      setNotifications(filtered)
    } catch { }
    setLoading(false)
  }

  useEffect(() => { 
    load() 

    const handleNotification = () => {
      load();
    };
    window.addEventListener('NotificationReceived', handleNotification);
    return () => window.removeEventListener('NotificationReceived', handleNotification);
  }, [user])

  return (
    <div className="page-container py-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold mb-4">Notification History</h1>
        <div className="card">
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading notifications...</div>
          ) : (
            <div className="space-y-3">
              {notifications.map(n => (
                <div key={n.id} className={`p-4 rounded-lg border ${n.isRead ? 'bg-white' : 'bg-blue-50'}`}>
                  <div className="flex justify-between items-start">
                    <div>
                      <div className="font-semibold">{n.title}</div>
                      <div className="text-sm text-gray-600">{n.message}</div>
                    </div>
                    <div className="text-xs text-gray-400">{new Date(n.createdAt).toLocaleString()}</div>
                  </div>
                  <div className="mt-2 text-xs text-gray-500">Type: {n.notificationType}</div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default AdminNotifications
