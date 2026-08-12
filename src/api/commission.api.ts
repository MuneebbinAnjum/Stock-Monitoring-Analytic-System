import api from './client'

export const CommissionApi = {
  create: async (payload: { employeeId: string; productId: string; commissionPercentage: number }) => {
    const res = await api.post('/commissions', payload)
    return res.data.data
  },

  getByEmployee: async (employeeId: string) => {
    const res = await api.get(`/commissions/employee/${employeeId}`)
    return res.data.data
  },

  update: async (id: string, payload: { commissionPercentage: number }) => {
    const res = await api.put(`/commissions/${id}`, payload)
    return res.data.data
  },

  delete: async (id: string) => {
    const res = await api.delete(`/commissions/${id}`)
    return res.data
  }
}
