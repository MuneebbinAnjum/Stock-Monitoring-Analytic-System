import api from './client'

export const ComplaintApi = {
  getAll: async () => {
    const res = await api.get('/complaints')
    return res.data.data
  },

  getMyComplaints: async () => {
    const res = await api.get('/complaints/my')
    return res.data.data
  },

  create: async (data: any) => {
    const res = await api.post('/complaints', data)
    return res.data.data
  },

  updateStatus: async (id: string, data: any) => {
    const res = await api.put(`/complaints/${id}/status`, data)
    return res.data.data
  }
,

  getById: async (id: string) => {
    const res = await api.get(`/complaints/${id}`)
    return res.data.data
  },

  postMessage: async (id: string, data: { message: string }) => {
    const res = await api.post(`/complaints/${id}/messages`, data)
    return res.data.data
  }
}
