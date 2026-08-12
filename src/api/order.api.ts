import api from './client'
import { OrderResponse } from '../types'

export const OrderApi = {
  getAll: async (): Promise<OrderResponse[]> => {
    const res = await api.get('/orders')
    return res.data.data as OrderResponse[]
  },

  // For buyers only - fetches only their own orders securely from the backend
  getMyOrders: async (): Promise<OrderResponse[]> => {
    const res = await api.get('/orders/my')
    return res.data.data as OrderResponse[]
  },

  // For salesmen only - fetches only orders they created
  getMySalesmanOrders: async (): Promise<OrderResponse[]> => {
    const res = await api.get('/orders/mine')
    return res.data.data as OrderResponse[]
  },

  getById: async (id: string): Promise<OrderResponse> => {
    const res = await api.get(`/orders/${id}`)
    return res.data.data as OrderResponse
  },

  create: async (payload: {
    customerId?: string
    employeeId?: string
    items: { productId: string; quantity: number }[]
    orderType?: string
    deliveryCity: string
    deliveryAddress: string
    paymentMethod?: string
    deliveryPeriod?: string
  }): Promise<OrderResponse> => {
    const res = await api.post('/orders', payload)
    return res.data.data as OrderResponse
  },

  getByNumber: async (orderNumber: string): Promise<OrderResponse> => {
    const res = await api.get(`/orders/number/${orderNumber}`)
    return res.data.data as OrderResponse
  },

  approve: async (id: string): Promise<OrderResponse> => {
    const res = await api.post(`/orders/${id}/approve`)
    return res.data.data as OrderResponse
  },

  reject: async (id: string): Promise<OrderResponse> => {
    const res = await api.post(`/orders/${id}/reject`)
    return res.data.data as OrderResponse
  },

  cancel: async (id: string): Promise<OrderResponse> => {
    const res = await api.post(`/orders/${id}/cancel`)
    return res.data.data as OrderResponse
  },

  receive: async (id: string): Promise<OrderResponse> => {
    const res = await api.post(`/orders/${id}/receive`)
    return res.data.data as OrderResponse
  },

  updateStatus: async (id: string, status: string): Promise<OrderResponse> => {
    const res = await api.put(`/orders/${id}/status`, { status })
    return res.data.data as OrderResponse
  },

  dispatch: async (id: string, courierType: string): Promise<void> => {
    await api.post(`/orders/${id}/dispatch`, JSON.stringify(courierType), {
      headers: { 'Content-Type': 'application/json' }
    })
  },
}
