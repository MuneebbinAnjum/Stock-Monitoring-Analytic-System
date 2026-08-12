import api from './client'

export const SettingsApi = {
  getAll: async () => {
    const res = await api.get('/settings')
    return res.data.data
  },

  getPublic: async (key: string) => {
    const res = await api.get(`/settings/public/${key}`)
    return res.data.data
  },

  update: async (key: string, data: { value: string }) => {
    const res = await api.put(`/settings/${key}`, data)
    return res.data.data
  }
}
