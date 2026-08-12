import api from './client'

export const SalaryApi = {
  setSalary: async (employeeId: string, payload: { monthlySalary: number }) => {
    const res = await api.post(`/salary/set/${employeeId}`, payload)
    return res.data
  },

  getSummary: async (employeeId: string, month?: number, year?: number) => {
    const q = [] as string[]
    if (month) q.push(`month=${month}`)
    if (year) q.push(`year=${year}`)
    const qs = q.length ? `?${q.join('&')}` : ''
    const res = await api.get(`/salary/summary/${employeeId}${qs}`)
    return res.data.data
  },

  getAll: async () => {
    const res = await api.get('/salary/all')
    return res.data.data
  }
}
