import api from './client'
import { cachedGet, fetchTimeSeriesInChunks } from './cache'

export const AnalyticsApi = {
  // Sales heatmap by city/province: returns counts/revenue keyed by region
  getSalesHeatmapByRegion: async (startDate?: string, endDate?: string) => {
    // Use cached fetch and chunking for large ranges
    if (startDate && endDate) {
      return await fetchTimeSeriesInChunks('/analytics/sales-heatmap', startDate, endDate)
    }
    const res = await cachedGet('/analytics/sales-heatmap')
    return res.data.data
  },

  // Top performing salesmen: by revenue or quantity
  getTopSalesmen: async (options?: { topN?: number; month?: number; year?: number; metric?: 'revenue' | 'quantity' }) => {
    const topN = options?.topN ?? 10
    const q: string[] = [`topN=${topN}`]
    if (options?.month) q.push(`month=${options.month}`)
    if (options?.year) q.push(`year=${options.year}`)
    if (options?.metric) q.push(`metric=${options.metric}`)
    const qs = `?${q.join('&')}`
    const res = await api.get(`/analytics/top-salesmen${qs}`)
    return res.data.data
  },

  // Top selling products & categories
  getTopProducts: async (options?: { topN?: number; month?: number; year?: number; categoryId?: string }) => {
    const topN = options?.topN ?? 10
    const q: string[] = [`topN=${topN}`]
    if (options?.month) q.push(`month=${options.month}`)
    if (options?.year) q.push(`year=${options.year}`)
    if (options?.categoryId) q.push(`category=${encodeURIComponent(options.categoryId)}`)
    const qs = `?${q.join('&')}`
    const res = await api.get(`/analytics/top-products${qs}`)
    return res.data.data
  },

  // Order status distribution (Pending, Processing, Shipped, Delivered, Cancelled)
  getOrderStatusDistribution: async (startDate?: string, endDate?: string) => {
    const q: string[] = []
    if (startDate) q.push(`start=${encodeURIComponent(startDate)}`)
    if (endDate) q.push(`end=${encodeURIComponent(endDate)}`)
    const qs = q.length ? `?${q.join('&')}` : ''
    const res = await api.get(`/analytics/order-status-distribution${qs}`)
    return res.data.data
  },

  // Demand forecasting vs actual sales
  getForecastVsActual: async (options?: { startDate?: string; endDate?: string; granularity?: 'day' | 'week' | 'month' }) => {
    // For large date ranges, fetch in chunks and merge
    if (options?.startDate && options?.endDate) {
      const params = options.granularity ? { granularity: options.granularity } : undefined
      return await fetchTimeSeriesInChunks('/analytics/forecast-vs-actual', options.startDate, options.endDate, 90, params)
    }
    const res = await cachedGet('/analytics/forecast-vs-actual', options)
    return res.data.data
  }
}
