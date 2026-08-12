import api from './client'

export const CategoryApi = {
  getAll: async () => {
    const res = await api.get('/categories')
    return res.data.data
  },

  getById: async (id: string) => {
    const res = await api.get(`/categories/${id}`)
    return res.data.data
  },

  create: async (data: any) => {
    const res = await api.post('/categories', data)
    return res.data.data
  },

  update: async (id: string, data: any) => {
    const res = await api.put(`/categories/${id}`, data)
    return res.data.data
  },

  delete: async (id: string) => {
    const res = await api.delete(`/categories/${id}`)
    return res.data.data
  }
}
