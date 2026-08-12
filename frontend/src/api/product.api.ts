import api from './client'

export const ProductApi = {
  getAll: async () => {
    const res = await api.get('/products')
    return res.data.data
  },

  getById: async (id: string) => {
    const res = await api.get(`/products/${id}`)
    return res.data.data
  },

  search: async (query: string) => {
    const res = await api.get(`/products/search?q=${encodeURIComponent(query)}`)
    return res.data.data
  },

  create: async (data: any) => {
    const res = await api.post('/products', data)
    return res.data.data
  },

  update: async (id: string, data: any) => {
    const res = await api.put(`/products/${id}`, data)
    return res.data.data
  },

  delete: async (id: string) => {
    const res = await api.delete(`/products/${id}`)
    return res.data.data
  }
}
