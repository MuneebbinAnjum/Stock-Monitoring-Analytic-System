import api from './client'

export const EmployeeApi = {
  getAll: async () => {
    const res = await api.get('/employees')
    return res.data.data
  },

  getPending: async () => {
    const res = await api.get('/employees/pending')
    return res.data.data
  },

  approve: async (id: string) => {
    const res = await api.post(`/employees/${id}/approve`)
    return res.data
  },

  reject: async (id: string) => {
    const res = await api.post(`/employees/${id}/reject`)
    return res.data
  },
}
