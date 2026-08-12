import api from './client';
import { cachedGet, fetchTimeSeriesInChunks } from './cache'

export class ReportApi {
  static async getSalesSummary(startDate: Date, endDate: Date) {
    // Use chunked fetching for large ranges and cache responses
    const startIso = startDate.toISOString()
    const endIso = endDate.toISOString()
    const merged = await fetchTimeSeriesInChunks('/reports/sales', startIso, endIso, 90)
    return merged
  }

  static async getInventoryReport() {
    const response = await api.get('/reports/inventory');
    return response.data.data;
  }

  static async getRevenueBreakdown(groupBy: 'category' | 'supplier' | 'employee') {
    const resp = await cachedGet('/reports/revenue', { groupBy })
    return resp.data.data
  }

  static async getSalesByLocation() {
    const resp = await cachedGet('/reports/sales-by-location')
    return resp.data.data
  }

  static async exportReport(reportType: string, format: 'csv' | 'excel' = 'csv', startDate?: Date, endDate?: Date) {
    const response = await api.get(`/reports/export/${reportType}`, {
      params: {
        format,
        start: startDate?.toISOString(),
        end: endDate?.toISOString()
      }
    });
    return response.data;
  }

  static async getAgentEarnings(salesmanId: string, days: number = 7) {
    const response = await api.get(`/commissions/salesman/${salesmanId}/earnings`, {
      params: { days }
    });
    return response.data.data;
  }
}
