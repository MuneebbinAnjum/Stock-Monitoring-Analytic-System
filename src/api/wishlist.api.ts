import api from './client'

export const WishlistApi = {
  getWishlist: async () => {
    const res = await api.get('/wishlist')
    return res.data.data
  },

  addItem: async (productId: string) => {
    const res = await api.post('/wishlist', { productId })
    return res.data
  },

  removeItem: async (productId: string) => {
    const res = await api.delete(`/wishlist/${productId}`)
    return res.data
  }
}
