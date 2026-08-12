import api from './client'

export const CustomerApi = {
  getAll: async () => {
    const res = await api.get('/customers')
    return res.data.data
  },

  getById: async (id: string) => {
    const res = await api.get(`/customers/${id}`)
    return res.data.data
  },

  create: async (payload: {
    fullName: string
    email: string
    phone?: string
    city?: string
    province?: string
    password?: string
  }) => {
    // Generate a default password if not provided
    const data = {
      ...payload,
      password: payload.password || 'everyonelovesallah'
    }
    const res = await api.post('/customers', data)
    return res.data.data
  }
}
