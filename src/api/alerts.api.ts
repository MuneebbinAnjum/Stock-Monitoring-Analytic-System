import api from './client'

export const AlertsApi = {
  getAll: async () => {
    const res = await api.get('/alerts')
    return res.data.data
  },

  resolve: async (id: string) => {
    const res = await api.post(`/alerts/${id}/resolve`)
    return res.data
  },
}
