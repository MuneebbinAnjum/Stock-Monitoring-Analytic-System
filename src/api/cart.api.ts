import api from './client'

export const CartApi = {
  getCart: async () => {
    const res = await api.get('/cart')
    return res.data.data
  },

  addItem: async (data: { productId: string; quantity: number }) => {
    const res = await api.post('/cart', data)
    return res.data
  },

  updateQuantity: async (id: string, quantity: number) => {
    const res = await api.put(`/cart/${id}`, { quantity })
    return res.data
  },

  removeItem: async (id: string) => {
    const res = await api.delete(`/cart/${id}`)
    return res.data
  },

  clearCart: async () => {
    const res = await api.delete('/cart/clear')
    return res.data
  }
}
