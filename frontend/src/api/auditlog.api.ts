import api from './client'

export const AuditLogApi = {
  getAll: async (params?: { entityName?: string; action?: string; page?: number; pageSize?: number }) => {
    const res = await api.get('/auditlogs', { params })
    return res.data.data
  }
}
