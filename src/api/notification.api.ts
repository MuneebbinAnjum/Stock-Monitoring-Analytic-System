import api from './client'

export const NotificationApi = {
  getAll: async (unreadOnly = false, limit = 50) => {
    const res = await api.get(`/notifications?unreadOnly=${unreadOnly}&limit=${limit}`)
    return res.data.data
  },

  getCount: async () => {
    const res = await api.get('/notifications/count')
    return res.data.data
  },

  markOneRead: async (id: string) => {
    const res = await api.post(`/notifications/${id}/read`)
    return res.data
  },

  markAllRead: async () => {
    const res = await api.post('/notifications/read-all')
    return res.data
  },

  getHistory: async (limit = 200) => {
    const res = await api.get(`/notifications/history?limit=${limit}`)
    return res.data.data
  }
}
